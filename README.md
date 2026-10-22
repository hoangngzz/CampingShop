# 🛍️ MyShop – Website bán hàng

**MyShop** là dự án website bán hàng được xây dựng bằng ASP.NET Web Forms và C#. Dự án hướng đến việc xây dựng giao diện mua sắm trực tuyến đơn giản, trực quan và dễ sử dụng, phục vụ mục đích học tập và thực hành phát triển ứng dụng web.

## 📌 Giới thiệu dự án

MyShop là dự án được xây dựng nhằm phục vụ mục đích nghiên cứu và học tập môn Web Programming (Lập trình Web). Dự án tập trung vào việc tìm hiểu, thực hành và vận dụng các kiến thức về phát triển ứng dụng web thông qua việc xây dựng một website bán hàng minh họa.
Thông qua dự án, nhóm hướng đến việc rèn luyện kỹ năng xây dựng giao diện người dùng, xử lý logic phía máy chủ, tổ chức cấu trúc ứng dụng web và quản lý mã nguồn bằng Git/GitHub.
Dự án sử dụng ASP.NET Web Forms, C#, HTML, CSS và JavaScript, tạo nền tảng để tìm hiểu quy trình phát triển một ứng dụng web và từng bước mở rộng các chức năng trong tương lai.

## ✨ Chức năng chính

- 🏠 **Trang chủ:** Giao diện giới thiệu và hiển thị sản phẩm.
- 🛒 **Danh sách sản phẩm:** Hiển thị các sản phẩm trên website.
- 🔎 **Chi tiết sản phẩm:** Xem thông tin, chọn màu, xem và gửi đánh giá (số sao, bình luận, điểm trung bình).
- 🧺 **Giỏ hàng:** Quản lý sản phẩm đã chọn (kèm màu) và đặt hàng; đơn được ghi vào `Application`.
- 📰 **Tin tức:** Danh sách và trang chi tiết bài viết.
- ✉️ **Liên hệ:** Thông tin cửa hàng và form gửi lời nhắn (lưu vào `Application`).
- 🛠️ **Quản trị:** Quản lý sản phẩm, khách hàng, đơn hàng (xem chi tiết, đổi trạng thái) và xem lời nhắn từ khách.
- 🔐 **Đăng nhập:** Trang đăng nhập tài khoản.
- 📝 **Đăng ký:** Giao diện đăng ký tài khoản người dùng.

> Lưu ý: Dữ liệu (sản phẩm, khách hàng, đánh giá, đơn hàng) nằm trong `Application[...]`, nạp ở `Application_Start` (Global.asax) và mọi thao tác ghi dùng `Application.Lock()` / `UnLock()`. Dữ liệu chỉ nằm trong bộ nhớ nên khi website khởi động lại sẽ quay về dữ liệu mẫu. Giỏ hàng của từng khách lưu trong `Session`. Tài khoản quản trị mẫu: `admin` / `admin`.

## 🛠️ Công nghệ sử dụng

| Công nghệ | Vai trò |
|---|---|
| ASP.NET Web Forms | Xây dựng ứng dụng web |
| C# | Xử lý logic phía máy chủ |
| HTML5 | Xây dựng cấu trúc trang |
| CSS3 | Thiết kế và định dạng giao diện |
| JavaScript | Xử lý tương tác phía trình duyệt |
| Visual Studio | Môi trường phát triển |
| Git & GitHub | Quản lý phiên bản và cộng tác nhóm |

## 📂 Cấu trúc dự án

```text
Thu-main/
├── Content/
│   ├── Site.css
│   └── header.js
├── Controls/
│   └── ProductCards.ascx      (lưới thẻ sản phẩm dùng chung)
├── Models/
│   ├── AppData.cs             (kho dữ liệu trong Application)
│   ├── ShopProduct.cs, Customer.cs, Review.cs, Order.cs
│   ├── Article.cs, ContactMessage.cs
│   └── ShopCart.cs            (giỏ hàng trong Session)
├── Global.asax
├── default.aspx
├── Site.Master
├── Products.aspx
├── ProductDetail.aspx
├── Cart.aspx
├── News.aspx
├── Contact.aspx
├── Admin.aspx
├── Login.aspx
├── SignUp.aspx
├── Web.config
└── README.md
```

*Cấu trúc trên mang tính minh họa; tên và vị trí file thực tế có thể khác tùy phiên bản project.*

## 🚀 Hướng dẫn chạy dự án

### Yêu cầu

- Windows
- Visual Studio có hỗ trợ phát triển ASP.NET trên .NET Framework
- .NET Framework Developer Pack phù hợp với project
- Các package NuGet được khai báo trong project

### Các bước thực hiện

1. Clone repository:

   ```bash
   git clone https://github.com/dthang-19/Thu.git
   ```

2. Mở file solution `.sln` bằng Visual Studio.

3. Khôi phục các package NuGet nếu được yêu cầu.

4. Trên tab Build -> chọn Clean Solution -> Rebuild/Build Solution.

5. Đặt trang khởi động phù hợp, ví dụ `Default.aspx`.

6. Nhấn `F5` hoặc `Ctrl + F5` để chạy website.

## 👥 Phát triển dự án

Dự án được phát triển theo hình thức làm việc nhóm, sử dụng GitHub để quản lý mã nguồn, theo dõi thay đổi và tích hợp các phần công việc của từng thành viên.

Khi đóng góp code, nên tạo branch riêng, kiểm tra thay đổi trước khi commit và sử dụng Pull Request khi cần review code.

## 🎯 Mục đích nghiên cứu và học tập

Tìm hiểu kiến trúc và nguyên lý hoạt động của ứng dụng web.
Thực hành xây dựng giao diện bằng HTML, CSS và JavaScript.
Nghiên cứu cách xử lý sự kiện và logic phía máy chủ với ASP.NET Web Forms và C#.
Tìm hiểu cách tổ chức các trang web, sử dụng Master Page và tái sử dụng thành phần giao diện.
Thực hành quản lý mã nguồn, theo dõi thay đổi và cộng tác nhóm bằng Git/GitHub.
Vận dụng kiến thức lập trình để xây dựng một sản phẩm minh họa có tính thực tiễn phục vụ học tập.


---

**MyShop** — Dự án học tập về phát triển website bán hàng với ASP.NET Web Forms và C#.
"# CampingShop" 
