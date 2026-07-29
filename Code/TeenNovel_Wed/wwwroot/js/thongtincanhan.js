// TeenNovel_WED — thongtincanhan.js

document.addEventListener('DOMContentLoaded', function () {

    // ── PREVIEW ẢNH AVATAR ───────────────────────────────
    const avatarInput = document.getElementById('avatarInput');
    const avatarPreview = document.getElementById('avatarPreview');
    const avatarDefault = document.getElementById('avatarDefault');

    if (avatarInput) {
        avatarInput.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            // Kiểm tra dung lượng (3MB)
            if (file.size > 3 * 1024 * 1024) {
                alert('Ảnh không được vượt quá 3MB.');
                this.value = '';
                return;
            }

            // Kiểm tra định dạng
            if (!file.type.startsWith('image/')) {
                alert('Vui lòng chọn file ảnh.');
                this.value = '';
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                // Ẩn avatar mặc định, hiện ảnh preview
                if (avatarDefault) avatarDefault.style.display = 'none';
                if (avatarPreview) {
                    avatarPreview.src = e.target.result;
                    avatarPreview.style.display = 'block';
                }
            };
            reader.readAsDataURL(file);
        });
    }

    // ── VALIDATE FORM ────────────────────────────────────
    const form = document.getElementById('profileForm');
    const btnSave = document.getElementById('btnSave');
    const inputTen = document.getElementById('inputTen');
    const inputSdt = document.getElementById('inputSdt');
    const errTen = document.getElementById('errTen');
    const errSdt = document.getElementById('errSdt');

    function setErr(input, errEl, msg) {
        if (errEl) errEl.textContent = msg;
        if (input) input.classList.toggle('has-error', !!msg);
    }
    function clearErr(input, errEl) { setErr(input, errEl, ''); }

    // Real-time validation
    if (inputTen) {
        inputTen.addEventListener('input', function () {
            if (this.value.trim()) clearErr(this, errTen);
        });
    }

    if (inputSdt) {
        inputSdt.addEventListener('blur', function () {
            const val = this.value.trim();
            if (val && !/^[0-9]{9,11}$/.test(val)) {
                setErr(this, errSdt, 'Số điện thoại không hợp lệ (9-11 chữ số).');
            } else {
                clearErr(this, errSdt);
            }
        });
        inputSdt.addEventListener('input', function () {
            clearErr(this, errSdt);
        });
    }

    // Submit validation
    if (form) {
        form.addEventListener('submit', function (e) {
            let valid = true;

            // Kiểm tra tên
            if (inputTen && !inputTen.value.trim()) {
                setErr(inputTen, errTen, 'Họ tên không được để trống.');
                valid = false;
            }

            // Kiểm tra SĐT nếu có nhập
            if (inputSdt && inputSdt.value.trim()) {
                if (!/^[0-9]{9,11}$/.test(inputSdt.value.trim())) {
                    setErr(inputSdt, errSdt, 'Số điện thoại không hợp lệ (9-11 chữ số).');
                    valid = false;
                }
            }

            if (!valid) {
                e.preventDefault();
                // Scroll đến lỗi đầu tiên
                const firstErr = form.querySelector('.has-error');
                if (firstErr) firstErr.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return;
            }

            // Disable nút tránh submit 2 lần
            if (btnSave) {
                btnSave.disabled = true;
                btnSave.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang lưu...';
            }
        });
    }

    // ── CẬP NHẬT TÊN HIỂN THỊ REALTIME ─────────────────
    if (inputTen) {
        inputTen.addEventListener('input', function () {
            const nameEl = document.querySelector('.pf-username');
            if (nameEl && this.value.trim()) {
                nameEl.textContent = this.value.trim();
            }

            // Cập nhật chữ cái đầu trong avatar mặc định
            if (avatarDefault && this.value.trim()) {
                avatarDefault.textContent = this.value.trim().charAt(0).toUpperCase();
            }
        });
    }

});
