const btn = document.getElementById("btnQR");

btn.addEventListener("click", async () => {

    btn.disabled = true;
    btn.innerText = "Đang tạo...";

    const id = btn.dataset.id;

    try {
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

        document.getElementById("stk").innerText = data.soTaiKhoan;
        document.getElementById("chuTK").innerText = data.tenTaiKhoan;
        document.getElementById("noiDung").innerText = data.noiDung;
        document.getElementById("bankName").innerText = data.nganHang;

        btn.remove();

        checkPayment(data.maNap);

    } catch (err) {
        console.error(err);
        alert("Có lỗi xảy ra, vui lòng thử lại.");
        btn.disabled = false;
        btn.innerText = "TẠO MÃ QR";
    }
});

document.querySelectorAll(".copy-btn").forEach(btn => {
    btn.onclick = () => {
        const id = btn.dataset.copy;
        navigator.clipboard.writeText(document.getElementById(id).innerText);
        btn.innerText = "Đã copy";
        setTimeout(() => {
            btn.innerText = "Copy";
        }, 1500);
    };
});

function checkPayment(maNap) {

    const timer = setInterval(async () => {

        try {

            const response = await fetch("/DocGia/CheckStatus?id=" + maNap);
            const data = await response.json();

            if (data.status === "DaThanhToan") {

                clearInterval(timer);

                // Hiện popup xử lý khi SePay đã xác nhận
                document.getElementById("paymentProcessing").style.display = "flex";

                // Chờ 2 giây để tạo hiệu ứng
                setTimeout(() => {

                    document.getElementById("paymentProcessing").style.display = "none";
                    document.getElementById("paymentSuccess").style.display = "flex";

                }, 2000);
            }

        } catch (err) {

            console.error("Lỗi kiểm tra trạng thái:", err);

        }

    }, 2000);

    // Dừng polling sau 15 phút
    setTimeout(() => clearInterval(timer), 15 * 60 * 1000);
}