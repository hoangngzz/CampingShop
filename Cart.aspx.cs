using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace Demo_btl_10_10
{
    public partial class Cart : System.Web.UI.Page
    {
        /// <summary>Một dòng giỏ hàng để hiển thị: thông tin sản phẩm lấy mới từ Application.</summary>
        public class CartLine
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Image { get; set; }
            public string Color { get; set; }
            public long Price { get; set; }
            public int Qty { get; set; }
            public bool InStock { get; set; }
            // Tính tổng tiền của một dòng bằng đơn giá nhân số lượng hiện tại của dòng đó.
            public long LineTotal { get { return Price * Qty; } }
            // Tạo khóa duy nhất gồm ID sản phẩm và màu đã chọn để các nút trong repeater
            // có thể gửi đúng biến thể sản phẩm qua CommandArgument.
            public string Key { get { return ProductId + "|" + Color; } }   // dùng làm CommandArgument
        }

        // Ở lần tải đầu tiên, điền tên người nhận từ Session để khách đã đăng nhập không phải
        // nhập lại thông tin; các lần postback giữ nguyên dữ liệu người dùng đang điền.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Điền sẵn tên người nhận nếu đã đăng nhập
                txtName.Text = Convert.ToString(Session["FullName"]);
            }
        }

        // Sau khi các lệnh thêm, bớt hoặc xóa đã xử lý, dựng lại danh sách dòng hàng, điều khiển
        // vùng rỗng/đã có hàng/đặt hàng thành công, binding repeater và cập nhật tổng tiền.
        protected void Page_PreRender(object sender, EventArgs e)
        {
            int doneId;
            if (!IsPostBack && int.TryParse(Request.QueryString["done"], out doneId) && doneId > 0)
            {
                litOrderId.Text = doneId.ToString();
                pnlDone.Visible = true;
            }

            List<CartLine> lines = BuildLines();
            pnlFilled.Visible = lines.Count > 0;
            pnlEmpty.Visible = lines.Count == 0 && !pnlDone.Visible;

            rptCart.DataSource = lines;
            rptCart.DataBind();

            long sum = lines.Sum(l => l.LineTotal);
            litSub.Text = Server.HtmlEncode(AppData.Money(sum));
            litTotal.Text = Server.HtmlEncode(AppData.Money(sum));
        }

        // Ghép các CartItem trong Session với bản sao sản phẩm mới nhất từ Application để lấy tên,
        // ảnh, giá và tình trạng hàng; đồng thời loại các dòng trỏ tới sản phẩm đã bị xóa.
        private List<CartLine> BuildLines()
        {
            List<CartItem> cart = ShopCart.Get(Session);
            List<CartLine> lines = new List<CartLine>();

            foreach (CartItem item in cart.ToList())
            {
                ShopProduct p = AppData.GetProduct(item.ProductId);
                if (p == null)
                {
                    cart.Remove(item);
                    continue;
                }
                lines.Add(new CartLine
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Image = p.Thumb,
                    Color = item.Color,
                    Price = p.Price,
                    Qty = item.Qty,
                    InStock = p.InStock
                });
            }
            return lines;
        }

        // Chuyển giá trị binding thành chuỗi tiền Việt Nam dùng trong tổng tiền và giá từng dòng.
        protected string Money(object value)
        {
            return AppData.Money(Convert.ToInt64(value));
        }

        // Tạo HTML hiển thị màu đã chọn sau khi mã hóa giá trị; không tạo markup nếu sản phẩm
        // không có màu hoặc màu đang rỗng.
        protected string ColorHtml(object color)
        {
            string c = Convert.ToString(color);
            return c.Length == 0 ? string.Empty : "<small class=\"cart-color\">Màu: " + Server.HtmlEncode(c) + "</small>";
        }

        // Phân tích CommandArgument theo định dạng productId|color; trả về ID 0 và màu rỗng
        // khi khóa không có dấu phân cách hoặc ID không chuyển đổi được.
        private static void ParseKey(string key, out int productId, out string color)
        {
            int i = key.IndexOf('|');
            productId = 0;
            color = string.Empty;
            if (i < 0) return;
            int.TryParse(key.Substring(0, i), out productId);
            color = key.Substring(i + 1);
        }

        // Xử lý các nút tăng, giảm và xóa trên một dòng giỏ hàng, gọi ShopCart thay đổi Session
        // tương ứng rồi chuyển hướng về giỏ để hiển thị dữ liệu mới nhất.
        protected void rptCart_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId;
            string color;
            ParseKey(Convert.ToString(e.CommandArgument), out productId, out color);
            if (productId == 0) return;

            if (e.CommandName == "plus") ShopCart.ChangeQty(Session, productId, color, 1);
            else if (e.CommandName == "minus") ShopCart.ChangeQty(Session, productId, color, -1);
            else if (e.CommandName == "del") ShopCart.Remove(Session, productId, color);

            Response.Redirect("Cart.aspx");
        }

        // Ghi thông báo lỗi đặt hàng vào Label sau khi mã hóa nội dung và gán CSS lỗi cho giao diện.
        private void ShowCheckout(string text)
        {
            lblCheckoutMsg.Text = Server.HtmlEncode(text);
            lblCheckoutMsg.CssClass = "admin-msg err";
        }

        // Kiểm tra giỏ, thông tin người nhận, số điện thoại, địa chỉ và tình trạng tồn kho; nếu hợp lệ
        // thì chuyển các dòng giỏ thành Order, lưu đơn vào Application, xóa giỏ và mở màn hình thành công.
        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            List<CartLine> lines = BuildLines();
            if (lines.Count == 0)
            {
                Response.Redirect("Cart.aspx");
                return;
            }

            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();

            if (name.Length == 0) { ShowCheckout("Vui lòng nhập họ và tên."); return; }
            if (!Regex.IsMatch(phone, @"^[0-9+\s.\-]{8,15}$")) { ShowCheckout("Số điện thoại không hợp lệ."); return; }
            if (address.Length == 0) { ShowCheckout("Vui lòng nhập địa chỉ nhận hàng."); return; }

            CartLine soldOut = lines.FirstOrDefault(l => !l.InStock);
            if (soldOut != null)
            {
                ShowCheckout("Sản phẩm \"" + soldOut.Name + "\" đã hết hàng, vui lòng xóa khỏi giỏ để tiếp tục.");
                return;
            }

            Order order = new Order
            {
                CustomerId = Session["UserId"] == null ? 0 : Convert.ToInt32(Session["UserId"]),
                ReceiverName = name,
                Phone = phone,
                Address = address,
                Note = txtNote.Text.Trim()
            };
            foreach (CartLine l in lines)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = l.ProductId,
                    ProductName = l.Name,
                    Color = l.Color,
                    Image = l.Image,
                    Price = l.Price,
                    Qty = l.Qty
                });
            }

            int id = AppData.AddOrder(order);
            ShopCart.Clear(Session);
            Response.Redirect("Cart.aspx?done=" + id);
        }
    }
}
