using System;

namespace Demo_btl_10_10
{
    /// <summary>Một bài viết ở trang Tin tức (lưu trong Application).</summary>
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }         // các đoạn cách nhau bằng một dòng trống
        public string Image { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
