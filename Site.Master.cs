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

        // Thông báo do ShopCart.Notify ghi vào Session: đọc ra, hiện giữa màn hình một lần rồi xóa đi.
        protected string NoticeText;
        protected bool NoticeOk = true;

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