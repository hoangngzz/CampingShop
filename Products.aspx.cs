using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo_btl_10_10
{
    public partial class Products : System.Web.UI.Page
    {
        public class Option
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public long Min { get; set; }
            public long Max { get; set; }
        }

        // Các mức giá lọc (Key là giá trị trên ?gia=)
        protected static readonly Option[] PriceRanges =
        {
            new Option { Key = "", Label = "Tất cả mức giá", Min = 0, Max = long.MaxValue },
            new Option { Key = "duoi-500", Label = "Dưới 500.000đ", Min = 0, Max = 499999 },
            new Option { Key = "500-2000", Label = "500.000đ - 2.000.000đ", Min = 500000, Max = 2000000 },
            new Option { Key = "tren-2000", Label = "Trên 2.000.000đ", Min = 2000001, Max = long.MaxValue }
        };

        // Các kiểu sắp xếp (Key là giá trị trên ?sort=)
        protected static readonly Option[] SortOptions =
        {
            new Option { Key = "default", Label = "Mặc định" },
            new Option { Key = "az", Label = "A → Z" },
            new Option { Key = "za", Label = "Z → A" },
            new Option { Key = "gia-tang", Label = "Giá tăng dần" },
            new Option { Key = "gia-giam", Label = "Giá giảm dần" },
            new Option { Key = "moi-nhat", Label = "Hàng mới nhất" },
            new Option { Key = "cu-nhat", Label = "Hàng cũ nhất" }
        };

        protected string Q { get; private set; }
        protected string Loai { get; private set; }
        protected string Gia { get; private set; }
        protected string Sort { get; private set; }

        protected Dictionary<string, string> Categories
        {
            get { return AppData.Categories; }
        }

        protected string CurrentSortLabel
        {
            get { return SortOptions.First(s => s.Key == Sort).Label; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Q = (Request.QueryString["q"] ?? "").Trim();
            Loai = Request.QueryString["loai"] ?? "";
            if (!AppData.Categories.ContainsKey(Loai)) Loai = "";

            Gia = Request.QueryString["gia"] ?? "";
            Option range = PriceRanges.FirstOrDefault(r => r.Key == Gia && r.Key != "");
            if (range == null) Gia = "";

            Sort = Request.QueryString["sort"] ?? "default";
            if (!SortOptions.Any(s => s.Key == Sort)) Sort = "default";

            // Tìm theo từ khóa (không phân biệt dấu) + lọc loại + lọc giá
            string[] words = AppData.Norm(Q).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<ShopProduct> result = AppData.GetProducts().Where(p =>
            {
                string hay = AppData.Norm(p.Name + " " + AppData.CatName(p.Cat));
                return words.All(w => hay.Contains(w));
            });
            if (Loai != "") result = result.Where(p => p.Cat == Loai);
            if (range != null) result = result.Where(p => p.Price >= range.Min && p.Price <= range.Max);

            switch (Sort)
            {
                case "az": result = result.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase); break;
                case "za": result = result.OrderByDescending(p => p.Name, StringComparer.CurrentCultureIgnoreCase); break;
                case "gia-tang": result = result.OrderBy(p => p.Price); break;
                case "gia-giam": result = result.OrderByDescending(p => p.Price); break;
                case "moi-nhat": result = result.OrderByDescending(p => p.Id); break;
                case "cu-nhat": result = result.OrderBy(p => p.Id); break;
            }
            List<ShopProduct> list = result.ToList();

            // Dòng thông tin kết quả khi đang tìm/lọc
            StringBuilder info = new StringBuilder();
            if (Loai != "") info.Append("Danh mục <strong>" + Server.HtmlEncode(AppData.CatName(Loai)) + "</strong>. ");
            if (Q != "") info.Append("Từ khóa “<strong>" + Server.HtmlEncode(Q) + "</strong>”. ");
            if (range != null) info.Append("Mức giá <strong>" + Server.HtmlEncode(range.Label) + "</strong>. ");
            if (info.Length > 0)
            {
                litInfo.Text = info + "Tìm thấy <strong>" + list.Count + "</strong> sản phẩm. <a href=\"Products.aspx\">Xem tất cả sản phẩm</a>";
                pnlInfo.Visible = true;
            }

            gridProducts.Products = list;
            gridProducts.Visible = list.Count > 0;
            pnlNoResult.Visible = list.Count == 0;
        }

        // Ghép địa chỉ trang với các bộ lọc, bỏ những giá trị mặc định
        protected string Link(string q, string loai, string gia, string sort)
        {
            List<string> parts = new List<string>();
            if (!string.IsNullOrEmpty(q)) parts.Add("q=" + Uri.EscapeDataString(q));
            if (!string.IsNullOrEmpty(loai)) parts.Add("loai=" + Uri.EscapeDataString(loai));
            if (!string.IsNullOrEmpty(gia)) parts.Add("gia=" + Uri.EscapeDataString(gia));
            if (!string.IsNullOrEmpty(sort) && sort != "default") parts.Add("sort=" + Uri.EscapeDataString(sort));
            return "Products.aspx" + (parts.Count > 0 ? "?" + string.Join("&", parts) : "");
        }
    }
}
