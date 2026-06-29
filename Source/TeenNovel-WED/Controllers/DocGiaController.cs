using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TeenNovel_WED.Data;
using TeenNovel_WED.Filters;

namespace TeenNovel_WED.Controllers
{
    public class DocGiaController : Controller
    {
        protected readonly TeenNovelDbContext _context;

        public DocGiaController(TeenNovelDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Lấy MaDocGia từ Claims
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
            if (maDocGiaClaim == null) return;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia)) return;

            // Inject số xu vào ViewData cho navbar
            var docGia = _context.DocGias.FirstOrDefault(d => d.MaDocGia == maDocGia);
            if (docGia != null)
                ViewData["SoXu"] = docGia.Soxu;
        }

        // ─── TRANG CHỦ ────────────────────────────────────
        public async Task<IActionResult> TrangChu()
        {
            ViewData["ActivePage"] = "Home";

            // Inject SoXu nếu đã đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
                if (int.TryParse(maDocGiaClaim, out int maDocGia))
                {
                    var docGia = await _context.DocGias
                        .FirstOrDefaultAsync(d => d.MaDocGia == maDocGia);
                    if (docGia != null)
                        ViewData["SoXu"] = docGia.Soxu;
                }
            }

            // Danh sách thể loại
            var dsTheLoai = await _context.TheoLoais
                .OrderBy(t => t.Tentheloa)
                .ToListAsync();

            // Truyện nổi bật — top 10 lượt đọc cao nhất
            var truyenNoiBat = await _context.Truyens
                .Include(t => t.MatheloaiNavigation)
                .OrderByDescending(t => t.LuotDoc)
                .Take(10)
                .ToListAsync();

            // Xếp hạng tuần — top 8 lượt đọc
            var truyenXepHang = await _context.Truyens
                .Include(t => t.MatheloaiNavigation)
                .OrderByDescending(t => t.LuotDoc)
                .Take(8)
                .ToListAsync();

            // Mới cập nhật — 8 truyện theo ngày đăng gần nhất
            var truyenMoi = await _context.Truyens
                .OrderByDescending(t => t.Ngaydang)
                .Take(8)
                .ToListAsync();

            // Lấy tên chương mới nhất cho mỗi truyện mới
            var matruyenList = truyenMoi.Select(t => t.Matruyen).ToList();
            var chuongMoiDict = await _context.Chuongs
                .Where(c => matruyenList.Contains(c.Matruyen))
                .GroupBy(c => c.Matruyen)
                .Select(g => new
                {
                    Matruyen = g.Key,
                    TenChuong = g.OrderByDescending(c => c.Thutu).First().Tenchuong
                })
                .ToDictionaryAsync(x => x.Matruyen, x => x.TenChuong);

            ViewBag.DsTheLoai = dsTheLoai;
            ViewBag.TruyenNoiBat = truyenNoiBat;
            ViewBag.TruyenXepHang = truyenXepHang;
            ViewBag.TruyenMoi = truyenMoi;
            ViewBag.ChuongMoiDict = chuongMoiDict;

            return View();
        }
    }
}
