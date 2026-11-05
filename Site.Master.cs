using System;
using System.Web;
using System.Web.UI;

namespace Demo_btl_10_10
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected bool IsLoggedIn
        {
            get { return HttpContext.Current.Session["User"] != null; }
        }

        protected bool IsAdmin
        {
            get { return AppData.IsAdmin(HttpContext.Current.Session); }
        }

        // Số sản phẩm trong giỏ (giỏ nằm trong Session của từng khách)
        protected int CartCount
        {
            get { return ShopCart.Count(HttpContext.Current.Session); }
        }

        protected string DisplayName
        {
            get { return Convert.ToString(HttpContext.Current.Session["FullName"]); }
        }

        // Chứa thông báo do ShopCart.Notify ghi vào Session; thông báo được đọc, hiển thị một lần
        // ở master page rồi xóa khỏi Session để không lặp lại sau lần tải trang tiếp theo.
        protected string NoticeText;
        protected bool NoticeOk = true;

        // Đọc nội dung và trạng thái thành công/lỗi của thông báo tạm trong Session trước khi
        // giao diện master được render, sau đó xóa cả hai khóa để thông báo chỉ xuất hiện một lần.
        protected void Page_PreRender(object sender, EventArgs e)
        {
            NoticeText = Session["MyShop.Notice"] as string;
            if (NoticeText != null)
            {
                NoticeOk = !(Session["MyShop.NoticeOk"] is bool) || (bool)Session["MyShop.NoticeOk"];
                Session.Remove("MyShop.Notice");
                Session.Remove("MyShop.NoticeOk");
            }
        }
    }
}