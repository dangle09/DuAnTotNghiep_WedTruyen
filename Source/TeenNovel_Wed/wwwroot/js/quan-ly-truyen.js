// TeenNovel_WED — quan-ly-truyen.js

function initTruyenForm() {
    const form      = document.getElementById('truyenForm');
    const submitBtn = document.getElementById('submitBtn');
    if (!form) return;

    // ── Preview ảnh bìa ──────────────────────────────────
    const fileInput   = document.getElementById('anhbia');
    const previewImg  = document.getElementById('previewImg');
    const placeholder = document.getElementById('uploadPlaceholder');

    if (fileInput) {
        fileInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            // Kiểm tra dung lượng (5MB)
            if (file.size > 5 * 1024 * 1024) {
                showFieldError('err-anhbia', 'Ảnh không được vượt quá 5MB.');
                this.value = '';
                return;
            }
            clearFieldError('err-anhbia');

            const reader = new FileReader();
            reader.onload = function (e) {
                if (previewImg) {
                    previewImg.src = e.target.result;
                    previewImg.style.display = 'block';
                }
                if (placeholder) placeholder.style.display = 'none';
            };
            reader.readAsDataURL(file);
        });
    }

    // ── Validate submit ──────────────────────────────────
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        let valid = true;

        // Tên truyện
        const tentruyen = document.getElementById('tentruyen');
        if (tentruyen) {
            const val = tentruyen.value.trim();
            if (!val) {
                showFieldError('err-tentruyen', 'Tên truyện không được để trống.');
                tentruyen.classList.add('error');
                valid = false;
            } else if (val.length > 200) {
                showFieldError('err-tentruyen', 'Tên truyện không được quá 200 ký tự.');
                tentruyen.classList.add('error');
                valid = false;
            } else {
                clearFieldError('err-tentruyen');
                tentruyen.classList.remove('error');
            }

            tentruyen.addEventListener('input', function () {
                if (this.value.trim()) {
                    clearFieldError('err-tentruyen');
                    this.classList.remove('error');
                }
            }, { once: false });
        }

        // Thể loại
        const matheloai = document.getElementById('matheloai');
        if (matheloai && !matheloai.value) {
            showFieldError('err-matheloai', 'Vui lòng chọn thể loại.');
            matheloai.classList.add('error');
            valid = false;
        } else if (matheloai) {
            clearFieldError('err-matheloai');
            matheloai.classList.remove('error');
        }

        if (!valid) return;

        // Disable nút tránh submit 2 lần
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang lưu...';
        }

        form.submit();
    });
}

// ── Helpers ──────────────────────────────────────────────

function showFieldError(id, msg) {
    const el = document.getElementById(id);
    if (el) el.textContent = msg;
}

function clearFieldError(id) {
    const el = document.getElementById(id);
    if (el) el.textContent = '';
}
