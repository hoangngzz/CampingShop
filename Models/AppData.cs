using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.SessionState;
using Newtonsoft.Json;

namespace Demo_btl_10_10
{
    /// <summary>
    /// Kho dữ liệu dùng chung cho cả website (sản phẩm, khách hàng, đánh giá, đơn hàng, tin tức, lời nhắn liên hệ).
    /// Dữ liệu nằm trong Application để đọc nhanh; mỗi lần thêm/sửa/xóa đều ghi lại ra MỘT file chung App_Data/data.json,
    /// khi website khởi động thì nạp lại từ file đó (chưa có file thì dùng dữ liệu mẫu).
    /// Mọi thao tác đọc/ghi đều Lock/UnLock để nhiều người truy cập cùng lúc không bị lỗi.
    /// </summary>
    public static class AppData
    {
        public const string RoleAdmin = "admin";
        public const string RoleCustomer = "customer";

        const string KProducts = "MyShop.Products";
        const string KNextProductId = "MyShop.NextProductId";
        const string KCustomers = "MyShop.Customers";
        const string KNextCustomerId = "MyShop.NextCustomerId";
        const string KReviews = "MyShop.Reviews";
        const string KNextReviewId = "MyShop.NextReviewId";
        const string KOrders = "MyShop.Orders";
        const string KNextOrderId = "MyShop.NextOrderId";

        public const string StatusPending = "Chờ xử lý";
        public const string StatusShipping = "Đang giao";
        public const string StatusDone = "Đã giao";
        public const string StatusCancelled = "Đã hủy";

        public static readonly string[] OrderStatuses = { StatusPending, StatusShipping, StatusDone, StatusCancelled };

        public static readonly Dictionary<string, string> Categories = new Dictionary<string, string>
        {
            { "leu", "Lều cắm trại" },
            { "den", "Đèn chiếu sáng" },
            { "ba-lo", "Ba lô - túi" },
            { "thiet-bi", "Thiết bị cắm trại" },
            { "xe-dap", "Xe đạp" },
        };

        static readonly CultureInfo Vi = new CultureInfo("vi-VN");

        // Giữ tham chiếu tới Application ngay từ Application_Start để không phụ thuộc HttpContext.Current
        static HttpApplicationState _app;   //Biến tĩnh lưu đối tượng Application, gán trong Init.
                                            // Dấu gạch dưới đầu tên là quy ước cho biến riêng.
        static HttpApplicationState App
        {
            get { return _app ?? HttpContext.Current.Application; }
        }

        // ===== Dữ liệu lưu ra file =====
        class DataFile
        {
            public List<ShopProduct> Products { get; set; }
            public List<Customer> Customers { get; set; }
            public List<Review> Reviews { get; set; }
            public List<Order> Orders { get; set; }
        }

        static string DataPath
        {
            get { return HostingEnvironment.MapPath("~/App_Data/data.json"); }
        }

        /// <summary>Đọc file dữ liệu từ App_Data và giải tuần tự thành DataFile.</summary>
        /// <remarks>Trả về null nếu chưa có file hoặc file hỏng; file hỏng được đổi tên kèm thời gian
        /// để lần khởi động sau có thể dùng dữ liệu mẫu mà không ghi đè file lỗi.</remarks>
        static DataFile LoadData()
        {
            string path = DataPath;
            try
            {
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path, Encoding.UTF8);
                return JsonConvert.DeserializeObject<DataFile>(json);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Không đọc được data.json: " + ex.Message);
                try { File.Move(path, path + ".loi-" + DateTime.Now.ToString("yyyyMMddHHmmss")); }
                catch (Exception) { }
                return null;
            }
        }

        /// <summary>
        /// Ghi toàn bộ dữ liệu ra file. Phải được gọi khi đang giữ App.Lock().
        /// Ghi file tạm rồi thay thế để không hỏng file cũ nếu lỗi giữa chừng.
        /// </summary>
        static void Persist()
        {
            try
            {
                DataFile data = new DataFile
                {
                    Products = (List<ShopProduct>)App[KProducts],
                    Customers = (List<Customer>)App[KCustomers],
                    Reviews = (List<Review>)App[KReviews],
                    Orders = (List<Order>)App[KOrders]
                };

                string path = DataPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, JsonConvert.SerializeObject(data, Formatting.Indented), new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                // Không lưu được file thì dữ liệu vẫn dùng được trong bộ nhớ, chỉ là sẽ mất khi khởi động lại
                Trace.TraceError("Không lưu được data.json: " + ex.Message);
            }
        }

