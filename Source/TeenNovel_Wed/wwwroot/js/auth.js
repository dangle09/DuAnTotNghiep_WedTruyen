// TeenNovel — auth.js
// Validation form đăng nhập + đăng ký

// ─── HELPERS ──────────────────────────────────────────────

function getEl(id) { return document.getElementById(id); }

function isValidEmail(email) {
    // Kiểm tra có @ không và định dạng cơ bản
    if (!email.includes('@')) return false;
    const parts = email.split('@');
    if (parts.length !== 2) return false;
    const local  = parts[0];
    const domain = parts[1];
    if (!local || local.length < 1)     return false;
    if (!domain || !domain.includes('.')) return false;
    const domainParts = domain.split('.');
    const tld = domainParts[domainParts.length - 1];
    if (!tld || tld.length < 2)         return false;
    return true;
}

function isValidUsername(username) {
    // 4-30 ký tự, chỉ chữ cái, số, dấu gạch dưới
    return /^[a-zA-Z0-9_]{4,30}$/.test(username);
}

function isValidPassword(password) {
    // Ít nhất 6 ký tự
    return password.length >= 6;
}

function setError(fieldId, msgId, message) {
    const field = getEl(fieldId);
    const msg   = getEl(msgId);
    if (field) field.classList.add('has-error');
    if (msg)   msg.textContent = message;
}

function clearError(fieldId, msgId) {
    const field = getEl(fieldId);
    const msg   = getEl(msgId);
    if (field) field.classList.remove('has-error');
    if (msg)   msg.textContent = '';
}

function setLoading(btn, loading) {
    if (!btn) return;
    btn.disabled    = loading;
    btn.textContent = loading ? 'Đang xử lý...' : btn.dataset.label;
}

// ─── TOGGLE MẬT KHẨU ─────────────────────────────────────

function setupTogglePassword(inputId, toggleId, iconId) {
    const input  = getEl(inputId);
    const toggle = getEl(toggleId);
    const icon   = getEl(iconId);
    if (!input || !toggle) return;

    toggle.addEventListener('click', function () {
        const isHidden = input.type === 'password';
        input.type     = isHidden ? 'text' : 'password';
        if (icon) icon.textContent = isHidden ? '🙈' : '👁';
    });
}

// ─── VALIDATE REAL-TIME (blur) ────────────────────────────

function setupRealtimeEmailValidation(inputId, errorId) {
    const input = getEl(inputId);
    if (!input) return;

    input.addEventListener('blur', function () {
        const val = this.value.trim();
        if (!val) {
            setError('fieldEmail', errorId, 'Email không được để trống.');
        } else if (!isValidEmail(val)) {
            if (!val.includes('@')) {
                setError('fieldEmail', errorId, 'Email phải chứa ký tự @.');
            } else {
                setError('fieldEmail', errorId, 'Email không đúng định dạng.');
            }
        } else {
            clearError('fieldEmail', errorId);
        }
    });

    input.addEventListener('input', function () {
        if (this.value.trim()) {
            clearError('fieldEmail', errorId);
        }
    });
}

// ─── FORM ĐĂNG NHẬP ──────────────────────────────────────

function initLoginForm() {
    const form    = getEl('loginForm');
    const btn     = getEl('loginBtn');
    const emailEl = getEl('loginEmail');
    const passEl  = getEl('loginPassword');

    if (!form) return;

    if (btn) btn.dataset.label = btn.textContent;

    setupTogglePassword('loginPassword', 'togglePassword', 'eyeIcon');
    setupRealtimeEmailValidation('loginEmail', 'emailError');

    // Real-time password
    if (passEl) {
        passEl.addEventListener('blur', function () {
            if (!this.value) {
                setError('fieldPassword', 'passwordError', 'Vui lòng nhập mật khẩu.');
            } else {
                clearError('fieldPassword', 'passwordError');
            }
        });
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        let valid = true;
        const email    = emailEl ? emailEl.value.trim() : '';
        const password = passEl  ? passEl.value         : '';

        // --- Validate email ---
        if (!email) {
            setError('fieldEmail', 'emailError', 'Email không được để trống.');
            valid = false;
        } else if (!email.includes('@')) {
            setError('fieldEmail', 'emailError', 'Email phải chứa ký tự @.');
            valid = false;
        } else if (!isValidEmail(email)) {
            setError('fieldEmail', 'emailError', 'Email không đúng định dạng (vd: ten@gmail.com).');
            valid = false;
        } else {
            clearError('fieldEmail', 'emailError');
        }

        // --- Validate mật khẩu ---
        if (!password) {
            setError('fieldPassword', 'passwordError', 'Vui lòng nhập mật khẩu.');
            valid = false;
        } else {
            clearError('fieldPassword', 'passwordError');
        }

        if (!valid) return;

        setLoading(btn, true);
        form.submit();
    });
}

