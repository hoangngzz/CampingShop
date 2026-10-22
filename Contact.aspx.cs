using System;
using System.Text.RegularExpressions;

namespace Demo_btl_10_10
{
    public partial class Contact : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            if (Request.QueryString["sent"] == "1")
            {
                Show("Cảm ơn bạn! Lời nhắn đã được gửi, chúng tôi sẽ liên hệ lại sớm.", true);
            }
            else if (Session["UserId"] != null)
            {
                // Điền sẵn tên và email nếu đã đăng nhập
                txtName.Text = Convert.ToString(Session["FullName"]);
                Customer c = AppData.GetCustomer(Convert.ToInt32(Session["UserId"]));
                if (c != null) txtEmail.Text = c.Email;
            }
        }

        private void Show(string text, bool ok)
        {
            lblMsg.Text = Server.HtmlEncode(text);
            lblMsg.CssClass = ok ? "admin-msg ok" : "admin-msg err";
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string subject = txtSubject.Text.Trim();
            string message = txtMessage.Text.Trim();

            if (name.Length == 0) { Show("Vui lòng nhập họ và tên.", false); return; }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { Show("Email không hợp lệ.", false); return; }
            if (phone.Length > 0 && !Regex.IsMatch(phone, @"^[0-9+\s.\-]{8,15}$")) { Show("Số điện thoại không hợp lệ.", false); return; }
            if (message.Length < 10) { Show("Nội dung cần có ít nhất 10 ký tự.", false); return; }
            if (message.Length > 1000) { Show("Nội dung tối đa 1000 ký tự.", false); return; }

            AppData.AddMessage(new ContactMessage
            {
                Name = name,
                Email = email,
                Phone = phone,
                Subject = subject.Length == 0 ? "(Không có chủ đề)" : subject,
                Message = message
            });

            Response.Redirect("Contact.aspx?sent=1");
        }
    }
}
