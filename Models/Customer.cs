using System;

namespace Demo_btl_10_10
{
    /// <summary>Tài khoản khách hàng / quản trị viên (lưu trong Application).</summary>
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }            // "admin" hoặc "customer"
        public DateTime CreatedAt { get; set; }

        // Tạo một bản sao nông của tài khoản để mã gọi bên ngoài có thể đọc/chỉnh sửa bản sao
        // mà không tác động trực tiếp tới đối tượng Customer đang được lưu trong Application.
        public Customer Clone()
        {
            return (Customer)MemberwiseClone();     // sao chép từng trường sang một đối tượng mới (sao chép nông),
                                                    // AppData luôn trả về bản sao, nên trang nào lấy tài khoản ra sửa cũng không làm hỏng dữ liệu gốc dùng chung.
        }
    }
}
