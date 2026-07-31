using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TeenNovel_Wed.Data;

namespace TeenNovel_Wed.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SePayController : ControllerBase
{
    private readonly TeenNovelDbContext _context;
    private readonly IConfiguration _configuration;

    public SePayController(TeenNovelDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        // 1. Đọc raw body (bắt buộc để tính HMAC đúng)
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        // 2. Lấy signature + timestamp
        //var signatureHeader = Request.Headers["X-SePay-Signature"].ToString();
        //var timestamp = Request.Headers["X-SePay-Timestamp"].ToString();

        // 3. Xác thực chữ ký — request không hợp lệ thì chặn ngay
        //if (!IsValidSignature(rawBody, timestamp, signatureHeader))
        //{
        //    Console.WriteLine("[SePay Webhook] Chữ ký không hợp lệ — từ chối request.");
        //    return Unauthorized();
        //}

        // 4. Deserialize sau khi verify xong
        SePayWebhook? model;
        try
        {
            model = System.Text.Json.JsonSerializer.Deserialize<SePayWebhook>(
                rawBody,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SePay Webhook] Lỗi parse JSON: {ex.Message}");
            return BadRequest();
        }

        if (model == null)
            return BadRequest();

        // 5. Chỉ xử lý giao dịch tiền vào
        if (!string.Equals(model.TransferType, "in", StringComparison.OrdinalIgnoreCase))
            return Ok();

        // 6. Tìm đơn nạp khớp nội dung chuyển khoản
        var napXu = await _context.NapXus
            .Include(x => x.MaDocGiaNavigation)
            .Where(x => x.Trangthai == "ChoThanhToan")
            .FirstOrDefaultAsync(x =>
        model.Content.Contains(x.NoiDungChuyenKhoan!));

        if (napXu == null)
        {
            Console.WriteLine($"[SePay Webhook] Không tìm thấy đơn khớp với nội dung: {model.Content}");
            return Ok();
        }
        // Nếu đơn đã thanh toán rồi thì bỏ qua
        if (napXu.Trangthai == "DaThanhToan")
        {
            Console.WriteLine($"[SePay Webhook] Đơn {napXu.Manap} đã được xử lý trước đó.");
            return Ok();
        }

        // 7. Kiểm tra đúng số tiền
        if (model.TransferAmount != napXu.Sotien)
        {
            Console.WriteLine($"[SePay Webhook] Sai số tiền. Nhận {model.TransferAmount}, cần {napXu.Sotien}");
            return Ok();
        }

        // 8. Cập nhật trạng thái
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            napXu.Trangthai = "DaThanhToan";
            napXu.Ngaynap = DateTime.Now;
            napXu.MaGiaoDich = model.ReferenceCode;

            napXu.MaDocGiaNavigation.Soxu += napXu.Soxunhan;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return Ok(new { success = true });
    }

    private bool IsValidSignature(string rawBody, string timestamp, string signatureHeader)
    {
        if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(timestamp))
            return false;

        var secretKey = _configuration["SePay:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
            return false;

        var signedPayload = $"{timestamp}.{rawBody}";

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        var computedSignature = "sha256=" + Convert.ToHexString(hashBytes).ToLower();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signatureHeader));
    }
}