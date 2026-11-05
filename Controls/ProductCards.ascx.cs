using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Demo_btl_10_10
{
    public partial class ProductCards : System.Web.UI.UserControl
    {
        /// <summary>Danh sách sản phẩm cần hiển thị (trang cha gán ở Page_Load).</summary>
        public IEnumerable<ShopProduct> Products { get; set; }

        // Đổ danh sách Products vào repeater ngay trước khi điều khiển được render; nếu trang cha
        // chưa gán danh sách thì dùng danh sách rỗng để giao diện không bị lỗi null.
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            rptCards.DataSource = Products ?? new List<ShopProduct>();
            rptCards.DataBind();
        }

        // Xử lý lệnh add từ một thẻ sản phẩm: đọc ID, kiểm tra sản phẩm còn tồn tại và còn hàng,
        // thêm một đơn vị với màu đầu tiên vào Session cart, tạo thông báo một lần rồi tải lại URL.
        protected void rptCards_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "add") return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            ShopProduct p = AppData.GetProduct(id);
            if (p != null && p.InStock)
            {
                ShopCart.Add(Session, p.Id, p.Colors.Count > 0 ? p.Colors[0] : string.Empty, 1);
                ShopCart.Notify(Session, "Đã thêm vào giỏ hàng", true);
            }

            Response.Redirect(Request.RawUrl);
        }

        // Định dạng giá trị số thành chuỗi tiền Việt Nam để binding trực tiếp trong mẫu thẻ sản phẩm.
        protected string Money(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

        // Trả về badge Sale khi sản phẩm có giá cũ lớn hơn 0; nếu không giảm giá thì trả về HTML rỗng.
        protected string SaleBadge(object oldPrice)
        {
            return Convert.ToInt64(oldPrice) > 0 ? "<span class=\"badge-sale\">Sale</span>" : string.Empty;
        }

        // Định dạng giá cũ thành HTML có mã hóa; chỉ hiển thị khi giá cũ hợp lệ và lớn hơn 0.
        protected string OldPrice(object oldPrice)
        {
            long n = Convert.ToInt64(oldPrice);
            return n > 0 ? "<span class=\"p-old\">" + Server.HtmlEncode(AppData.Money(n)) + "</span>" : string.Empty;
        }
    }
}