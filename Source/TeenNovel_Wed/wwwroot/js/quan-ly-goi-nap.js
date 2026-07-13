// TeenNovel_WED - quan-ly-goi-nap.js

function initGoiNapForm() {

    const form = document.getElementById("goiNapForm");
    const submitBtn = document.getElementById("submitBtn");

    if (!form) return;

    form.addEventListener("submit", function (e) {

        e.preventDefault();

        let valid = true;

        clearAllErrors();

        //=========================
        // Tên gói
        //=========================

        const tenGoi = document.getElementById("tengoi");

        if (tenGoi) {

            const value = tenGoi.value.trim();

            if (value === "") {
                showError("err-tengoi", "Tên gói không được để trống.");
                tenGoi.classList.add("error");
                valid = false;
            }
            else if (value.length > 100) {
                showError("err-tengoi", "Tên gói tối đa 100 ký tự.");
                tenGoi.classList.add("error");
                valid = false;
            }
            else {
                tenGoi.classList.remove("error");
            }
        }

        //=========================
        // Giá tiền
        //=========================

        const giaTien = document.getElementById("giatien");

        if (giaTien) {

            const value = Number(giaTien.value);

            if (!giaTien.value) {

                showError("err-giatien", "Vui lòng nhập giá tiền.");

                giaTien.classList.add("error");

                valid = false;
            }
            else if (value <= 0) {

                showError("err-giatien", "Giá tiền phải lớn hơn 0.");

                giaTien.classList.add("error");

                valid = false;
            }
            else {

                giaTien.classList.remove("error");

            }
        }

        //=========================
        // Số xu
        //=========================

        const soXu = document.getElementById("soxu");

        if (soXu) {

            const value = Number(soXu.value);

            if (!soXu.value) {

                showError("err-soxu", "Vui lòng nhập số xu.");

                soXu.classList.add("error");

                valid = false;
            }
            else if (value <= 0) {

                showError("err-soxu", "Số xu phải lớn hơn 0.");

                soXu.classList.add("error");

                valid = false;
            }
            else {

                soXu.classList.remove("error");

            }
        }

        //=========================
        // Submit
        //=========================

        if (!valid)
            return;

        if (submitBtn) {

            submitBtn.disabled = true;

            submitBtn.innerHTML =
                '<i class="bi bi-hourglass-split"></i> Đang lưu...';

        }

        form.submit();

    });

}

//=========================
// Helpers
//=========================

function showError(id, message) {

    const el = document.getElementById(id);

    if (el)
        el.innerText = message;

}

function clearError(id) {

    const el = document.getElementById(id);

    if (el)
        el.innerText = "";

}

function clearAllErrors() {

    clearError("err-tengoi");
    clearError("err-giatien");
    clearError("err-soxu");

}