        // Tạo danh sách tài khoản mặc định tối thiểu gồm một quản trị viên với mật khẩu được băm,
        // dùng khi kho dữ liệu chưa có tài khoản hợp lệ.
        static List<Customer> DefaultCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    FullName = "Quản trị viên",
                    Email = "admin@mycamp.vn",
                    Username = "admin",
                    PasswordHash = Hash("admin"),
                    Role = RoleAdmin,
                    CreatedAt = DateTime.Now
                }
            };
        }

        // ===== Khởi tạo (gọi từ Application_Start trong Global.asax.cs) =====
        // Gắn vùng Application, khóa dữ liệu trong suốt quá trình nạp/khởi tạo, chọn dữ liệu từ file
        // hoặc dữ liệu mẫu, thiết lập các danh sách và ID kế tiếp rồi ghi snapshot ban đầu ra đĩa.
        public static void Init(HttpApplicationState app)
        {
            _app = app;
            app.Lock();
            try
            {
                DataFile data = LoadData();
                if (data == null) data = new DataFile();

                // Chưa có dữ liệu nào thì dùng dữ liệu mẫu (sản phẩm, đánh giá, tài khoản admin)
                List<ShopProduct> products = data.Products ?? SeedProducts();
                List<Customer> customers = (data.Customers != null && data.Customers.Count > 0)
                    ? data.Customers : DefaultCustomers();
                List<Review> reviews = data.Reviews ?? SeedReviews();
                List<Order> orders = data.Orders ?? new List<Order>();

                app[KProducts] = products;
                app[KNextProductId] = (products.Count == 0 ? 0 : products.Max(p => p.Id)) + 1;

                app[KCustomers] = customers;
                app[KNextCustomerId] = customers.Max(c => c.Id) + 1;

                app[KReviews] = reviews;
                app[KNextReviewId] = (reviews.Count == 0 ? 0 : reviews.Max(r => r.Id)) + 1;

                app[KOrders] = orders;
                app[KNextOrderId] = (orders.Count == 0 ? 0 : orders.Max(o => o.Id)) + 1;

                // Tạo file ngay lần chạy đầu để chắc chắn ghi được
                Persist();
            }
            finally
            {
                app.UnLock();
            }
        }

        // ===== Hàm băm mật khẩu =====
        // Băm mật khẩu UTF-8 bằng SHA-256 và trả về chuỗi hexadecimal 64 ký tự để lưu/so sánh,
        // không lưu mật khẩu rõ trong dữ liệu ứng dụng.
        public static string Hash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // Định dạng số nguyên theo văn hóa Việt Nam với dấu phân cách hàng nghìn và hậu tố đồng.
        public static string Money(long n)  //"N0" là số có dấu ngăn cách hàng nghìn, 0 chữ số thập phân
        {                                   //. Vi là văn hóa Việt Nam nên dấu ngăn cách là dấu chấm.
            return n.ToString("N0", Vi) + "đ";
        }

        // Đổi mã danh mục nội bộ thành tên hiển thị; trả về mã gốc nếu key null hoặc không tồn tại.
        public static string CatName(string key)
        {
            string name;
            return key != null && Categories.TryGetValue(key, out name) ? name : key;
        }

        /// <summary>Chữ thường + bỏ dấu để gõ "leu" vẫn tìm ra "Lều".</summary>
        // Chuẩn hóa chuỗi phục vụ tìm kiếm: chuyển chữ thường, tách dấu Unicode và đổi đ thành d.
        public static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            string d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Replace('đ', 'd');
        }

        // Kiểm tra Session có role admin hay không, đồng thời xử lý an toàn trường hợp Session null.
        public static bool IsAdmin(HttpSessionState session)
        {
            return session != null &&
                   string.Equals(session["Role"] as string, RoleAdmin, StringComparison.OrdinalIgnoreCase);
        }

        // ===== Sản phẩm =====
        // Lấy danh sách sản phẩm gốc đang được lưu trong vùng Application.
        static List<ShopProduct> ProductList()
        {
            return (List<ShopProduct>)App[KProducts];
        }

        // Đọc toàn bộ sản phẩm dưới khóa Application, khóa truy cập trong lúc đọc và trả về các bản sao
        // để mã gọi không thể thay đổi trực tiếp dữ liệu dùng chung.
        public static List<ShopProduct> GetProducts()
        {
            App.Lock();
            try { return ProductList().Select(p => p.Clone()).ToList(); }   //Với mỗi sản phẩm gốc tạo một bản sao, gom thành danh sách mới. 
            finally { App.UnLock(); }                                       //Trang nhận được danh sách riêng, sửa thoải mái không ảnh hưởng dữ liệu chung.
        }

        // Tìm một sản phẩm theo ID dưới khóa Application và trả về bản sao độc lập, hoặc null nếu không có.
        public static ShopProduct GetProduct(int id)
        {
            App.Lock();
            try
            {
                ShopProduct p = ProductList().FirstOrDefault(x => x.Id == id);
                return p == null ? null : p.Clone();
            }
            finally { App.UnLock(); }
        }

        /// <summary>Id = 0 là thêm mới, Id &gt; 0 là cập nhật.</summary>
        // Thêm sản phẩm mới với ID kế tiếp hoặc thay thế sản phẩm cùng ID, sau đó persist toàn bộ dữ liệu.
        public static void SaveProduct(ShopProduct product)
        {
            App.Lock();
            try
            {
                List<ShopProduct> list = ProductList();
                if (product.Id <= 0)
                {
                    int next = (int)App[KNextProductId];
                    product.Id = next;
                    App[KNextProductId] = next + 1;
                    list.Add(product.Clone());
                }
                else
                {
                    int idx = list.FindIndex(x => x.Id == product.Id);
                    if (idx >= 0) list[idx] = product.Clone();
                    else list.Add(product.Clone());
                }
                Persist();
            }
            finally { App.UnLock(); }
        }

        // Xóa sản phẩm theo ID, xóa luôn các đánh giá liên quan và persist nếu có thay đổi; trả về
        // true khi thực sự xóa được ít nhất một sản phẩm.
        public static bool DeleteProduct(int id)
        {
            App.Lock();
            try
            {
                bool removed = ProductList().RemoveAll(x => x.Id == id) > 0;
                if (removed)
                {
                    ReviewList().RemoveAll(r => r.ProductId == id);
                    Persist();
                }
                return removed;
            }
            finally { App.UnLock(); }
        }

        // ===== Tài khoản (khách hàng và quản trị viên) =====
        // Lấy danh sách Customer gốc từ vùng Application để các phương thức tài khoản dùng nội bộ.
        static List<Customer> CustomerList()
        {
            return (List<Customer>)App[KCustomers];
        }

        // Đọc tất cả tài khoản dưới khóa Application và trả về bản sao của từng tài khoản.
        public static List<Customer> GetCustomers()
        {
            App.Lock();
            try { return CustomerList().Select(c => c.Clone()).ToList(); }
            finally { App.UnLock(); }
        }

        // Tìm tài khoản theo ID, trả về bản sao để bảo vệ dữ liệu gốc hoặc null nếu không tìm thấy.
        public static Customer GetCustomer(int id)
        {
            App.Lock();
            try
            {
                Customer c = CustomerList().FirstOrDefault(x => x.Id == id);
                return c == null ? null : c.Clone();
            }
            finally { App.UnLock(); }
        }

        /// <summary>Trả về tài khoản nếu đúng tên đăng nhập + mật khẩu, ngược lại trả về null.</summary>
        // Băm mật khẩu nhận vào, so sánh không phân biệt hoa thường cho username và trả về bản sao
        // tài khoản phù hợp khi cả username lẫn hash đều khớp.
        public static Customer Authenticate(string username, string password)
        {
            string hash = Hash(password);
            App.Lock();
            try
            {
                Customer c = CustomerList().FirstOrDefault(x =>
                    string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase) &&
                    x.PasswordHash == hash);
                return c == null ? null : c.Clone();
            }
            finally { App.UnLock(); }
        }

        // Kiểm tra trùng username/email với các tài khoản khác ignoreId và đặt thông báo lỗi phù hợp
        // qua tham số out; trả về true nếu phát hiện xung đột.
        static bool IsDuplicate(List<Customer> list, int ignoreId, string username, string email, out string error)
        {
            foreach (Customer x in list)
            {
                if (x.Id == ignoreId) continue;
                if (string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    error = "Tên đăng nhập đã tồn tại.";
                    return true;
                }
                if (string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    error = "Email đã được sử dụng.";
                    return true;
                }
            }
            error = null;
            return false;
        }

        // Kiểm tra trùng thông tin, cấp ID mới, băm mật khẩu, chuẩn hóa role, thêm tài khoản vào kho
        // và ghi file; đồng thời trả ID được cấp vào đối tượng đầu vào.
        public static bool AddCustomer(Customer customer, string plainPassword, out string error)
        {
            App.Lock();
            try
            {
                List<Customer> list = CustomerList();
                if (IsDuplicate(list, 0, customer.Username, customer.Email, out error)) return false;

                int next = (int)App[KNextCustomerId];
                App[KNextCustomerId] = next + 1;

                Customer c = customer.Clone();
                c.Id = next;
                c.PasswordHash = Hash(plainPassword);
                c.CreatedAt = DateTime.Now;
                if (c.Role != RoleAdmin) c.Role = RoleCustomer;
                list.Add(c);
                Persist();

                customer.Id = next;
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        /// <summary>Cập nhật khách hàng. newPassword để trống = giữ nguyên mật khẩu cũ.</summary>
        // Tìm tài khoản hiện có, kiểm tra trùng và bảo vệ admin cuối cùng, cập nhật thông tin cho phép
        // cùng mật khẩu mới nếu có, rồi persist kết quả qua error.
        public static bool UpdateCustomer(Customer customer, string newPassword, out string error)
        {
            App.Lock();
            try
            {
                List<Customer> list = CustomerList();
                Customer c = list.FirstOrDefault(x => x.Id == customer.Id);
                if (c == null)
                {
                    error = "Không tìm thấy tài khoản.";
                    return false;
                }
                if (IsDuplicate(list, c.Id, customer.Username, customer.Email, out error)) return false;

                string role = customer.Role == RoleAdmin ? RoleAdmin : RoleCustomer;
                if (c.Role == RoleAdmin && role != RoleAdmin && list.Count(x => x.Role == RoleAdmin) <= 1)
                {
                    error = "Phải còn ít nhất một quản trị viên.";
                    return false;
                }

                c.FullName = customer.FullName;
                c.Email = customer.Email;
                c.Username = customer.Username;
                c.Role = role;
                if (!string.IsNullOrEmpty(newPassword)) c.PasswordHash = Hash(newPassword);
                Persist();
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // Xóa tài khoản theo ID sau khi bảo đảm tài khoản tồn tại và không xóa admin cuối cùng;
        // trả thông báo chi tiết qua error.
        public static bool DeleteCustomer(int id, out string error)
        {
            App.Lock();
            try
            {
                List<Customer> list = CustomerList();
                Customer c = list.FirstOrDefault(x => x.Id == id);
                if (c == null)
                {
                    error = "Không tìm thấy tài khoản.";
                    return false;
                }
                if (c.Role == RoleAdmin && list.Count(x => x.Role == RoleAdmin) <= 1)
                {
                    error = "Không thể xóa quản trị viên cuối cùng.";
                    return false;
                }
                list.Remove(c);
                Persist();
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // ===== Đánh giá =====
        // Lấy danh sách Review gốc từ vùng Application để các thao tác đánh giá dùng nội bộ.
        static List<Review> ReviewList()
        {
            return (List<Review>)App[KReviews];
        }

        /// <summary>Đánh giá của một sản phẩm, mới nhất lên đầu.</summary>
        // Lọc đánh giá theo sản phẩm, sắp xếp mới nhất trước và trả về bản sao của từng đánh giá.
        public static List<Review> GetReviews(int productId)
        {
            App.Lock();
            try
            {
                return ReviewList().Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.CreatedAt).Select(r => r.Clone()).ToList();
            }
            finally { App.UnLock(); }
        }

        /// <summary>Điểm trung bình (0 nếu chưa có đánh giá) và số lượt đánh giá của sản phẩm.</summary>
        // Lọc đánh giá của sản phẩm, tính số lượng và trung bình điểm; trả average=0 khi chưa có đánh giá.
        public static void GetRating(int productId, out double average, out int count)
        {
            App.Lock();
            try
            {
                List<Review> list = ReviewList().Where(r => r.ProductId == productId).ToList();
                count = list.Count;
                average = count == 0 ? 0 : list.Average(r => r.Rating);
            }
            finally { App.UnLock(); }
        }

        /// <summary>Thêm đánh giá. Khách đã đăng nhập chỉ có một đánh giá cho mỗi sản phẩm: gửi lại sẽ ghi đè bản cũ.</summary>
        // Kiểm tra điểm và sản phẩm tồn tại, sau đó cập nhật đánh giá cũ của cùng khách/sản phẩm hoặc
        // cấp ID để thêm đánh giá mới, luôn ghi thời điểm và persist dữ liệu.
        public static bool SaveReview(Review review, out string error)
        {
            if (review.Rating < 1 || review.Rating > 5)
            {
                error = "Vui lòng chọn số sao từ 1 đến 5.";
                return false;
            }

            App.Lock();
            try
            {
                if (!ProductList().Any(p => p.Id == review.ProductId))
                {
                    error = "Sản phẩm không tồn tại.";
                    return false;
                }

                List<Review> list = ReviewList();
                Review existing = review.CustomerId > 0
                    ? list.FirstOrDefault(r => r.ProductId == review.ProductId && r.CustomerId == review.CustomerId)
                    : null;

                if (existing != null)
                {
                    existing.AuthorName = review.AuthorName;
                    existing.Rating = review.Rating;
                    existing.Comment = review.Comment;
                    existing.CreatedAt = DateTime.Now;
                }
                else
                {
                    int next = (int)App[KNextReviewId];
                    App[KNextReviewId] = next + 1;

                    Review r = review.Clone();
                    r.Id = next;
                    r.CreatedAt = DateTime.Now;
                    list.Add(r);
                }
                Persist();
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // ===== Đơn hàng =====
        // Lấy danh sách Order gốc từ vùng Application để các phương thức đơn hàng dùng nội bộ.
        static List<Order> OrderList()
        {
            return (List<Order>)App[KOrders];
        }

        /// <summary>Đơn hàng của một tài khoản, mới nhất lên đầu (dùng cho trang theo dõi đơn hàng).</summary>
        // Lọc đơn theo customerId, sắp xếp ID giảm dần và trả về bản sao để khách chỉ đọc dữ liệu riêng.
        public static List<Order> GetOrdersByCustomer(int customerId)
        {
            App.Lock();
            try
            {
                return OrderList().Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.Id).Select(o => o.Clone()).ToList();
            }
            finally { App.UnLock(); }
        }

        /// <summary>Tất cả đơn hàng, mới nhất lên đầu.</summary>
        // Lấy toàn bộ đơn, sắp xếp đơn mới nhất trước và trả về các bản sao cho trang quản trị.
        public static List<Order> GetOrders()
        {
            App.Lock();
            try { return OrderList().OrderByDescending(o => o.Id).Select(o => o.Clone()).ToList(); }
            finally { App.UnLock(); }
        }

        // Tìm đơn theo ID dưới khóa Application và trả bản sao độc lập hoặc null nếu không tồn tại.
        public static Order GetOrder(int id)
        {
            App.Lock();
            try
            {
                Order o = OrderList().FirstOrDefault(x => x.Id == id);
                return o == null ? null : o.Clone();
            }
            finally { App.UnLock(); }
        }

        /// <summary>Ghi đơn mới (trạng thái "Chờ xử lý") và trả về mã đơn.</summary>
        // Cấp ID kế tiếp, sao chép đơn đầu vào, đặt trạng thái/thời gian tạo, thêm vào kho và persist,
        // sau đó trả về mã đơn mới.
        public static int AddOrder(Order order)
        {
            App.Lock();
            try
            {
                int next = (int)App[KNextOrderId];
                App[KNextOrderId] = next + 1;

                Order o = order.Clone();
                o.Id = next;
                o.Status = StatusPending;
                o.CreatedAt = DateTime.Now;
                OrderList().Add(o);
                Persist();
                return next;
            }
            finally { App.UnLock(); }
        }

        // Kiểm tra trạng thái thuộc danh sách cho phép, tìm đơn theo ID, cập nhật trạng thái và persist;
        // mọi lỗi nghiệp vụ được trả qua error.
        public static bool UpdateOrderStatus(int id, string status, out string error)
        {
            if (Array.IndexOf(OrderStatuses, status) < 0)
            {
                error = "Trạng thái không hợp lệ.";
                return false;
            }

            App.Lock();
            try
            {
                Order o = OrderList().FirstOrDefault(x => x.Id == id);
                if (o == null)
                {
                    error = "Không tìm thấy đơn hàng.";
                    return false;
                }
                o.Status = status;
                Persist();
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // ===== Dữ liệu mẫu ban đầu =====
        // Tạo một ShopProduct mẫu từ các tham số cấu hình, đồng thời chuyển mảng ảnh và chuỗi màu
        // phân cách bằng dấu phẩy thành hai danh sách thuộc tính.
        static ShopProduct Seed(int id, string name, long price, long oldPrice, string cat,
            string[] images, string description, string material, string sizes, string status, string colors)
        {
            ShopProduct p = new ShopProduct
            {
                Id = id,
                Name = name,
                Price = price,
                OldPrice = oldPrice,
                Cat = cat,
                Description = description,
                Material = material,
                Sizes = sizes,
                Status = status
            };
            p.Images.AddRange(images);
            p.Colors.AddRange(colors.Split(',').Select(c => c.Trim()).Where(c => c.Length > 0));
            return p;
        }

        // Tạo một Review mẫu với thời điểm được lùi lại số ngày chỉ định để dữ liệu demo có thứ tự
        // thời gian giống đánh giá đã tồn tại trước đó.
        static Review SeedReview(int id, int productId, string author, int rating, string comment, int daysAgo)
        {
            return new Review
            {
                Id = id,
                ProductId = productId,
                CustomerId = 0,
                AuthorName = author,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now.AddDays(-daysAgo)
            };
        }

        // Tạo danh sách các đánh giá mẫu bằng cách gọi SeedReview cho từng bản ghi demo.
        static List<Review> SeedReviews()
        {
            return new List<Review>
            {
                SeedReview(1, 1, "Minh Anh", 5, "Lều dựng rất nhanh, cả nhà 5 người nằm vẫn thoải mái. Chống mưa tốt.", 12),
                SeedReview(2, 1, "Quốc Huy", 4, "Chất liệu ổn, hơi nặng khi mang đi xa nhưng đáng tiền.", 6),
                SeedReview(3, 3, "Thu Hà", 5, "Ba lô nhẹ, nhiều ngăn tiện lợi, đi dã ngoại một ngày là vừa.", 9),
                SeedReview(4, 5, "Đức Thắng", 4, "Đèn sáng rõ, vặn thu phóng mượt. Pin dùng được khá lâu.", 3)
            };
        }

        // Tạo toàn bộ danh sách sản phẩm mẫu, gồm thông tin giá, ảnh, mô tả, tồn kho và màu sắc
        // để website có dữ liệu hiển thị ngay lần chạy đầu.
        static List<ShopProduct> SeedProducts()
        {
            return new List<ShopProduct>
            {
                Seed(1, "Lều vòm dựng nhanh Ozark Trail 6 người", 3500000, 4800000, "leu",
                    new[] { "Images/sp1_1.jpg", "Images/sp1_2.jpg", "Images/sp1_3.jpg", "Images/sp1_4.jpg" },
                    "Lều vòm dựng nhanh với khung gập sẵn, một người cũng dựng được trong vài phút. Có cửa lưới thoáng khí, cổng luồn dây điện tiện sạc thiết bị và túi lưới đựng đồ cá nhân bên trong.",
                    "Vải polyester chống thấm, khung sợi thủy tinh", "Rộng 3,0 x 2,7 m, ngủ thoải mái 6 người", "Còn hàng", "Xanh rêu, Cam, Xám"),
                Seed(2, "Quạt cắm trại Panergy có đèn LED", 1800000, 0, "thiet-bi",
                    new[] { "Images/sp2_1.jpg", "Images/sp2_2.jpg", "Images/sp2_3.jpg", "Images/sp2_4.jpg" },
                    "Quạt sạc pin dung lượng lớn kèm đèn LED viền xung quanh, có móc treo trong lều. Nhiều tốc độ gió, dùng được cả ban ngày lẫn ban đêm.",
                    "Nhựa ABS, pin lithium sạc lại", "Đường kính cánh quạt 20 cm", "Còn hàng", "Trắng, Đen"),
                Seed(3, "Ba lô Ozark Trail 20,5 lít", 300000, 600000, "ba-lo",
                    new[] { "Images/sp3_1.jpg", "Images/sp3_2.jpg", "Images/sp3_3.jpg", "Images/sp3_4.jpg" },
                    "Ba lô dã ngoại 20,5 lít gọn nhẹ, dây rút chun phía trước để buộc thêm đồ, ngăn lưới hai bên đựng chai nước, quai đeo êm vai.",
                    "Vải polyester bền chống nước nhẹ", "Dung tích 20,5 lít", "Còn hàng", "Xanh navy, Đen, Xám"),
                Seed(4, "Đèn pha đa màu Ozark Trail", 130000, 180000, "den",
                    new[] { "Images/sp4_1.jpg", "Images/sp4_2.jpg", "Images/sp4_3.jpg", "Images/sp4_4.jpg" },
                    "Đèn đeo đầu nhỏ gọn, nhiều chế độ sáng và nhiều màu ánh sáng. Dây đeo co giãn, chỉnh được góc chiếu, phù hợp đi bộ đêm, dựng lều và sửa chữa.",
                    "Nhựa ABS, dây đeo thun dệt", "Free size, dùng 3 pin AAA", "Còn hàng", "Đen, Cam"),
                Seed(5, "Đèn pin LED Ozark Trail 420", 250000, 600000, "den",
                    new[] { "Images/sp5_1.jpg", "Images/sp5_2.jpg", "Images/sp5_3.jpg", "Images/sp5_4.jpg" },
                    "Đèn pin LED 420 lumen, vặn đầu để thu phóng chùm sáng. Thân nhôm chắc chắn, chống nước bắn, cầm gọn trong tay.",
                    "Nhôm hợp kim", "Dài 14 cm, 420 lumen", "Còn hàng", "Đen, Bạc"),
                Seed(6, "Lều vòm cắm trại 3 người Alpha Camp", 3800000, 0, "leu",
                    new[] { "Images/sp6_1.jpg", "Images/sp6_2.jpg", "Images/sp6_3.jpg", "Images/sp6_4.jpg" },
                    "Lều vòm 2 lớp cho 3 người, cửa sổ lưới thông gió, lớp ngoài chống nước. Gọn nhẹ, dễ dựng và dễ mang theo khi đi dã ngoại.",
                    "Vải polyester 190T chống thấm, khung sợi thủy tinh", "Rộng 2,1 x 2,1 m, cho 3 người", "Còn hàng", "Xanh lá, Cam"),
                Seed(7, "Xe đạp leo núi 6 tốc độ", 1800000, 0, "xe-dap",
                    new[] { "Images/sp7_1.jpg", "Images/sp7_2.jpg", "Images/sp7_3.jpg", "Images/sp7_4.jpg" },
                    "Xe đạp leo núi cỡ nhỏ 6 tốc độ, lốp địa hình bám đường tốt, khung thép chắc chắn. Phù hợp thiếu niên đạp quanh khu cắm trại hay đường đất.",
                    "Khung thép, lốp địa hình", "Bánh 20 inch", "Còn hàng", "Đỏ, Xanh dương, Đen"),
                Seed(8, "Xe đạp leo núi dành cho người lớn", 4000000, 6000000, "xe-dap",
                    new[] { "Images/sp8_1.jpg", "Images/sp8_2.jpg", "Images/sp8_3.jpg", "Images/sp8_4.jpg" },
                    "Xe đạp leo núi người lớn với phuộc giảm xóc trước, phanh đĩa và bộ truyền động nhiều tốc độ. Chạy êm trên đường đồi dốc và đường mòn.",
                    "Khung hợp kim nhôm, phanh đĩa", "Bánh 26 inch", "Còn hàng", "Đen, Xám, Xanh dương")
            };
        }
    }
}