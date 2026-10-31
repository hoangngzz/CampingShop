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

        // Đổ đánh giá sau khi nút gửi đã xử lý, để thấy ngay đánh giá vừa gửi
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

        // ===== Hàm hỗ trợ cho giao diện =====
        protected string Money(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

        /// <summary>Chuỗi sao đầy/rỗng, ví dụ 4 sao: ★★★★☆</summary>
        protected string Stars(object rating)
        {
            int full = (int)Math.Round(Convert.ToDouble(rating), MidpointRounding.AwayFromZero);
            full = Math.Min(5, Math.Max(0, full));
            return new string('★', full) + new string('☆', 5 - full);
        }

        private void ShowAdd(string text)
        {
            lblAddMsg.Text = Server.HtmlEncode(text);
            lblAddMsg.CssClass = "admin-msg err";
        }

        private void ShowReview(string text, bool ok)
        {
            lblReviewMsg.Text = Server.HtmlEncode(text);
            lblReviewMsg.CssClass = ok ? "admin-msg ok" : "admin-msg err";
        }

        // ===== Thêm vào giỏ (lưu cả màu đã chọn) =====
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

        // ===== Gửi đánh giá =====
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