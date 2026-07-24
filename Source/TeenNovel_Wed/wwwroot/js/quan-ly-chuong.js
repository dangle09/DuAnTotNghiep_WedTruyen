// TeenNovel_WED — quan-ly-chuong.js

function initChuongForm() {
    const form = document.getElementById('chuongForm');
    if (!form) return;

    const tenchuong = document.getElementById('tenchuong');
    const thutu     = document.getElementById('thutu');
    const noidung   = document.getElementById('noidung');
    const wordCount = document.getElementById('wordCount');
    const submitBtn = document.getElementById('submitBtn');

    // Đếm từ realtime
    function updateWordCount() {
        if (!noidung || !wordCount) return;
        const text = noidung.value.trim();
        const words = text ? text.split(/\s+/).length : 0;
        wordCount.textContent = words.toLocaleString('vi-VN') + ' từ';
    }
    if (noidung) {
        noidung.addEventListener('input', updateWordCount);
        updateWordCount();
    }

    form.addEventListener('submit', function (e) {
        let valid = true;

        if (tenchuong && !tenchuong.value.trim()) {
            showErr('err-tenchuong', 'Tên chương không được để trống.');
            tenchuong.classList.add('error');
            valid = false;
        } else if (tenchuong) {
            clearErr('err-tenchuong');
            tenchuong.classList.remove('error');
        }

        if (thutu && (!thutu.value || parseInt(thutu.value) < 1)) {
            showErr('err-thutu', 'Thứ tự phải >= 1.');
            thutu.classList.add('error');
            valid = false;
        } else if (thutu) {
            clearErr('err-thutu');
            thutu.classList.remove('error');
        }

        if (noidung && !noidung.value.trim()) {
            showErr('err-noidung', 'Nội dung chương không được để trống.');
            noidung.classList.add('error');
            valid = false;
        } else if (noidung) {
            clearErr('err-noidung');
            noidung.classList.remove('error');
        }

        if (!valid) {
            e.preventDefault();
            return;
        }

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang lưu...';
        }
    });
}

function showErr(id, msg) {
    const el = document.getElementById(id);
    if (el) el.textContent = msg;
}
function clearErr(id) {
    const el = document.getElementById(id);
    if (el) el.textContent = '';
}
