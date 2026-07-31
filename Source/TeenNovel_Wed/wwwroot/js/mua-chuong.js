let chapterId = 0;

const modal = document.getElementById("buyModal");
const btnConfirm = document.getElementById("btnConfirmBuy");
const btnCancel = document.getElementById("btnCancelBuy");
const content = document.getElementById("buyContent");

// Click chương mất xu
document.querySelectorAll(".btn-buy-chapter").forEach(btn => {

    btn.addEventListener("click", function (e) {

        e.preventDefault();

        chapterId = this.dataset.id;

        content.innerHTML =
            `Bạn có chắc muốn mua <b>${this.dataset.name}</b><br><br>
             Giá: <b>${this.dataset.price} Xu</b>`;

        modal.classList.add("show");

    });

});

// Hủy
btnCancel.onclick = () => {

    modal.classList.remove("show");

};

// Click nền đen
modal.onclick = (e) => {

    if (e.target === modal)
        modal.classList.remove("show");

};

// Xác nhận mua
btnConfirm.onclick = async () => {

    btnConfirm.disabled = true;
    btnConfirm.innerText = "Đang xử lý...";

    try {

        const res = await fetch("/DocGia/MuaChuong?maChuong=" + chapterId, {

            method: "POST"

        });

        const data = await res.json();

        if (!data.success) {

            alert(data.message);

            return;

        }

        // cập nhật số xu trên menu nếu có
        const soXu = document.getElementById("txtSoXu");

        if (soXu && data.soXu != null)
            soXu.innerText = data.soXu;

        window.location.href = data.redirect;

    }
    finally {

        btnConfirm.disabled = false;
        btnConfirm.innerText = "Xác nhận";

        modal.classList.remove("show");

    }

};