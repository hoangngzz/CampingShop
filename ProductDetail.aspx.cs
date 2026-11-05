using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Demo_btl_10_10
{
    public partial class ProductDetail : System.Web.UI.Page
    {
        protected ShopProduct Product;      // null nếu không tìm thấy
        protected int ImageIndex;           // ảnh đang xem
        protected double AvgRating;
        protected int RatingCount;
        protected int SelectedRating;       // số sao đã chọn (giữ lại khi gửi lỗi)

        static readonly CultureInfo Vi = new CultureInfo("vi-VN");

        protected string MainImage
        {
            get { return Product.Images.Count > 0 ? Product.Images[ImageIndex] : "Images/product1.jpg"; }
        }

        protected string AvgText
        {
            get { return AvgRating.ToString("0.0", Vi); }
        }

        // Đọc ID sản phẩm và ảnh từ URL, tải sản phẩm, thiết lập trạng thái không tìm thấy hoặc
        // dữ liệu chi tiết; khởi tạo màu, trạng thái thêm giỏ, biểu mẫu đánh giá và sản phẩm liên quan.
        protected void Page_Load(object sender, EventArgs e)
        {
            int id;
            int.TryParse(Request.QueryString["id"], out id);
            Product = AppData.GetProduct(id);

            if (Product == null)
            {
                pnlDetail.Visible = false;
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return;
            }

            Title = Product.Name + " - MyCamp";

            int img;
            int.TryParse(Request.QueryString["img"], out img);
            ImageIndex = Product.Images.Count == 0 ? 0 : Math.Min(Math.Max(0, img), Product.Images.Count - 1);

            // Danh sách màu: chỉ nạp lần đầu, lần postback ASP.NET tự khôi phục lựa chọn của khách
            pnlColors.Visible = Product.Colors.Count > 0;
            if (!IsPostBack)
            {
                foreach (string c in Product.Colors) rblColors.Items.Add(new ListItem(c, c));
                if (rblColors.Items.Count > 0) rblColors.SelectedIndex = 0;

                pnlAdded.Visible = Request.QueryString["added"] == "1";
                if (Request.QueryString["reviewed"] == "1") ShowReview("Cảm ơn bạn đã đánh giá sản phẩm!", true);
            }

            if (!Product.InStock)
            {
                btnAdd.Enabled = false;
                btnAdd.Text = "Hết hàng";
            }

            bool loggedIn = Session["UserId"] != null;
            pnlReviewForm.Visible = loggedIn;
            pnlLoginHint.Visible = !loggedIn;

            // Sản phẩm liên quan: ưu tiên cùng danh mục, đủ 4 sản phẩm
            List<ShopProduct> others = AppData.GetProducts().Where(p => p.Id != Product.Id).ToList();
            gridRelated.Products = others.Where(p => p.Cat == Product.Cat)
                .Concat(others.Where(p => p.Cat != Product.Cat)).Take(4).ToList();
        }

        // Tải lại điểm trung bình và các đánh giá sau khi event handler đã xử lý, binding repeater
        // và bật/tắt vùng danh sách hoặc thông báo chưa có đánh giá.
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Product == null) return;

            AppData.GetRating(Product.Id, out AvgRating, out RatingCount);
            List<Review> reviews = AppData.GetReviews(Product.Id);
            rptReviews.DataSource = reviews;
            rptReviews.DataBind();
            rptReviews.Visible = reviews.Count > 0;
            pnlNoReviews.Visible = reviews.Count == 0;
        }

        // Chuyển giá trị số từ binding thành chuỗi tiền Việt Nam để hiển thị giá sản phẩm.
        protected string Money(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

        /// <summary>Chuyển điểm đánh giá thành đúng năm ký hiệu sao đầy/rỗng.</summary>
        /// <remarks>Làm tròn theo quy tắc away-from-zero, sau đó giới hạn trong khoảng 0-5
        /// để dữ liệu ngoài phạm vi không làm hỏng giao diện.</remarks>
        protected string Stars(object rating)
        {
            int full = (int)Math.Round(Convert.ToDouble(rating), MidpointRounding.AwayFromZero);
            full = Math.Min(5, Math.Max(0, full));
            return new string('★', full) + new string('☆', 5 - full);
        }

        // Hiển thị thông báo liên quan đến thao tác thêm giỏ và gán CSS lỗi cho Label của biểu mẫu.
        private void ShowAdd(string text)
        {
            lblAddMsg.Text = Server.HtmlEncode(text);
            lblAddMsg.CssClass = "admin-msg err";
        }

        // Hiển thị thông báo đánh giá, mã hóa nội dung và chọn CSS thành công hoặc lỗi theo tham số ok.
        private void ShowReview(string text, bool ok)
        {
            lblReviewMsg.Text = Server.HtmlEncode(text);
            lblReviewMsg.CssClass = ok ? "admin-msg ok" : "admin-msg err";
        }

        // Kiểm tra sản phẩm và số lượng, xác nhận màu được chọn thuộc danh sách hợp lệ, sau đó thêm
        // biến thể vào Session cart và chuyển hướng lại trang với cờ báo đã thêm.
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (Product == null || !Product.InStock) return;

            int qty;
            if (!int.TryParse(txtQty.Text, out qty) || qty < 1) qty = 1;
            qty = Math.Min(qty, ShopCart.MaxQty);

            string color = string.Empty;
            if (Product.Colors.Count > 0)
            {
                color = rblColors.SelectedValue;
                if (!Product.Colors.Contains(color))
                {
                    ShowAdd("Vui lòng chọn màu sắc.");
                    return;
                }
            }

            ShopCart.Add(Session, Product.Id, color, qty);
            ShopCart.Notify(Session, "Đã thêm vào giỏ hàng", true);
            Response.Redirect("ProductDetail.aspx?id=" + Product.Id + "&added=1");
        }

        // Kiểm tra quyền đăng nhập, điểm sao và nội dung bình luận; tạo hoặc cập nhật đánh giá qua
        // AppData, hiển thị lỗi khi không hợp lệ và chuyển hướng để hiển thị đánh giá mới khi thành công.
        protected void btnReview_Click(object sender, EventArgs e)
        {
            if (Product == null) return;

            if (Session["UserId"] == null)
            {
                ShowReview("Vui lòng đăng nhập để đánh giá.", false);
                return;
            }

            int rating;
            int.TryParse(Request.Form["rating"], out rating);
            SelectedRating = rating;
            string comment = txtComment.Text.Trim();

            if (rating < 1 || rating > 5)
            {
                ShowReview("Vui lòng chọn số sao.", false);
                return;
            }
            if (comment.Length == 0)
            {
                ShowReview("Vui lòng nhập bình luận.", false);
                return;
            }
            if (comment.Length > 1000)
            {
                ShowReview("Bình luận tối đa 1000 ký tự.", false);
                return;
            }

            Review review = new Review
            {
                ProductId = Product.Id,
                CustomerId = Convert.ToInt32(Session["UserId"]),
                AuthorName = Convert.ToString(Session["FullName"]),
                Rating = rating,
                Comment = comment
            };

            string error;
            if (!AppData.SaveReview(review, out error))
            {
                ShowReview(error, false);
                return;
            }

            Response.Redirect("ProductDetail.aspx?id=" + Product.Id + "&reviewed=1#reviews");
        }
    }
}