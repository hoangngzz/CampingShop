using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.SessionState;

namespace Demo_btl_10_10
{
    /// <summary>Một dòng trong giỏ hàng: sản phẩm + màu đã chọn + số lượng. Tên, giá, ảnh luôn lấy mới từ AppData.</summary>
    [Serializable]
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Color { get; set; }           // "" nếu sản phẩm không có màu
        public int Qty { get; set; }
    }

    /// <summary>
    /// Giỏ hàng của từng khách, lưu trong Session (mỗi người một giỏ riêng).
    /// Dữ liệu dùng chung của cửa hàng thì nằm ở Application (xem AppData).
    /// </summary>
    public static class ShopCart
    {
        const string Key = "MyShop.Cart";
        public const int MaxQty = 99;

        /// <summary>Thông báo ngắn (hiện giữa màn hình một lần ở Site.Master) sau khi trang chuyển hướng.</summary>
        // Lưu nội dung và cờ thành công/lỗi vào Session để Site.Master đọc và hiển thị sau redirect.
        public static void Notify(HttpSessionState session, string text, bool ok)
        {
            session["MyShop.Notice"] = text;
            session["MyShop.NoticeOk"] = ok;
        }

        // Lấy giỏ hàng của phiên hiện tại; nếu Session chưa có giỏ thì tạo List mới, lưu lại và trả về.
        public static List<CartItem> Get(HttpSessionState session)
        {
            List<CartItem> cart = session[Key] as List<CartItem>;   // as = ép kiểu an toàn, ko đúng kiểu trả về null
            if (cart == null)
            {
                cart = new List<CartItem>();
                session[Key] = cart;
            }
            return cart;
        }

        // Đếm tổng số lượng sản phẩm trong giỏ mà không tạo giỏ mới khi Session chưa có dữ liệu.
        public static int Count(HttpSessionState session)
        {
            if (session == null) return 0;
            List<CartItem> cart = session[Key] as List<CartItem>;
            return cart == null ? 0 : cart.Sum(i => i.Qty);
        }

        /// <summary>Thêm vào giỏ; cùng sản phẩm + cùng màu thì cộng dồn số lượng.</summary>
        // Chuẩn hóa màu, tìm biến thể tương ứng và cộng dồn số lượng trong giới hạn 1..MaxQty;
        // nếu chưa có thì thêm một CartItem mới.
        public static void Add(HttpSessionState session, int productId, string color, int qty)
        {
            color = color ?? string.Empty;  //color != null -> giữ nguyên, color == null -> gán = ""
            List<CartItem> cart = Get(session); //lấy giỏ hàng từ session, nếu chưa có thì tạo mới
            CartItem item = Find(cart, productId, color);
            if (item == null)
                cart.Add(new CartItem { ProductId = productId, Color = color, Qty = Math.Min(MaxQty, Math.Max(1, qty)) });
            else
                item.Qty = Math.Min(MaxQty, item.Qty + Math.Max(1, qty));
        }

        // Tìm dòng theo sản phẩm và màu rồi thay đổi số lượng theo delta, luôn giữ số lượng trong
        // khoảng hợp lệ và bỏ qua yêu cầu nếu biến thể chưa tồn tại.
        public static void ChangeQty(HttpSessionState session, int productId, string color, int delta)  //nút + / - trong giỏ hàng
        {
            CartItem item = Find(Get(session), productId, color ?? string.Empty);
            if (item == null) return;
            item.Qty = Math.Min(MaxQty, Math.Max(1, item.Qty + delta)); //đảm bảo 1 dòng sản phầm có slg min = 1
        }

        // Xóa mọi dòng có cùng productId và màu đã chọn khỏi giỏ hàng của Session.
        public static void Remove(HttpSessionState session, int productId, string color)    //nút x trong giỏ hàng
        {
            Get(session).RemoveAll(i => i.ProductId == productId && i.Color == (color ?? string.Empty));
        }

        // Xóa toàn bộ các dòng trong giỏ hàng sau khi đơn hàng đã được tạo thành công.
        public static void Clear(HttpSessionState session)  //xóa sạch giở sau khi gọi đặt hàng thành công
        {
            Get(session).Clear();
        }

        // Tìm dòng giỏ hàng đại diện cho đúng cặp sản phẩm và màu; trả về null nếu chưa có.
        static CartItem Find(List<CartItem> cart, int productId, string color)
        {
            return cart.FirstOrDefault(i => i.ProductId == productId && i.Color == color);
        }
    }
}