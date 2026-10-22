using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo_btl_10_10
{
    public partial class News : System.Web.UI.Page
    {
        protected Article Item;     // bài đang xem, null khi ở trang danh sách

        protected void Page_Load(object sender, EventArgs e)
        {
            List<Article> all = AppData.GetArticles();

            // News.aspx -> danh sách, News.aspx?id=N -> chi tiết bài viết
            if (Request.QueryString["id"] == null)
            {
                rptNews.DataSource = all;
                rptNews.DataBind();
                return;
            }

            int id;
            int.TryParse(Request.QueryString["id"], out id);
            Item = AppData.GetArticle(id);

            pnlList.Visible = false;
            if (Item == null)
            {
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return;
            }

            Title = Item.Title + " - MyCamp";
            pnlDetail.Visible = true;

            // Nội dung: mỗi đoạn cách nhau một dòng trống, mã hóa HTML trước khi in
            StringBuilder sb = new StringBuilder();
            foreach (string para in Item.Content.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries))
                sb.Append("<p>").Append(Server.HtmlEncode(para.Trim())).Append("</p>");
            litContent.Text = sb.ToString();

            rptOther.DataSource = all.Where(a => a.Id != Item.Id).Take(4).ToList();
            rptOther.DataBind();
        }
    }
}
