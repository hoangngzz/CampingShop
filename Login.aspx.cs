using System;
using System.Drawing;
using System.Web.UI;

namespace Demo_btl_10_10
{
    public partial class Login : Page
    {
        // Xử lý việc đăng xuất khi URL có logout=1 ở lần tải trang đầu tiên; xóa toàn bộ Session
        // rồi chuyển người dùng về trang chủ, còn các lần postback thì giữ nguyên phiên hiện tại.
        protected void Page_Load(object sender, EventArgs e)
        {
            // Login.aspx?logout=1 -> đăng xuất
            if (!IsPostBack && Request.QueryString["logout"] == "1")
            {
                Session.Clear();
                Session.Abandon();
                Response.Redirect("~/default.aspx");
            }
        }

        // Kiểm tra dữ liệu đăng nhập đã qua validation, xác thực tên người dùng và mật khẩu,
        // lưu thông tin tài khoản vào Session khi thành công và chuyển admin/khách hàng tới
        // trang phù hợp; nếu thất bại thì hiển thị thông báo lỗi màu đỏ.
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Kiểm tra tài khoản trong danh sách khách hàng lưu ở Application
            Customer user = AppData.Authenticate(username, password);

            if (user != null)
            {
                Session["UserId"] = user.Id;
                Session["User"] = user.Username;
                Session["FullName"] = user.FullName;
                Session["Role"] = user.Role;

                // Admin vào trang quản trị, khách hàng về trang chủ
                if (user.Role == AppData.RoleAdmin)
                    Response.Redirect("~/Admin.aspx");
                else
                    Response.Redirect("~/default.aspx");
            }
            else
            {
                lblMessage.ForeColor = Color.Red;
                lblMessage.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }
        }
    }
}
