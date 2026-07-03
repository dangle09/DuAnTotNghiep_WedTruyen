// TeenNovel_WED — chi-tiet-actions.js

// ── TOGGLE FORM ĐÁNH GIÁ ─────────────────────────────────
function toggleDanhGia() {
    const form = document.getElementById('formDanhGia');
    const btn  = document.getElementById('btnDanhGia');
    if (!form) return;

    const isOpen = form.style.display !== 'none';
    form.style.display = isOpen ? 'none' : 'block';

    if (btn) {
        btn.innerHTML = isOpen
            ? '<i class="bi bi-star-half"></i> Đánh giá'
            : '<i class="bi bi-x-lg"></i> Đóng';
    }

    // Scroll đến form khi mở
    if (!isOpen) {
        setTimeout(() => form.scrollIntoView({ behavior: 'smooth', block: 'nearest' }), 50);
    }
}

// ── CHỌN SAO ─────────────────────────────────────────────
function pickStar(val) {
    const stars = document.querySelectorAll('.ct-star-btn');
    const hidSao  = document.getElementById('hidSao');
    const label   = document.getElementById('starLabel');
    const submitBtn = document.getElementById('btnSubmitDG');

    stars.forEach(function (btn) {
        const v = parseInt(btn.dataset.val);
        btn.classList.toggle('on', v <= val);
    });

    if (hidSao)  hidSao.value = val;
    if (label) {
        const labels = ['', 'Tệ', 'Không hay', 'Bình thường', 'Hay', 'Xuất sắc'];
        label.textContent = val + ' sao — ' + (labels[val] || '');
    }

    // Bật nút submit sau khi chọn sao
    if (submitBtn) submitBtn.disabled = false;
}

// Hover preview sao
document.addEventListener('DOMContentLoaded', function () {
    const stars = document.querySelectorAll('.ct-star-btn');
    const hidSao = document.getElementById('hidSao');
    const submitBtn = document.getElementById('btnSubmitDG');

    // Disable submit nếu chưa chọn sao
    if (submitBtn && (!hidSao || hidSao.value === '0')) {
        submitBtn.disabled = true;
    }

    stars.forEach(function (btn) {
        // Hover hiện preview
        btn.addEventListener('mouseenter', function () {
            const val = parseInt(this.dataset.val);
            stars.forEach(function (b) {
                b.style.color = parseInt(b.dataset.val) <= val ? '#f59e0b' : '';
            });
        });

        // Rời chuột thì về trạng thái đã chọn
        btn.addEventListener('mouseleave', function () {
            const selected = parseInt(hidSao?.value ?? '0');
            stars.forEach(function (b) {
                const v = parseInt(b.dataset.val);
                b.style.color = (selected > 0 && v <= selected) ? '#f59e0b' : '';
            });
        });
    });
});

// ── TOGGLE DANH SÁCH ĐÁNH GIÁ ────────────────────────────
function toggleXemDG() {
    const list = document.getElementById('dsDanhGia');
    const btn  = document.getElementById('btnXemDG');
    const icon = document.getElementById('iconXemDG');
    if (!list) return;

    const isOpen = list.style.display !== 'none';
    list.style.display = isOpen ? 'none' : 'flex';

    if (btn)  btn.classList.toggle('open', !isOpen);
    if (icon) {
        icon.className = isOpen ? 'bi bi-chevron-down' : 'bi bi-chevron-up';
    }

    if (!isOpen) {
        setTimeout(() => list.scrollIntoView({ behavior: 'smooth', block: 'nearest' }), 50);
    }
}
