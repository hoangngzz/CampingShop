using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Demo_btl_10_10
{
    public partial class Admin : Page
    {
        // Đơn đang xem chi tiết (Admin.aspx?tab=dh&id=...), null nếu không có
        protected Order CurOrder;

        // Admin.aspx?tab=kh -> tài khoản, ?tab=dh -> đơn hàng, còn lại là sản phẩm
        private string CurrentTab
        {
            get
            {
                string tab = (Request.QueryString["tab"] ?? "").ToLowerInvariant();
                return tab == "kh" || tab == "dh" ? tab : "sp";
            }
        }

        // Kiểm tra quyền admin trước khi cho phép truy cập; ở lần tải đầu tiên nạp danh mục sản phẩm
        // và các trạng thái đơn vào dropdown để các biểu mẫu quản trị sử dụng.
        protected void Page_Load(object sender, EventArgs e)
        {
            // Chỉ admin mới được vào trang này
            if (!AppData.IsAdmin(Session))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                foreach (KeyValuePair<string, string> kv in AppData.Categories)
                    ddlPCat.Items.Add(new ListItem(kv.Value, kv.Key));

                foreach (string status in AppData.OrderStatuses)
                    ddlOrderStatus.Items.Add(new ListItem(status, status));
            }
        }

        // Xác định tab hiện tại, bật đúng panel và trạng thái tab, sau đó binding lại dữ liệu tương ứng
        // sau khi event handler đã chạy để danh sách sản phẩm, khách hàng hoặc đơn hàng luôn mới nhất.
        protected void Page_PreRender(object sender, EventArgs e)
        {
            string tab = CurrentTab;
            pnlProducts.Visible = tab == "sp";
            pnlCustomers.Visible = tab == "kh";
            pnlOrders.Visible = tab == "dh";
            tabProducts.CssClass = tab == "sp" ? "admin-tab active" : "admin-tab";
            tabCustomers.CssClass = tab == "kh" ? "admin-tab active" : "admin-tab";
            tabOrders.CssClass = tab == "dh" ? "admin-tab active" : "admin-tab";

            if (tab == "dh")
            {
                BindOrders();
            }
            else if (tab == "kh")
            {
                List<Customer> customers = AppData.GetCustomers();
                litCustomerCount.Text = customers.Count.ToString();
                rptCustomers.DataSource = customers;
                rptCustomers.DataBind();
            }
            else
            {
                List<ShopProduct> products = AppData.GetProducts();
                litProductCount.Text = products.Count.ToString();
                rptProducts.DataSource = products;
                rptProducts.DataBind();
            }
        }

        // ===== Đơn hàng =====
        // Lấy danh sách đơn, binding bảng và xử lý đơn được chọn từ query string; nếu có đơn thì
        // binding các dòng hàng và chọn trạng thái hiện tại, nếu không thì hiển thị thông báo phù hợp.
        private void BindOrders()
        {
            List<Order> orders = AppData.GetOrders();
            litOrderCount.Text = orders.Count.ToString();
            rptOrders.DataSource = orders;
            rptOrders.DataBind();
            pnlNoOrders.Visible = orders.Count == 0;

            int id;
            CurOrder = int.TryParse(Request.QueryString["id"], out id) ? AppData.GetOrder(id) : null;
            pnlOrderDetail.Visible = CurOrder != null;
            if (CurOrder != null)
            {
                rptOrderItems.DataSource = CurOrder.Items;
                rptOrderItems.DataBind();
                Select(ddlOrderStatus, CurOrder.Status);
            }
            else if (Request.QueryString["id"] != null)
            {
                Show(lblOrderMsg, "Không tìm thấy đơn hàng.", false);
            }
        }

        // Đọc ID đơn từ URL, xác nhận trạng thái được chọn thông qua AppData và hiển thị kết quả
        // cập nhật cho admin; trường hợp ID hoặc trạng thái không hợp lệ sẽ không ghi dữ liệu.
        protected void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(Request.QueryString["id"], out id)) return;

            string error;
            if (AppData.UpdateOrderStatus(id, ddlOrderStatus.SelectedValue, out error))
                Show(lblOrderMsg, "Đã cập nhật trạng thái đơn #" + id + ".", true);
            else
                Show(lblOrderMsg, error, false);
        }

        // Chuyển giá trị binding sang chuỗi tiền Việt Nam dùng trong bảng chi tiết đơn hàng.
        protected string OrderMoney(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

        // Ánh xạ trạng thái đơn hàng sang CSS class để badge trên trang quản trị có màu phù hợp.
        protected string StatusCss(object status)
        {
            switch (Convert.ToString(status))
            {
                case AppData.StatusPending: return "tag pending";
                case AppData.StatusShipping: return "tag shipping";
                case AppData.StatusCancelled: return "tag cancel";
                default: return "tag";
            }
        }

        // Tạo nhãn tài khoản của đơn: khách vãng lai, tài khoản đã xóa hoặc username kèm họ tên.
        protected string CustomerLabel(Order order)
        {
            if (order.CustomerId <= 0) return "Khách chưa đăng nhập";
            Customer c = AppData.GetCustomer(order.CustomerId);
            return c == null ? "Tài khoản đã xóa" : c.Username + " (" + c.FullName + ")";
        }

        // Tạo HTML màu sản phẩm đã mã hóa để hiển thị trong dòng đơn; màu rỗng thì không tạo HTML.
        protected string ColorHtml(object color)
        {
            string c = Convert.ToString(color);
            return string.IsNullOrEmpty(c) ? string.Empty : "<span class=\"order-item-color\">Màu: " + Server.HtmlEncode(c) + "</span>";
        }

        /// <summary>Tạo vùng thumbnail cho tối đa ba sản phẩm trong đơn.</summary>
        /// <remarks>Các thuộc tính đường dẫn và tên được HTML-encode; nếu còn sản phẩm ngoài ba ảnh
        /// đầu tiên thì thêm nhãn số lượng còn lại.</remarks>
        protected string Thumbs(object dataItem)
        {
            Order order = (Order)dataItem;
            StringBuilder sb = new StringBuilder("<div class=\"order-thumbs\">");
            foreach (OrderItem i in order.Items.Take(3))
            {
                string name = Server.HtmlEncode(i.ProductName);
                sb.Append("<img class=\"order-thumb\" src=\"" + Server.HtmlEncode(i.Image) + "\" alt=\"" + name + "\" title=\"" + name + "\" />");
            }
            if (order.Items.Count > 3)
                sb.Append("<span class=\"order-more\">+" + (order.Items.Count - 3) + "</span>");
            return sb.Append("</div>").ToString();
        }

        // ===== Hàm hỗ trợ cho giao diện =====
        // Định dạng số tiền dương theo văn hóa Việt Nam; giá trị 0 hoặc âm được biểu diễn bằng chuỗi rỗng.
        protected string Money(object value)
        {
            long n = Convert.ToInt64(value);
            return n > 0 ? n.ToString("N0", new CultureInfo("vi-VN")) + "đ" : "";
        }

        // Tra mã danh mục trong bảng Categories và trả về tên hiển thị; nếu mã chưa biết thì giữ nguyên mã.
        protected string CatName(object cat)
        {
            string key = Convert.ToString(cat);
            string name;
            return AppData.Categories.TryGetValue(key, out name) ? name : key;
        }

        // Chuyển role lưu trong dữ liệu thành nhãn tiếng Việt dùng ở danh sách tài khoản quản trị.
        protected string RoleName(object role)
        {
            return Convert.ToString(role) == AppData.RoleAdmin ? "Quản trị viên" : "Khách hàng";
        }

        // Chuyển role thành CSS class cho badge admin hoặc badge người dùng thông thường.
        protected string RoleCss(object role)
        {
            return Convert.ToString(role) == AppData.RoleAdmin ? "tag admin" : "tag";
        }

        // Ghi nội dung đã mã hóa vào Label và chọn CSS class thành công/lỗi cho các biểu mẫu quản trị.
        private void Show(Label label, string text, bool ok)
        {
            label.Text = Server.HtmlEncode(text);
            label.CssClass = ok ? "admin-msg ok" : "admin-msg err";
        }

        // Chọn item trong DropDownList theo value; nếu không tồn tại thì giữ nguyên lựa chọn hiện tại.
        private static void Select(DropDownList ddl, string value)
        {
            ListItem item = ddl.Items.FindByValue(value);
            if (item == null) return;
            ddl.ClearSelection();
            item.Selected = true;
        }

        // ================= SẢN PHẨM =================
        // Xóa ID ẩn và toàn bộ trường nhập sản phẩm, đồng thời bỏ chọn danh mục/trạng thái để chuẩn bị
        // cho biểu mẫu thêm mới.
        private void ClearProductForm()
        {
            hfProductId.Value = "0";
            txtPName.Text = "";
            txtPPrice.Text = "";
            txtPOld.Text = "";
            txtPImages.Text = "";
            txtPDesc.Text = "";
            txtPMaterial.Text = "";
            txtPSizes.Text = "";
            txtPColors.Text = "";
            ddlPCat.ClearSelection();
            ddlPStatus.ClearSelection();
        }

        // Mở biểu mẫu sản phẩm ở chế độ thêm mới sau khi đặt lại các trường về giá trị ban đầu.
        protected void btnNewProduct_Click(object sender, EventArgs e)
        {
            ClearProductForm();
            litProductFormTitle.Text = "Thêm sản phẩm mới";
            pnlProductForm.Visible = true;
        }

        // Đóng biểu mẫu sản phẩm mà không lưu thay đổi hiện tại.
        protected void btnCancelProduct_Click(object sender, EventArgs e)
        {
            pnlProductForm.Visible = false;
        }

        // Xử lý lệnh sửa hoặc xóa của repeater sản phẩm; khi sửa thì nạp dữ liệu vào biểu mẫu,
        // khi xóa thì gọi AppData và cập nhật thông báo/trạng thái biểu mẫu.
        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            if (e.CommandName == "edit")
            {
                ShopProduct p = AppData.GetProduct(id);
                if (p == null)
                {
                    Show(lblProductMsg, "Không tìm thấy sản phẩm.", false);
                    return;
                }
                hfProductId.Value = p.Id.ToString();
                txtPName.Text = p.Name;
                Select(ddlPCat, p.Cat);
                Select(ddlPStatus, p.Status);
                txtPPrice.Text = p.Price.ToString(CultureInfo.InvariantCulture);
                txtPOld.Text = p.OldPrice > 0 ? p.OldPrice.ToString(CultureInfo.InvariantCulture) : "";
                txtPImages.Text = string.Join(Environment.NewLine, p.Images);
                txtPDesc.Text = p.Description;
                txtPMaterial.Text = p.Material;
                txtPSizes.Text = p.Sizes;
                txtPColors.Text = string.Join(", ", p.Colors);
                litProductFormTitle.Text = "Sửa sản phẩm #" + p.Id;
                pnlProductForm.Visible = true;
            }
            else if (e.CommandName == "delete")
            {
                if (AppData.DeleteProduct(id))
                {
                    if (hfProductId.Value == id.ToString()) pnlProductForm.Visible = false;
                    Show(lblProductMsg, "Đã xóa sản phẩm.", true);
                }
                else
                {
                    Show(lblProductMsg, "Không tìm thấy sản phẩm để xóa.", false);
                }
            }
        }

        // Kiểm tra đường dẫn ảnh có là URL HTTP(S) hoặc đường dẫn nội bộ an toàn hay không,
        // đồng thời chặn scheme nguy hiểm như javascript:, data: và protocol-relative URL.
        private static bool IsSafeImage(string path)
        {
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return true;
            // đường dẫn trong website: không chứa dấu ":" (chặn javascript:, data:...) và không có "//" ở đầu
            return path.IndexOf(':') < 0 && !path.StartsWith("//");
        }

        // Đọc và kiểm tra toàn bộ dữ liệu biểu mẫu sản phẩm, chuẩn hóa danh sách ảnh/màu,
        // tạo ShopProduct rồi lưu mới hoặc cập nhật qua AppData và thông báo kết quả cho admin.
        protected void btnSaveProduct_Click(object sender, EventArgs e)
        {
            string name = txtPName.Text.Trim();
            if (name.Length == 0)
            {
                Show(lblProductMsg, "Vui lòng nhập tên sản phẩm.", false);
                return;
            }

            long price;
            if (!long.TryParse(txtPPrice.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out price) || price <= 0)
            {
                Show(lblProductMsg, "Giá bán phải là số nguyên lớn hơn 0.", false);
                return;
            }

            long oldPrice = 0;
            if (txtPOld.Text.Trim().Length > 0 &&
                (!long.TryParse(txtPOld.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out oldPrice) || oldPrice <= price))
            {
                Show(lblProductMsg, "Giá cũ phải là số lớn hơn giá bán (hoặc để trống nếu không giảm giá).", false);
                return;
            }

            List<string> images = new List<string>();
            foreach (string line in txtPImages.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string path = line.Trim();
                if (path.Length == 0) continue;
                if (!IsSafeImage(path))
                {
                    Show(lblProductMsg, "Đường dẫn ảnh không hợp lệ: " + path, false);
                    return;
                }
                images.Add(path);
            }
            if (images.Count == 0) images.Add("Images/product1.jpg");

            int id;
            int.TryParse(hfProductId.Value, out id);

            ShopProduct p = new ShopProduct
            {
                Id = id,
                Name = name,
                Cat = ddlPCat.SelectedValue,
                Price = price,
                OldPrice = oldPrice,
                Images = images,
                Colors = txtPColors.Text.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Description = txtPDesc.Text.Trim(),
                Material = txtPMaterial.Text.Trim(),
                Sizes = txtPSizes.Text.Trim(),
                Status = ddlPStatus.SelectedValue
            };

            AppData.SaveProduct(p);
            pnlProductForm.Visible = false;
            Show(lblProductMsg, id > 0 ? "Đã cập nhật sản phẩm." : "Đã thêm sản phẩm mới.", true);
        }

        // ================= KHÁCH HÀNG =================
        // Đóng biểu mẫu khách hàng mà không ghi lại các giá trị đang nhập.
        protected void btnCancelCustomer_Click(object sender, EventArgs e)
        {
            pnlCustomerForm.Visible = false;
        }

        // Xử lý lệnh sửa hoặc xóa tài khoản trong repeater; khi sửa thì nạp dữ liệu vào biểu mẫu,
        // khi xóa thì ngăn xóa tài khoản đang đăng nhập và hiển thị kết quả từ AppData.
        protected void rptCustomers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            if (e.CommandName == "edit")
            {
                Customer c = AppData.GetCustomer(id);
                if (c == null)
                {
                    Show(lblCustomerMsg, "Không tìm thấy tài khoản.", false);
                    return;
                }
                hfCustomerId.Value = c.Id.ToString();
                txtCName.Text = c.FullName;
                txtCEmail.Text = c.Email;
                txtCUser.Text = c.Username;
                Select(ddlCRole, c.Role);
                litCustomerFormTitle.Text = "Sửa tài khoản #" + c.Id;
                pnlCustomerForm.Visible = true;
            }
            else if (e.CommandName == "delete")
            {
                if (id == Convert.ToInt32(Session["UserId"]))
                {
                    Show(lblCustomerMsg, "Không thể xóa tài khoản đang đăng nhập.", false);
                    return;
                }

                string error;
                if (AppData.DeleteCustomer(id, out error))
                {
                    if (hfCustomerId.Value == id.ToString()) pnlCustomerForm.Visible = false;
                    Show(lblCustomerMsg, "Đã xóa tài khoản.", true);
                }
                else
                {
                    Show(lblCustomerMsg, error, false);
                }
            }
        }

        // Kiểm tra dữ liệu tài khoản, giới hạn vai trò theo lựa chọn quản trị, gọi cập nhật AppData
        // và đồng bộ lại Session nếu admin vừa sửa chính tài khoản đang đăng nhập.
        protected void btnSaveCustomer_Click(object sender, EventArgs e)
        {
            string fullName = txtCName.Text.Trim();
            string email = txtCEmail.Text.Trim();
            string username = txtCUser.Text.Trim();
            string password = txtCPass.Text;

            int id;
            int.TryParse(hfCustomerId.Value, out id);

            // Admin chỉ sửa / xóa tài khoản, không thêm thành viên mới (thành viên tự đăng ký ở SignUp.aspx)
            if (id <= 0)
            {
                Show(lblCustomerMsg, "Quản trị viên không thể thêm tài khoản mới.", false);
                pnlCustomerForm.Visible = false;
                return;
            }

            if (fullName.Length == 0)
            {
                Show(lblCustomerMsg, "Vui lòng nhập họ và tên.", false);
                return;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Show(lblCustomerMsg, "Email không hợp lệ.", false);
                return;
            }
            if (username.Length < 3 || Regex.IsMatch(username, @"\s"))
            {
                Show(lblCustomerMsg, "Tên đăng nhập phải từ 3 ký tự và không chứa khoảng trắng.", false);
                return;
            }
            if (password.Length > 0 && password.Length < 8)
            {
                Show(lblCustomerMsg, "Mật khẩu phải có ít nhất 8 ký tự.", false);
                return;
            }

            Customer c = new Customer
            {
                Id = id,
                FullName = fullName,
                Email = email,
                Username = username,
                Role = ddlCRole.SelectedValue
            };

            string error;
            bool ok = AppData.UpdateCustomer(c, password, out error);

            if (!ok)
            {
                Show(lblCustomerMsg, error, false);
                return;
            }

            // Admin sửa chính tài khoản của mình -> cập nhật lại phiên đăng nhập
            if (id == Convert.ToInt32(Session["UserId"]))
            {
                Session["User"] = c.Username;
                Session["FullName"] = c.FullName;
                Session["Role"] = c.Role;
                if (c.Role != AppData.RoleAdmin)
                {
                    Response.Redirect("~/default.aspx");
                    return;
                }
            }

            pnlCustomerForm.Visible = false;
            Show(lblCustomerMsg, "Đã cập nhật tài khoản.", true);
        }
    }
}