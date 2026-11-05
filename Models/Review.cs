using System;

namespace Demo_btl_10_10
{
    /// <summary>Một đánh giá (số sao + bình luận) của khách cho một sản phẩm, lưu trong Application.</summary>
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string AuthorName { get; set; }
        public int Rating { get; set; }             // 1..5 sao
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Tạo bản sao của đánh giá trước khi trả dữ liệu ra ngoài vùng Application dùng chung.
        public Review Clone()
        {
            return (Review)MemberwiseClone();
        }
    }
}
