// TeenNovel_WED — quan-ly-nguoidung.js

// ── MODAL XOÁ ────────────────────────────────────────────
function confirmDelete(id, name, type) {
    document.getElementById('deleteName').textContent = name;
    const action = type === 'nv' ? '/QuanLy/XoaNhanVien/' : '/QuanLy/XoaKhachHang/';
    document.getElementById('deleteForm').action = action + id;
    document.getElementById('deleteModal').classList.add('show');
}

// ── MODAL KHOÁ / MỞ KHOÁ ─────────────────────────────────
function confirmToggle(id, name, trangthaiHienTai, type) {
    const dangKhoa = trangthaiHienTai === 'hoatdong'; // sắp khoá
    const icon     = document.getElementById('toggleIcon');
    const title    = document.getElementById('toggleTitle');
    const msg      = document.getElementById('toggleMsg');
    const btn      = document.getElementById('toggleSubmitBtn');

    if (dangKhoa) {
        icon.className = 'nd-modal-icon nd-modal-icon--warn';
        icon.innerHTML = '<i class="bi bi-lock"></i>';
        title.textContent = 'Xác nhận khoá tài khoản';
        msg.innerHTML = `Bạn có chắc muốn khoá tài khoản <strong>${name}</strong>?<br/>Người này sẽ không thể đăng nhập cho đến khi được mở khoá.`;
        btn.textContent = 'Khoá tài khoản';
        btn.style.background = '#ea580c';
        btn.style.borderColor = '#ea580c';
    } else {
        icon.className = 'nd-modal-icon nd-modal-icon--danger';
        icon.style.background = '#f0fdf4';
        icon.style.color = '#16a34a';
        icon.innerHTML = '<i class="bi bi-unlock"></i>';
        title.textContent = 'Xác nhận mở khoá';
        msg.innerHTML = `Bạn có chắc muốn mở khoá tài khoản <strong>${name}</strong>?`;
        btn.textContent = 'Mở khoá';
        btn.style.background = '#16a34a';
        btn.style.borderColor = '#16a34a';
    }

    const action = type === 'nv' ? '/QuanLy/ToggleKhoaNhanVien/' : '/QuanLy/ToggleKhoaKhachHang/';
    document.getElementById('toggleForm').action = action + id;
    document.getElementById('toggleModal').classList.add('show');
}

// ── ĐÓNG MODAL ───────────────────────────────────────────
function closeModal(id) {
    document.getElementById(id).classList.remove('show');
}

document.addEventListener('DOMContentLoaded', function () {
    ['deleteModal', 'toggleModal'].forEach(function (id) {
        const modal = document.getElementById(id);
        if (modal) {
            modal.addEventListener('click', function (e) {
                if (e.target === this) closeModal(id);
            });
        }
    });
});

// ── FORM NHÂN VIÊN ───────────────────────────────────────
function initNhanVienForm() {
    const form = document.getElementById('nvForm');
    if (!form) return;

    const ten     = document.getElementById('ten');
    const email   = document.getElementById('email');
    const matkhau = document.getElementById('matkhau');

    form.addEventListener('submit', function (e) {
        let valid = true;

        if (ten && !ten.value.trim()) {
            showErr('err-ten', 'Họ tên không được để trống.');
            valid = false;
        } else if (ten) clearErr('err-ten');

        if (email) {
            const val = email.value.trim();
            if (!val) {
                showErr('err-email', 'Email không được để trống.');
                valid = false;
            } else if (!val.includes('@')) {
                showErr('err-email', 'Email phải chứa ký tự @.');
                valid = false;
            } else clearErr('err-email');
        }

        if (matkhau) {
            if (!matkhau.value) {
                showErr('err-matkhau', 'Vui lòng nhập mật khẩu.');
                valid = false;
            } else if (matkhau.value.length < 6) {
                showErr('err-matkhau', 'Mật khẩu phải có ít nhất 6 ký tự.');
                valid = false;
            } else clearErr('err-matkhau');
        }

        if (!valid) {
            e.preventDefault();
            return;
        }

        const btn = document.getElementById('submitBtn');
        if (btn) { btn.disabled = true; btn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang lưu...'; }
    });
}

// ── FORM KHÁCH HÀNG ───────────────────────────────────────
function initKhachHangForm() {
    const form = document.getElementById('dgForm');
    if (!form) return;

    const ten  = document.getElementById('ten');
    const soxu = document.getElementById('soxu');

    form.addEventListener('submit', function (e) {
        let valid = true;

        if (ten && !ten.value.trim()) {
            showErr('err-ten', 'Họ tên không được để trống.');
            valid = false;
        } else if (ten) clearErr('err-ten');

        if (soxu && (soxu.value === '' || parseInt(soxu.value) < 0)) {
            showErr('err-soxu', 'Số xu không được âm.');
            valid = false;
        } else if (soxu) clearErr('err-soxu');

        if (!valid) {
            e.preventDefault();
            return;
        }

        const btn = document.getElementById('submitBtn');
        if (btn) { btn.disabled = true; btn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang lưu...'; }
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
