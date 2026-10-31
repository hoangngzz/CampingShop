using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Demo_btl_10_10
{
    /// <summary>Trang theo dõi đơn hàng: khách đã đăng nhập xem các đơn của chính mình và trạng thái xử lý.</summary>
    public partial class MyOrders : Page
    {
        protected List<Order> Items = new List<Order>();
        protected int OpenId;   // đơn được mở sẵn (Orders.aspx?id=...), mặc định là đơn mới nhất

        protected void Page_Load(object sender, EventArgs e)
        {
            // Phải đăng nhập mới xem được đơn của mình
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Quản trị viên không mua hàng, xem đơn của mọi khách ở trang quản trị
            if (AppData.IsAdmin(Session))
            {
                Response.Redirect("~/Admin.aspx?tab=dh");
                return;
            }

            Items = AppData.GetOrdersByCustomer(Convert.ToInt32(Session["UserId"]));

            int.TryParse(Request.QueryString["id"], out OpenId);
            if (OpenId == 0 && Items.Count > 0) OpenId = Items[0].Id;
        }

        protected string Money(long value)
        {
            return AppData.Money(value);
        }

        protected string StatusCss(string status)
        {
            switch (status)
            {
                case AppData.StatusPending: return "tag pending";
                case AppData.StatusShipping: return "tag shipping";
                case AppData.StatusCancelled: return "tag cancel";
                default: return "tag";
            }
        }

        protected string ColorHtml(string color)
        {
            return string.IsNullOrEmpty(color) ? string.Empty : "<small class=\"cart-color\">Màu: " + Server.HtmlEncode(color) + "</small>";
        }
    }
}