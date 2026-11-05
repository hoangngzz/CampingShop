using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo_btl_10_10
{
    public partial class _default : System.Web.UI.Page
    {
        // Được ASP.NET gọi khi trang chủ tải; lấy danh sách sản phẩm hiện tại rồi chia thành
        // nhóm nổi bật (bốn sản phẩm đầu) và nhóm mới nhất (bốn sản phẩm cuối) để truyền cho
        // hai điều khiển ProductCards hiển thị trên giao diện.
        protected void Page_Load(object sender, EventArgs e)
        {
            // Dữ liệu lấy từ Application nên admin thêm/sửa/xóa sản phẩm là trang chủ tự cập nhật
            List<ShopProduct> all = AppData.GetProducts();
            gridFeatured.Products = all.Take(4).ToList();                       // 4 sản phẩm đầu danh sách
            gridLatest.Products = all.Skip(Math.Max(0, all.Count - 4)).ToList(); // 4 sản phẩm mới thêm gần nhất
        }
    }
}