// ─── FORM ĐĂNG KÝ ────────────────────────────────────────

function initRegisterForm() {
    const form       = getEl('registerForm');
    const btn        = getEl('registerBtn');
    const usernameEl = getEl('regUsername');
    const emailEl    = getEl('regEmail');
    const passEl     = getEl('regPassword');

    if (!form) return;

    if (btn) btn.dataset.label = btn.textContent;

    setupTogglePassword('regPassword', 'toggleRegPassword', 'eyeIconReg');
    setupRealtimeEmailValidation('regEmail', 'emailError');

    // Real-time username
    if (usernameEl) {
        usernameEl.addEventListener('blur', function () {
            const val = this.value.trim();
            if (!val) {
                setError('fieldUsername', 'usernameError', 'Tên đăng nhập không được để trống.');
            } else if (val.length < 4) {
                setError('fieldUsername', 'usernameError', 'Tên đăng nhập phải có ít nhất 4 ký tự.');
            } else if (!isValidUsername(val)) {
                setError('fieldUsername', 'usernameError', 'Chỉ dùng chữ cái, số và dấu gạch dưới (_).');
            } else {
                clearError('fieldUsername', 'usernameError');
            }
        });
        usernameEl.addEventListener('input', function () {
            if (this.value.trim()) clearError('fieldUsername', 'usernameError');
        });
    }

    // Real-time password
    if (passEl) {
        passEl.addEventListener('blur', function () {
            const val = this.value;
            if (!val) {
                setError('fieldPassword', 'passwordError', 'Vui lòng nhập mật khẩu.');
            } else if (!isValidPassword(val)) {
                setError('fieldPassword', 'passwordError', 'Mật khẩu phải có ít nhất 6 ký tự.');
            } else {
                clearError('fieldPassword', 'passwordError');
            }
        });
        passEl.addEventListener('input', function () {
            if (this.value) clearError('fieldPassword', 'passwordError');
        });
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        let valid = true;
        const username = usernameEl ? usernameEl.value.trim() : '';
        const email    = emailEl    ? emailEl.value.trim()    : '';
        const password = passEl     ? passEl.value            : '';

        // --- Validate tên đăng nhập ---
        if (!username) {
            setError('fieldUsername', 'usernameError', 'Tên đăng nhập không được để trống.');
            valid = false;
        } else if (username.length < 4) {
            setError('fieldUsername', 'usernameError', 'Tên đăng nhập phải có ít nhất 4 ký tự.');
            valid = false;
        } else if (username.length > 30) {
            setError('fieldUsername', 'usernameError', 'Tên đăng nhập không được quá 30 ký tự.');
            valid = false;
        } else if (!isValidUsername(username)) {
            setError('fieldUsername', 'usernameError', 'Chỉ dùng chữ cái, số và dấu gạch dưới (_).');
            valid = false;
        } else {
            clearError('fieldUsername', 'usernameError');
        }

        // --- Validate email ---
        if (!email) {
            setError('fieldEmail', 'emailError', 'Email không được để trống.');
            valid = false;
        } else if (!email.includes('@')) {
            setError('fieldEmail', 'emailError', 'Email phải chứa ký tự @.');
            valid = false;
        } else if (!isValidEmail(email)) {
            setError('fieldEmail', 'emailError', 'Email không đúng định dạng (vd: ten@gmail.com).');
            valid = false;
        } else {
            clearError('fieldEmail', 'emailError');
        }

        // --- Validate mật khẩu ---
        if (!password) {
            setError('fieldPassword', 'passwordError', 'Vui lòng nhập mật khẩu.');
            valid = false;
        } else if (!isValidPassword(password)) {
            setError('fieldPassword', 'passwordError', 'Mật khẩu phải có ít nhất 6 ký tự.');
            valid = false;
        } else {
            clearError('fieldPassword', 'passwordError');
        }

        if (!valid) return;

        setLoading(btn, true);
        form.submit();
    });
}
