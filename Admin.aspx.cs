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
                return tab == "kh" || tab == "dh" || tab == "lh" ? tab : "sp";
            }
        }

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

        // Đổ dữ liệu sau khi các nút đã xử lý xong, để bảng luôn hiện dữ liệu mới nhất trong Application
        protected void Page_PreRender(object sender, EventArgs e)
        {
            string tab = CurrentTab;
            pnlProducts.Visible = tab == "sp";
            pnlCustomers.Visible = tab == "kh";
            pnlOrders.Visible = tab == "dh";
            pnlMessages.Visible = tab == "lh";
            tabProducts.CssClass = tab == "sp" ? "admin-tab active" : "admin-tab";
            tabCustomers.CssClass = tab == "kh" ? "admin-tab active" : "admin-tab";
            tabOrders.CssClass = tab == "dh" ? "admin-tab active" : "admin-tab";
            tabMessages.CssClass = tab == "lh" ? "admin-tab active" : "admin-tab";

            if (tab == "lh")
            {
                List<ContactMessage> messages = AppData.GetMessages();
                litMessageCount.Text = messages.Count.ToString();
                rptMessages.DataSource = messages;
                rptMessages.DataBind();
                pnlNoMessages.Visible = messages.Count == 0;
            }
            else if (tab == "dh")
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

        protected string OrderMoney(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

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

        protected string CustomerLabel(Order order)
        {
            if (order.CustomerId <= 0) return "Khách chưa đăng nhập";
            Customer c = AppData.GetCustomer(order.CustomerId);
            return c == null ? "Tài khoản đã xóa" : c.Username + " (" + c.FullName + ")";
        }

        protected string ColorHtml(object color)
        {
            string c = Convert.ToString(color);
            return string.IsNullOrEmpty(c) ? string.Empty : "<span class=\"order-item-color\">Màu: " + Server.HtmlEncode(c) + "</span>";
        }

        /// <summary>Tối đa 3 ảnh nhỏ của các sản phẩm trong đơn, dư thì ghi "+N".</summary>
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
        protected string Money(object value)
        {
            long n = Convert.ToInt64(value);
            return n > 0 ? n.ToString("N0", new CultureInfo("vi-VN")) + "đ" : "";
        }

        protected string CatName(object cat)
        {
            string key = Convert.ToString(cat);
            string name;
            return AppData.Categories.TryGetValue(key, out name) ? name : key;
        }

        protected string RoleName(object role)
        {
            return Convert.ToString(role) == AppData.RoleAdmin ? "Quản trị viên" : "Khách hàng";
        }

        protected string RoleCss(object role)
        {
            return Convert.ToString(role) == AppData.RoleAdmin ? "tag admin" : "tag";
        }

        private void Show(Label label, string text, bool ok)
        {
            label.Text = Server.HtmlEncode(text);
            label.CssClass = ok ? "admin-msg ok" : "admin-msg err";
        }

        private static void Select(DropDownList ddl, string value)
        {
            ListItem item = ddl.Items.FindByValue(value);
            if (item == null) return;
            ddl.ClearSelection();
            item.Selected = true;
        }

        // ================= SẢN PHẨM =================
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

        protected void btnNewProduct_Click(object sender, EventArgs e)
        {
            ClearProductForm();
            litProductFormTitle.Text = "Thêm sản phẩm mới";
            pnlProductForm.Visible = true;
        }

        protected void btnCancelProduct_Click(object sender, EventArgs e)
        {
            pnlProductForm.Visible = false;
        }

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

        private static bool IsSafeImage(string path)
        {
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return true;
            // đường dẫn trong website: không chứa dấu ":" (chặn javascript:, data:...) và không có "//" ở đầu
            return path.IndexOf(':') < 0 && !path.StartsWith("//");
        }

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
        private void ClearCustomerForm()
        {
            hfCustomerId.Value = "0";
            txtCName.Text = "";
            txtCEmail.Text = "";
            txtCUser.Text = "";
            ddlCRole.ClearSelection();
        }

        protected void btnNewCustomer_Click(object sender, EventArgs e)
        {
            ClearCustomerForm();
            litCustomerFormTitle.Text = "Thêm tài khoản mới";
            pnlCustomerForm.Visible = true;
        }

        protected void btnCancelCustomer_Click(object sender, EventArgs e)
        {
            pnlCustomerForm.Visible = false;
        }

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

        protected void btnSaveCustomer_Click(object sender, EventArgs e)
        {
            string fullName = txtCName.Text.Trim();
            string email = txtCEmail.Text.Trim();
            string username = txtCUser.Text.Trim();
            string password = txtCPass.Text;

            int id;
            int.TryParse(hfCustomerId.Value, out id);

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
            if ((id == 0 || password.Length > 0) && password.Length < 8)
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
            bool ok = id == 0
                ? AppData.AddCustomer(c, password, out error)
                : AppData.UpdateCustomer(c, password, out error);

            if (!ok)
            {
                Show(lblCustomerMsg, error, false);
                return;
            }

            // Admin sửa chính tài khoản của mình -> cập nhật lại phiên đăng nhập
            if (id != 0 && id == Convert.ToInt32(Session["UserId"]))
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
            Show(lblCustomerMsg, id > 0 ? "Đã cập nhật tài khoản." : "Đã thêm tài khoản mới.", true);
        }
    }
}
