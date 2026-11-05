using System;
using System.Web;

namespace Demo_btl_10_10
{
    public class Global : HttpApplication
    {
        // Được ASP.NET gọi đúng một lần khi ứng dụng khởi động; truyền vùng Application dùng chung
        // vào AppData để nạp dữ liệu từ file, tạo dữ liệu mẫu nếu cần và khởi tạo các bộ đếm ID.
        protected void Application_Start(object sender, EventArgs e)
        {
            AppData.Init(Application);
        }
    }
}
