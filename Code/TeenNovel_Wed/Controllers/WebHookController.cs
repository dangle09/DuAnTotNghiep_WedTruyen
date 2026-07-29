using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> Webhook([FromBody] SePayWebhook model)
    {
        // Kiểm tra Secret Key
        if (Request.Headers["Authorization"] !=
            $"Bearer {_configuration["SePay:SecretKey"]}")
        {
            return Unauthorized();
        }

        var napXu = await _context.NapXus
            .Include(x => x.MaDocGiaNavigation)
            .FirstOrDefaultAsync(x =>
                x.NoiDungChuyenKhoan == model.Content &&
                x.Trangthai == "ChoThanhToan");

        if (napXu == null)
            return Ok();

        napXu.Trangthai = "DaThanhToan";
        napXu.Ngaynap = DateTime.Now;
        napXu.MaGiaoDich = model.TransactionId;

        napXu.MaDocGiaNavigation.Soxu += napXu.Soxunhan;

        await _context.SaveChangesAsync();

        return Ok();
    }
}