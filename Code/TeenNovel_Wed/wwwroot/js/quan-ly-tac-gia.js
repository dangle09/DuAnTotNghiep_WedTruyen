// ==========================================================
// TeenNovel_WED - quan-ly-tac-gia.js
// ==========================================================

function initTacGiaForm() {

    const form = document.getElementById("tacGiaForm");
    if (!form) return;

    const submitBtn = document.getElementById("submitBtn");
    const uploadArea = document.getElementById("uploadArea");
    const fileInput = document.getElementById("anhdaidien");
    const previewImg = document.getElementById("previewImg");
    const placeholder = document.getElementById("uploadPlaceholder");

    // =========================
    // CLICK CHỌN ẢNH
    // =========================

    if (uploadArea && fileInput) {

        uploadArea.addEventListener("click", function (e) {

            if (e.target !== fileInput)
                fileInput.click();

        });

    }

    // =========================
    // DRAG & DROP
    // =========================

    if (uploadArea) {

        ["dragenter", "dragover"].forEach(evt => {

            uploadArea.addEventListener(evt, function (e) {

                e.preventDefault();
                uploadArea.classList.add("dragging");

            });

        });

        ["dragleave", "drop"].forEach(evt => {

            uploadArea.addEventListener(evt, function (e) {

                e.preventDefault();
                uploadArea.classList.remove("dragging");

            });

        });

        uploadArea.addEventListener("drop", function (e) {

            if (e.dataTransfer.files.length) {

                fileInput.files = e.dataTransfer.files;

                previewImage(fileInput.files[0]);

            }

        });

    }

    // =========================
    // PREVIEW
    // =========================

    if (fileInput) {

        fileInput.addEventListener("change", function () {

            if (this.files.length)
                previewImage(this.files[0]);

        });

    }

    function previewImage(file) {

        if (!file) return;

        if (!file.type.startsWith("image/")) {

            showError("err-anh", "Vui lòng chọn file ảnh.");

            return;

        }

        if (file.size > 5 * 1024 * 1024) {

            showError("err-anh", "Ảnh không được vượt quá 5MB.");

            return;

        }

        clearError("err-anh");

        const reader = new FileReader();

        reader.onload = function (e) {

            previewImg.src = e.target.result;

            previewImg.style.display = "block";

            placeholder.style.display = "none";

        }

        reader.readAsDataURL(file);

    }

    // =========================
    // SUBMIT
    // =========================

    form.addEventListener("submit", function (e) {

        e.preventDefault();

        clearAllErrors();

        let valid = true;

        //-------------------
        // Tên
        //-------------------

        const ten = document.getElementById("tentacgia");

        if (ten) {

            const value = ten.value.trim();

            if (value === "") {

                showError("err-ten", "Tên tác giả không được để trống.");

                ten.classList.add("error");

                valid = false;

            }

            else if (value.length > 100) {

                showError("err-ten", "Tên tác giả tối đa 100 ký tự.");

                ten.classList.add("error");

                valid = false;

            }

            else {

                ten.classList.remove("error");

            }

        }

        //-------------------

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

//=====================================================
// Helpers
//=====================================================

function showError(id, text) {

    const el = document.getElementById(id);

    if (el)
        el.innerText = text;

}

function clearError(id) {

    const el = document.getElementById(id);

    if (el)
        el.innerText = "";

}

function clearAllErrors() {

    clearError("err-ten");
    clearError("err-anh");

}