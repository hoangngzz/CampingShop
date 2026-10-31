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
        public static void Notify(HttpSessionState session, string text, bool ok)
        {
            session["MyShop.Notice"] = text;
            session["MyShop.NoticeOk"] = ok;
        }

        public static List<CartItem> Get(HttpSessionState session)
        {
            List<CartItem> cart = session[Key] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                session[Key] = cart;
            }
            return cart;
        }

        public static int Count(HttpSessionState session)
        {
            if (session == null) return 0;
            List<CartItem> cart = session[Key] as List<CartItem>;
            return cart == null ? 0 : cart.Sum(i => i.Qty);
        }

        /// <summary>Thêm vào giỏ; cùng sản phẩm + cùng màu thì cộng dồn số lượng.</summary>
        public static void Add(HttpSessionState session, int productId, string color, int qty)
        {
            color = color ?? string.Empty;
            List<CartItem> cart = Get(session);
            CartItem item = Find(cart, productId, color);
            if (item == null)
                cart.Add(new CartItem { ProductId = productId, Color = color, Qty = Math.Min(MaxQty, Math.Max(1, qty)) });
            else
                item.Qty = Math.Min(MaxQty, item.Qty + Math.Max(1, qty));
        }

        public static void ChangeQty(HttpSessionState session, int productId, string color, int delta)
        {
            CartItem item = Find(Get(session), productId, color ?? string.Empty);
            if (item == null) return;
            item.Qty = Math.Min(MaxQty, Math.Max(1, item.Qty + delta));
        }

        public static void Remove(HttpSessionState session, int productId, string color)
        {
            Get(session).RemoveAll(i => i.ProductId == productId && i.Color == (color ?? string.Empty));
        }

        public static void Clear(HttpSessionState session)
        {
            Get(session).Clear();
        }

        static CartItem Find(List<CartItem> cart, int productId, string color)
        {
            return cart.FirstOrDefault(i => i.ProductId == productId && i.Color == color);
        }
    }
}