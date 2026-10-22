using System;
using System.Drawing;
using System.Web.UI;

namespace Demo_btl_10_10
{
    public partial class Signup : Page
    {
        protected void btnSignup_Click(object sender, EventArgs e)
        {
            // Kiểm tra dữ liệu nhập vào
            Page.Validate("Signup");

            if (!Page.IsValid)
            {
                return;
            }

            // Lấy thông tin người dùng nhập
            Customer customer = new Customer
            {
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Role = AppData.RoleCustomer
            };

            // Lưu vào danh sách khách hàng trong Application (Global.asax)
            string error;
            if (!AppData.AddCustomer(customer, txtPassword.Text, out error))
            {
                lblMessage.ForeColor = Color.Red;
                lblMessage.Text = Server.HtmlEncode(error);
                return;
            }

            lblMessage.ForeColor = Color.Green;
            lblMessage.Text =
                "Đăng ký thành công! Xin chào " +
                Server.HtmlEncode(customer.FullName) + ". Bạn có thể <a href=\"Login.aspx\">đăng nhập</a> ngay.";

            // Xóa dữ liệu nhạy cảm sau khi xử lý
            txtPassword.Text = string.Empty;
            txtConfirmPassword.Text = string.Empty;
        }
    }
}
