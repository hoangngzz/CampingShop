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

        public Customer Clone()
        {
            return (Customer)MemberwiseClone();
        }
    }
}
