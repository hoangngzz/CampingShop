using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Demo_btl_10_10
{
    public partial class ProductCards : System.Web.UI.UserControl
    {
        /// <summary>Danh sách sản phẩm cần hiển thị (trang cha gán ở Page_Load).</summary>
        public IEnumerable<ShopProduct> Products { get; set; }

        // Đổ dữ liệu sau khi các nút "Thêm" đã xử lý xong
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            rptCards.DataSource = Products ?? new List<ShopProduct>();
            rptCards.DataBind();
        }

        // Nút "Thêm" trên thẻ: thêm 1 sản phẩm (màu đầu tiên) vào giỏ rồi tải lại trang
        protected void rptCards_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "add") return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            ShopProduct p = AppData.GetProduct(id);
            if (p != null && p.InStock)
                ShopCart.Add(Session, p.Id, p.Colors.Count > 0 ? p.Colors[0] : string.Empty, 1);

            Response.Redirect(Request.RawUrl);
        }

        protected string Money(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

        protected string SaleBadge(object oldPrice)
        {
            return Convert.ToInt64(oldPrice) > 0 ? "<span class=\"badge-sale\">Sale</span>" : string.Empty;
        }

        protected string OldPrice(object oldPrice)
        {
            long n = Convert.ToInt64(oldPrice);
            return n > 0 ? "<span class=\"p-old\">" + Server.HtmlEncode(AppData.Money(n)) + "</span>" : string.Empty;
        }
    }
}
