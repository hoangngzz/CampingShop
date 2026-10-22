using System;

namespace Demo_btl_10_10
{
    /// <summary>Lời nhắn khách gửi từ trang Liên hệ (lưu trong Application).</summary>
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
