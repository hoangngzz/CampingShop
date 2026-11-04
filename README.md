# 🏕️ CampingShop – Website bán đồ cắm trại

**CampingShop** là website bán hàng trực tuyến chuyên về đồ cắm trại (lều, đèn, ba lô, thiết bị, xe đạp), xây dựng bằng **ASP.NET Web Forms** và **C#**. Dự án được thực hiện cho môn Lập trình Web, hướng tới một cửa hàng online đơn giản, dễ dùng và dễ mở rộng.

## ✨ Chức năng

**Dành cho khách hàng**

- 🏠 **Trang chủ:** giới thiệu cửa hàng và các sản phẩm nổi bật.
- 🛍️ **Danh sách sản phẩm:** lọc theo danh mục, tìm kiếm theo tên.
- 🔎 **Chi tiết sản phẩm:** xem ảnh, giá, chất liệu, kích thước, chọn màu, chọn số lượng.
- ⭐ **Đánh giá:** đăng nhập để chấm 1–5 sao và bình luận. Mỗi tài khoản chỉ có một đánh giá cho mỗi sản phẩm (gửi lại sẽ ghi đè). Trang hiển thị điểm trung bình và số lượt đánh giá.
- 🧺 **Giỏ hàng:** thêm, tăng/giảm số lượng, xóa sản phẩm. Cùng sản phẩm và cùng màu thì được cộng dồn.
- 📦 **Đặt hàng:** nhập thông tin người nhận (tên, số điện thoại, địa chỉ, ghi chú). Khách chưa đăng nhập vẫn đặt hàng được.
- 📋 **Đơn hàng của tôi:** thành viên xem lại các đơn đã đặt và trạng thái từng đơn.
- 📝 **Đăng ký / 🔐 Đăng nhập:** mật khẩu được băm bằng SHA-256, không lưu dạng chữ thường.

**Dành cho quản trị viên** (trang `Admin.aspx`)

- Quản lý **sản phẩm:** thêm, sửa, xóa (xóa sản phẩm sẽ xóa luôn đánh giá của sản phẩm đó).
- Quản lý **tài khoản:** sửa thông tin, đổi vai trò, đặt lại mật khẩu, xóa. Luôn phải còn ít nhất một quản trị viên.
- Quản lý **đơn hàng:** xem chi tiết và đổi trạng thái: `Chờ xử lý` → `Đang giao` → `Đã giao` hoặc `Đã hủy`.

> Tài khoản quản trị mẫu: `admin` / `admin`. **Hãy đổi mật khẩu này trước khi đưa website lên chạy thật.**

## 🛠️ Công nghệ

| Công nghệ | Vai trò |
|---|---|
| ASP.NET Web Forms (.NET Framework) | Khung ứng dụng web, Master Page, User Control |
| C# | Xử lý logic phía máy chủ |
| Newtonsoft.Json | Đọc/ghi dữ liệu ra file JSON |
| HTML, CSS, JavaScript | Giao diện và tương tác phía trình duyệt |
| Visual Studio | Môi trường phát triển |
| Git & GitHub | Quản lý phiên bản, làm việc nhóm |

## 🗄️ Cách lưu dữ liệu

Dự án **không dùng cơ sở dữ liệu SQL**:

- Sản phẩm, tài khoản, đánh giá và đơn hàng nằm trong `Application[...]` để đọc nhanh.
- Mỗi lần thêm/sửa/xóa, toàn bộ dữ liệu được ghi ra một file duy nhất: `App_Data/data.json`. Khi website khởi động (`Application_Start` trong `Global.asax`), dữ liệu được nạp lại từ file này.
- Mọi thao tác đọc/ghi dùng `Application.Lock()` / `UnLock()` để nhiều người truy cập cùng lúc không gây lỗi.
- Giỏ hàng của từng khách lưu trong `Session`, nên mất khi hết phiên làm việc.
- Chưa có `data.json` (hoặc muốn làm lại từ đầu) thì xóa file đó và chạy lại: website sẽ tạo dữ liệu mẫu gồm 8 sản phẩm, 4 đánh giá và tài khoản `admin`.

Các danh mục sản phẩm cố định: Lều cắm trại, Đèn chiếu sáng, Ba lô - túi, Thiết bị cắm trại, Xe đạp.

## 📂 Cấu trúc dự án

```text
CampingShop/
├── App_Data/
│   └── data.json              (dữ liệu của cửa hàng, tự sinh khi chạy)
├── Content/
│   ├── Site.css
│   └── header.js
├── Controls/
│   └── ProductCards.ascx      (lưới thẻ sản phẩm dùng chung)
├── Images/                    (ảnh banner và ảnh sản phẩm)
├── Models/
│   ├── AppData.cs             (kho dữ liệu, đọc/ghi data.json)
│   ├── ShopProduct.cs         (sản phẩm)
│   ├── Customer.cs            (khách hàng và quản trị viên)
│   ├── Review.cs              (đánh giá)
│   ├── Order.cs               (đơn hàng và chi tiết đơn)
│   └── ShopCart.cs            (giỏ hàng trong Session)
├── default.aspx               (trang chủ)
├── Products.aspx              (danh sách, lọc, tìm kiếm)
├── ProductDetail.aspx         (chi tiết và đánh giá)
├── Cart.aspx                  (giỏ hàng và đặt hàng)
├── Orders.aspx                (đơn hàng của tôi)
├── Login.aspx / SignUp.aspx   (đăng nhập, đăng ký)
├── Admin.aspx                 (quản trị)
├── Site.Master                (bố cục chung)
├── Global.asax                (khởi tạo dữ liệu khi website chạy)
├── Web.config
└── BTL LTW.sln
```

## 🚀 Chạy dự án

### Yêu cầu

- Windows
- Visual Studio có workload **ASP.NET and web development**
- .NET Framework Developer Pack đúng phiên bản khai báo trong file `.csproj`
- Kết nối mạng lần đầu để khôi phục gói NuGet

### Các bước

1. Clone repository:

   ```bash
   git clone https://github.com/hoangngzz/CampingShop.git
   ```

2. Mở file `BTL LTW.sln` bằng Visual Studio.
3. Khôi phục gói NuGet nếu Visual Studio yêu cầu.
4. Chọn **Build → Clean Solution**, sau đó **Build → Rebuild Solution**.
5. Đặt `default.aspx` làm trang khởi động, rồi nhấn `F5` (hoặc `Ctrl + F5`) để chạy.

Nếu website báo lỗi không ghi được dữ liệu, kiểm tra thư mục `App_Data` có quyền ghi. Dữ liệu vẫn dùng được trong bộ nhớ nhưng sẽ mất khi website khởi động lại.

## 🔭 Hướng phát triển

- Chuyển nơi lưu dữ liệu từ `data.json` sang SQL Server.
- Thêm thanh toán trực tuyến và quản lý tồn kho.
- Thêm phân trang, sắp xếp và lọc theo khoảng giá.
- Cho phép tải ảnh sản phẩm lên từ trang quản trị.

## 👥 Phát triển nhóm

Dự án được làm theo nhóm trên GitHub. Khi đóng góp, hãy tạo branch riêng, kiểm tra thay đổi trước khi commit và dùng Pull Request để review code.

---

**CampingShop** – dự án học tập về phát triển website bán hàng với ASP.NET Web Forms và C#.
