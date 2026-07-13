// TeenNovel_WED — genre-dropdown.js

(function () {
    const genreItem = document.getElementById('navGenreItem');
    const genreToggle = document.getElementById('navGenreToggle');
    const genreOverlay = document.getElementById('genreOverlay');

    if (!genreItem || !genreToggle) return;

    let closeTimer = null;

    function openDropdown() {
        clearTimeout(closeTimer);
        genreItem.classList.add('open');
        genreOverlay?.classList.add('show');
    }

    function scheduleClose() {
        // Delay nhỏ giúp chuột di chuyển từ toggle xuống dropdown không bị đứt
        closeTimer = setTimeout(function () {
            genreItem.classList.remove('open');
            genreOverlay?.classList.remove('show');
        }, 120);
    }

    // Hover vào li → mở
    genreItem.addEventListener('mouseenter', openDropdown);

    // Rời khỏi li → schedule đóng
    genreItem.addEventListener('mouseleave', scheduleClose);

    // Nếu chuột vào lại dropdown (nằm trong li) thì huỷ đóng
    genreItem.addEventListener('mouseenter', function () {
        clearTimeout(closeTimer);
    });

    // Click toggle (mobile)
    genreToggle.addEventListener('click', function (e) {
        e.stopPropagation();
        genreItem.classList.contains('open') ? scheduleClose() : openDropdown();
    });

    // Click overlay đóng ngay
    genreOverlay?.addEventListener('click', function () {
        clearTimeout(closeTimer);
        genreItem.classList.remove('open');
        genreOverlay.classList.remove('show');
    });

    // ESC đóng
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            clearTimeout(closeTimer);
            genreItem.classList.remove('open');
            genreOverlay?.classList.remove('show');
        }
    });

    // Click link thể loại → đóng
    document.querySelectorAll('.ih-genre-item').forEach(function (item) {
        item.addEventListener('click', function () {
            clearTimeout(closeTimer);
            genreItem.classList.remove('open');
            genreOverlay?.classList.remove('show');
        });
    });
})();
