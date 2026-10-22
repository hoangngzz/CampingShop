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
using System.Xml.Serialization;

namespace Demo_btl_10_10
{
    /// <summary>
    /// Kho dữ liệu dùng chung cho cả website (sản phẩm, khách hàng, đánh giá, đơn hàng),
    /// lưu trong Application (khởi tạo ở Global.asax).
    /// Mọi thao tác đọc/ghi đều Lock/UnLock để nhiều người truy cập cùng lúc không bị lỗi.
    /// Tài khoản (khách hàng/admin) còn được lưu ra App_Data/customers.xml để không mất khi website khởi động lại.
    /// Các dữ liệu khác (sản phẩm, đánh giá, đơn hàng, ...) nằm trong bộ nhớ và quay về dữ liệu mẫu khi khởi động lại.
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
        const string KArticles = "MyShop.Articles";
        const string KMessages = "MyShop.Messages";
        const string KNextMessageId = "MyShop.NextMessageId";

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

        static HttpApplicationState App
        {
            get { return HttpContext.Current.Application; }
        }

        // ===== Khởi tạo (gọi từ Application_Start trong Global.asax.cs) =====
        public static void Init(HttpApplicationState app)
        {
            app.Lock();
            try
            {
                List<ShopProduct> products = SeedProducts();
                app[KProducts] = products;
                app[KNextProductId] = products.Max(p => p.Id) + 1;

                // Tài khoản: nạp từ file nếu đã có, chưa có thì tạo tài khoản admin mặc định
                List<Customer> customers = LoadCustomers();
                if (customers == null)
                {
                    customers = new List<Customer>();
                    customers.Add(new Customer
                    {
                        Id = 1,
                        FullName = "Quản trị viên",
                        Email = "admin@mycamp.vn",
                        Username = "admin",
                        PasswordHash = Hash("admin"),
                        Role = RoleAdmin,
                        CreatedAt = DateTime.Now
                    });
                    SaveCustomers(customers);
                }
                app[KCustomers] = customers;
                app[KNextCustomerId] = customers.Max(c => c.Id) + 1;

                List<Review> reviews = SeedReviews();
                app[KReviews] = reviews;
                app[KNextReviewId] = reviews.Max(r => r.Id) + 1;

                app[KOrders] = new List<Order>();
                app[KNextOrderId] = 1;

                app[KArticles] = SeedArticles();
                app[KMessages] = new List<ContactMessage>();
                app[KNextMessageId] = 1;
            }
            finally
            {
                app.UnLock();
            }
        }

        // ===== Tiện ích =====
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

        public static string Money(long n)
        {
            return n.ToString("N0", Vi) + "đ";
        }

        public static string CatName(string key)
        {
            string name;
            return key != null && Categories.TryGetValue(key, out name) ? name : key;
        }

        /// <summary>Chữ thường + bỏ dấu để gõ "leu" vẫn tìm ra "Lều".</summary>
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

        public static bool IsAdmin(HttpSessionState session)
        {
            return session != null &&
                   string.Equals(session["Role"] as string, RoleAdmin, StringComparison.OrdinalIgnoreCase);
        }

        // ===== Sản phẩm =====
        static List<ShopProduct> ProductList()
        {
            return (List<ShopProduct>)App[KProducts];
        }

