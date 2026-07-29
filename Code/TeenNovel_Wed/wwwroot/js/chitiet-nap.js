const btn = document.getElementById("btnQR");

btn.addEventListener("click", async () => {

    btn.disabled = true;
    btn.innerText = "Đang tạo...";

    const id = btn.dataset.id;

    const res = await fetch("/DocGia/TaoDonNap?id=" + id, {
        method: "POST"
    });

    const data = await res.json();

    if (!data.success) {

        alert(data.message);

        btn.disabled = false;
        btn.innerText = "TẠO MÃ QR";

        return;
    }

    document.getElementById("imgQR").src = data.qrUrl;
    document.getElementById("imgQR").style.display = "block";

    document.getElementById("stk").innerText =
        data.soTaiKhoan;

    document.getElementById("chuTK").innerText =
        data.chuTaiKhoan;

    document.getElementById("noiDung").innerText =
        data.noiDung;

    document.getElementById("bankName").innerText =
        data.nganHang;

    btn.remove();

});

document.querySelectorAll(".copy-btn").forEach(btn => {

    btn.onclick = () => {

        let id = btn.dataset.copy;

        navigator.clipboard.writeText(
            document.getElementById(id).innerText
        );

        btn.innerText = "Đã copy";

        setTimeout(() => {

            btn.innerText = "Copy";

        }, 1500);

    };

});