using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo_btl_10_10
{
    /// <summary>Một dòng hàng trong đơn. Lưu sẵn tên/giá/ảnh tại thời điểm đặt để đơn không đổi khi sản phẩm bị sửa hoặc xóa.</summary>
    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Image { get; set; }
        public long Price { get; set; }
        public int Qty { get; set; }

        // Tính tổng tiền của dòng bằng giá tại thời điểm đặt nhân số lượng đã lưu trong đơn.
        public long LineTotal
        {
            get { return Price * Qty; }
        }
    }

    /// <summary>Đơn hàng (lưu trong Application).</summary>
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }         // 0 = khách chưa đăng nhập
        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> Items { get; set; }

        // Khởi tạo danh sách Items rỗng để mọi Order mới có thể thêm dòng hàng ngay lập tức.
        public Order()
        {
            Items = new List<OrderItem>();
        }

        // Cộng tổng LineTotal của tất cả dòng hàng để lấy giá trị đơn hàng hiện tại.
        public long Total
        {
            get { return Items.Sum(i => i.LineTotal); }
        }

        // Cộng số lượng của tất cả dòng hàng để biết tổng số sản phẩm trong đơn.
        public int TotalQty
        {
            get { return Items.Sum(i => i.Qty); }
        }

        // Sao chép thông tin đơn và tạo danh sách OrderItem mới để chỉnh sửa bản sao không làm
        // thay đổi danh sách dòng hàng của đơn gốc trong Application.
        public Order Clone()
        {
            Order c = (Order)MemberwiseClone();
            c.Items = Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Color = i.Color,
                Image = i.Image,
                Price = i.Price,
                Qty = i.Qty
            }).ToList();
            return c;
        }
    }
}
