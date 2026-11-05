using System.Collections.Generic;

namespace Demo_btl_10_10
{
    /// <summary>Một sản phẩm của cửa hàng (lưu trong Application).</summary>
    public class ShopProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public long OldPrice { get; set; }          // 0 = không giảm giá
        public string Cat { get; set; }             // mã loại: leu, den, ba-lo, thiet-bi, xe-dap
        public List<string> Images { get; set; }    // ảnh đầu tiên là ảnh đại diện
        public List<string> Colors { get; set; }    // các màu khách có thể chọn (có thể rỗng nếu sản phẩm chỉ có 1 màu)
        public string Description { get; set; }
        public string Material { get; set; }
        public string Sizes { get; set; }
        public string Status { get; set; }          // còn hàng/ hết hàng

        // Khởi tạo hai danh sách ảnh và màu rỗng để các thao tác AddRange hoặc binding không gặp null.
        public ShopProduct()
        {
            Images = new List<string>();
            Colors = new List<string>();
        }

        // Trả về ảnh đầu tiên làm ảnh đại diện; nếu chưa có ảnh thì dùng ảnh mặc định của website.
        public string Thumb
        {
            get { return Images.Count > 0 ? Images[0] : "Images/product1.jpg"; }
        }

        // Xác định sản phẩm có thể bán dựa trên trạng thái khác chuỗi "Hết hàng".
        public bool InStock
        {
            get { return Status != "Hết hàng"; }        
        }

        // Sao chép sản phẩm và tạo hai List độc lập cho Images/Colors để bản sao có thể được chỉnh
        // sửa an toàn mà không ảnh hưởng dữ liệu gốc trong Application.
        public ShopProduct Clone()
        {
            ShopProduct c = (ShopProduct)MemberwiseClone();             //Các thuộc tính kiểu giá trị như Id, Price, OldPrice được sao chép bình thường
            c.Images = new List<string>(Images);                        //Nhưng Images, Colors vẫn dùng chung danh sách với đối tượng ban đầu.
            c.Colors = new List<string>(Colors);                        //sử dụng new List để tạo hai danh sách mới và sao chép các phần tử sang,
            return c;                                                   //giúp đối tượng bản sao hoạt động độc lập, khi chỉnh sửa danh sách của bản sao sẽ không ảnh hưởng đến đối tượng ban đầu.
        }
    }
}