        public static List<ShopProduct> GetProducts()
        {
            App.Lock();
            try { return ProductList().Select(p => p.Clone()).ToList(); }
            finally { App.UnLock(); }
        }

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
            }
            finally { App.UnLock(); }
        }

        public static bool DeleteProduct(int id)
        {
            App.Lock();
            try
            {
                bool removed = ProductList().RemoveAll(x => x.Id == id) > 0;
                if (removed) ReviewList().RemoveAll(r => r.ProductId == id);
                return removed;
            }
            finally { App.UnLock(); }
        }

        // ===== Lưu tài khoản ra file (App_Data/customers.xml) =====
        static string CustomersFile
        {
            get { return HostingEnvironment.MapPath("~/App_Data/customers.xml"); }
        }

        /// <summary>Đọc tài khoản từ file. Trả về null nếu chưa có file, file rỗng hoặc bị hỏng (file hỏng được đổi tên để không bị ghi đè).</summary>
        static List<Customer> LoadCustomers()
        {
            string path = CustomersFile;
            try
            {
                if (!File.Exists(path)) return null;

                List<Customer> list;
                using (FileStream fs = File.OpenRead(path))
                {
                    list = (List<Customer>)new XmlSerializer(typeof(List<Customer>)).Deserialize(fs);
                }
                return list != null && list.Count > 0 ? list : null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Không đọc được customers.xml: " + ex.Message);
                try { File.Move(path, path + ".loi-" + DateTime.Now.ToString("yyyyMMddHHmmss")); }
                catch (Exception) { }
                return null;
            }
        }

        /// <summary>Ghi toàn bộ danh sách tài khoản ra file (ghi file tạm rồi thay thế để không hỏng file cũ nếu lỗi giữa chừng).</summary>
        static void SaveCustomers(List<Customer> list)
        {
            try
            {
                string path = CustomersFile;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp = path + ".tmp";
                using (FileStream fs = File.Create(tmp))
                {
                    new XmlSerializer(typeof(List<Customer>)).Serialize(fs, list);
                }
                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
            }
            catch (Exception ex)
            {
                // Không lưu được file thì tài khoản vẫn dùng được trong bộ nhớ, chỉ là sẽ mất khi khởi động lại
                Trace.TraceError("Không lưu được customers.xml: " + ex.Message);
            }
        }

        // ===== Tài khoản (khách hàng và quản trị viên) =====
        static List<Customer> CustomerList()
        {
            return (List<Customer>)App[KCustomers];
        }

        public static List<Customer> GetCustomers()
        {
            App.Lock();
            try { return CustomerList().Select(c => c.Clone()).ToList(); }
            finally { App.UnLock(); }
        }

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
                SaveCustomers(list);

                customer.Id = next;
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        /// <summary>Cập nhật khách hàng. newPassword để trống = giữ nguyên mật khẩu cũ.</summary>
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
                SaveCustomers(list);
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

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
                SaveCustomers(list);
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // ===== Đánh giá =====
        static List<Review> ReviewList()
        {
            return (List<Review>)App[KReviews];
        }

        /// <summary>Đánh giá của một sản phẩm, mới nhất lên đầu.</summary>
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
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // ===== Đơn hàng =====
        static List<Order> OrderList()
        {
            return (List<Order>)App[KOrders];
        }

        /// <summary>Tất cả đơn hàng, mới nhất lên đầu.</summary>
        public static List<Order> GetOrders()
        {
            App.Lock();
            try { return OrderList().OrderByDescending(o => o.Id).Select(o => o.Clone()).ToList(); }
            finally { App.UnLock(); }
        }

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
                return next;
            }
            finally { App.UnLock(); }
        }

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
                error = null;
                return true;
            }
            finally { App.UnLock(); }
        }

        // ===== Tin tức =====
        /// <summary>Tất cả bài viết, mới nhất lên đầu.</summary>
        public static List<Article> GetArticles()
        {
            App.Lock();
            try { return ((List<Article>)App[KArticles]).OrderByDescending(a => a.CreatedAt).ToList(); }
            finally { App.UnLock(); }
        }

        public static Article GetArticle(int id)
        {
            App.Lock();
            try { return ((List<Article>)App[KArticles]).FirstOrDefault(a => a.Id == id); }
            finally { App.UnLock(); }
        }

        // ===== Liên hệ =====
        /// <summary>Lưu lời nhắn của khách và trả về mã lời nhắn.</summary>
        public static int AddMessage(ContactMessage message)
        {
            App.Lock();
            try
            {
                int next = (int)App[KNextMessageId];
                App[KNextMessageId] = next + 1;

                ContactMessage m = new ContactMessage
                {
                    Id = next, Name = message.Name, Email = message.Email, Phone = message.Phone,
                    Subject = message.Subject, Message = message.Message, CreatedAt = DateTime.Now
                };
                ((List<ContactMessage>)App[KMessages]).Add(m);
                return next;
            }
            finally { App.UnLock(); }
        }

        /// <summary>Tất cả lời nhắn, mới nhất lên đầu.</summary>
        public static List<ContactMessage> GetMessages()
        {
            App.Lock();
            try { return ((List<ContactMessage>)App[KMessages]).OrderByDescending(m => m.Id).ToList(); }
            finally { App.UnLock(); }
        }

        // ===== Dữ liệu mẫu ban đầu =====
        static ShopProduct Seed(int id, string name, long price, long oldPrice, string cat,
            string[] images, string description, string material, string sizes, string status, string colors)
        {
            ShopProduct p = new ShopProduct
            {
                Id = id, Name = name, Price = price, OldPrice = oldPrice, Cat = cat,
                Description = description, Material = material, Sizes = sizes, Status = status
            };
            p.Images.AddRange(images);
            p.Colors.AddRange(colors.Split(',').Select(c => c.Trim()).Where(c => c.Length > 0));
            return p;
        }

        static Review SeedReview(int id, int productId, string author, int rating, string comment, int daysAgo)
        {
            return new Review
            {
                Id = id, ProductId = productId, CustomerId = 0, AuthorName = author,
                Rating = rating, Comment = comment, CreatedAt = DateTime.Now.AddDays(-daysAgo)
            };
        }

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

        static List<Article> SeedArticles()
        {
            return new List<Article>
            {
                new Article
                {
                    Id = 1, Title = "5 mẹo chọn lều cắm trại phù hợp cho gia đình", Image = "Images/sp1_2.jpg", CreatedAt = DateTime.Now.AddDays(-2),
                    Summary = "Số người, mùa đi và cách dựng là ba điều cần cân nhắc trước khi mua lều.",
                    Content = "Trước hết hãy chọn lều theo số người thực tế rồi cộng thêm một chỗ. Lều ghi 6 người thường chỉ thoải mái cho 4 đến 5 người khi có thêm hành lý.\n\nTiếp theo là chất liệu và khả năng chống thấm. Vải polyester phủ lớp chống nước và khung sợi thủy tinh là lựa chọn phổ biến, nhẹ và đủ bền cho các chuyến đi cuối tuần.\n\nCuối cùng, ưu tiên lều có cửa lưới thoáng khí và khung dựng nhanh. Một chiếc lều dễ dựng giúp cả nhà tiết kiệm thời gian để tận hưởng chuyến đi."
                },
                new Article
                {
                    Id = 2, Title = "Cách dựng lều vòm nhanh gọn trong 5 phút", Image = "Images/sp6_2.jpg", CreatedAt = DateTime.Now.AddDays(-5),
                    Summary = "Chỉ với vài bước đơn giản, một người cũng có thể dựng xong lều vòm.",
                    Content = "Chọn nền đất bằng phẳng, nhặt bỏ đá và cành khô, trải tấm lót chống ẩm trước khi dựng. Đặt cửa lều theo hướng khuất gió.\n\nLắp khung theo hai thanh chéo nhau, luồn vào các rãnh trên thân lều rồi nâng lên từng góc. Cố định bốn góc bằng cọc trước khi căng lớp áo mưa bên ngoài.\n\nCuối cùng kiểm tra dây giằng và độ căng của vải. Nếu trời có mưa, hãy căng thật chặt để nước không đọng trên mái."
                },
                new Article
                {
                    Id = 3, Title = "Danh sách đồ cần mang cho chuyến cắm trại qua đêm", Image = "Images/sp3_1.jpg", CreatedAt = DateTime.Now.AddDays(-9),
                    Summary = "Chuẩn bị đủ những món thiết yếu giúp chuyến đi nhẹ nhàng và an toàn hơn.",
                    Content = "Nhóm đồ ngủ nghỉ gồm lều, túi ngủ, tấm lót và gối hơi. Đừng quên áo ấm vì nhiệt độ ban đêm ở vùng núi có thể giảm mạnh.\n\nNhóm chiếu sáng và tiện ích gồm đèn pin, đèn đội đầu, quạt mini và pin sạc dự phòng. Mang thêm túi rác để giữ khu cắm trại sạch sẽ.\n\nNgoài ra hãy mang nước uống, đồ ăn nhẹ và bộ sơ cứu cơ bản. Một chiếc ba lô nhiều ngăn sẽ giúp bạn sắp xếp mọi thứ gọn gàng."
                },
                new Article
                {
                    Id = 4, Title = "Chọn đèn chiếu sáng nào cho chuyến dã ngoại?", Image = "Images/sp5_1.jpg", CreatedAt = DateTime.Now.AddDays(-14),
                    Summary = "Đèn pin, đèn đội đầu hay đèn treo lều: mỗi loại phù hợp một nhu cầu riêng.",
                    Content = "Đèn đội đầu giúp rảnh hai tay khi dựng lều, nấu ăn hoặc đi bộ đêm. Hãy chọn loại có nhiều chế độ sáng và dây đeo co giãn thoải mái.\n\nĐèn pin có thu phóng chùm sáng phù hợp để quan sát xa hoặc soi đường. Thân nhôm và khả năng chống nước bắn sẽ bền hơn khi dùng ngoài trời.\n\nVới không gian chung, một chiếc đèn treo trong lều cho ánh sáng dịu và đều. Nên mang thêm pin hoặc cục sạc dự phòng cho cả chuyến đi."
                },
                new Article
                {
                    Id = 5, Title = "Kinh nghiệm chọn ba lô dã ngoại 20 đến 30 lít", Image = "Images/sp3_2.jpg", CreatedAt = DateTime.Now.AddDays(-20),
                    Summary = "Dung tích, quai đeo và ngăn chứa quyết định ba lô có thoải mái hay không.",
                    Content = "Chuyến đi trong ngày chỉ cần ba lô khoảng 20 lít, đủ cho nước uống, áo mưa, đồ ăn nhẹ và vài vật dụng cá nhân.\n\nQuai đeo êm vai và có dây ngực giúp phân bổ trọng lượng đều hơn. Lưng ba lô thoáng khí sẽ đỡ bí khi đi lâu dưới trời nóng.\n\nHãy ưu tiên chất liệu chống nước nhẹ và ngăn lưới hai bên để đựng chai nước. Những chi tiết nhỏ này rất tiện khi di chuyển."
                },
                new Article
                {
                    Id = 6, Title = "Đạp xe địa hình: chuẩn bị gì trước khi lên đường?", Image = "Images/sp8_1.jpg", CreatedAt = DateTime.Now.AddDays(-27),
                    Summary = "Kiểm tra xe, mang đủ nước và chọn cung đường vừa sức là ba điều nên làm.",
                    Content = "Trước mỗi chuyến đi hãy kiểm tra phanh, lốp và xích. Bơm lốp đủ hơi và tra dầu xích giúp xe chạy êm và an toàn hơn trên đường đất.\n\nMang theo nước, đồ ăn nhẹ, bộ vá săm và bơm mini. Mũ bảo hiểm và găng tay là trang bị không nên bỏ qua.\n\nNếu mới bắt đầu, hãy chọn cung đường ngắn, ít dốc và đi cùng bạn bè. Tăng dần quãng đường khi đã quen với xe."
                }
            };
        }

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
