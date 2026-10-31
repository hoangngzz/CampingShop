(function () {
    // ===== Ô tìm kiếm: chuyển sang Products.aspx?q=... =====
    var searchInput = document.getElementById('searchInput');
    var searchBtn = document.getElementById('searchBtn');
    if (searchInput && searchBtn) {
        var goSearch = function () {
            var q = searchInput.value.trim();
            location.href = 'Products.aspx' + (q ? '?q=' + encodeURIComponent(q) : '');
        };
        searchBtn.addEventListener('click', goSearch);
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault(); // tránh postback của <form runat="server">
                goSearch();
            }
        });
        // Giữ lại từ khóa trên ô tìm kiếm
        var m = /[?&]q=([^&]*)/.exec(location.search);
        if (m) searchInput.value = decodeURIComponent(m[1].replace(/\+/g, ' '));
    }
    // ===== Menu "Sắp xếp": chạm để mở / đóng =====
    var sortDd = document.querySelector('.sort-dropdown');
    var sortBtn = sortDd && sortDd.querySelector('.sort-toggle');
    if (sortDd && sortBtn) {
        var setSortOpen = function (open) {
            sortDd.classList.toggle('open', open);
            sortBtn.setAttribute('aria-expanded', open ? 'true' : 'false');
        };
        sortBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            setSortOpen(!sortDd.classList.contains('open'));
        });
        document.addEventListener('click', function (e) {
            if (!sortDd.contains(e.target)) setSortOpen(false);
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') setSortOpen(false);
        });
    }
    // ===== Thông báo giữa màn hình (do server hiện sau khi thêm vào giỏ): tự ẩn sau ~2 giây =====
    var toast = document.getElementById('mcToast');
    if (toast) {
        setTimeout(function () { toast.classList.remove('show'); }, 2000);
    }
    var body = document.body;
    var nav = document.getElementById('mainNav');
    var toggle = document.getElementById('navToggle');
    var closeBtn = document.getElementById('navClose');
    var overlay = document.getElementById('navOverlay');
    if (!nav || !toggle) return;

    // ===== Menu trượt trên mobile =====
    function setOpen(open) {
        body.classList.toggle('nav-open', open);
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    }
    toggle.addEventListener('click', function () {
        setOpen(!body.classList.contains('nav-open'));
    });
    if (closeBtn) closeBtn.addEventListener('click', function () { setOpen(false); });
    if (overlay) overlay.addEventListener('click', function () { setOpen(false); });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') setOpen(false);
    });
    window.addEventListener('resize', function () {
        if (window.innerWidth > 900) setOpen(false);
    });

    // Mũi tên mở/đóng danh mục con (chỉ hiện trên mobile)
    Array.prototype.forEach.call(nav.querySelectorAll('.dd-toggle'), function (b) {
        b.addEventListener('click', function () {
            var li = b.closest('.has-dropdown');
            var open = li.classList.toggle('open');
            b.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    });

    // ===== Đánh dấu mục menu của trang hiện tại =====
    var page = (location.pathname.split('/').pop() || 'default.aspx').toLowerCase();
    if (page === 'productdetail.aspx') page = 'products.aspx';
    Array.prototype.forEach.call(nav.querySelectorAll('.nav-list > .nav-item > .nav-link'), function (a) {
        var target = (a.getAttribute('href') || '').split('?')[0].toLowerCase();
        if (target === page) a.classList.add('active');
    });
})();