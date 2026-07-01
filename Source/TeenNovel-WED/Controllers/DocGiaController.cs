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

        // ─── TRANG XẾP HẠNG ───────────────────────────────
        // /DocGia/XepHang
        public async Task<IActionResult> XepHang()
        {
            ViewData["ActivePage"] = "XepHang";
            ViewData["Title"] = "Xếp hạng";

            // Top nổi bật — lượt đọc cao nhất
            var topNoiBat = await _context.Truyens
                .OrderByDescending(x => x.LuotDoc)
                .Take(20)
                .ToListAsync();

            // Top xếp hạng — lượt thích cao nhất
            var topXepHang = await _context.Truyens
                .OrderByDescending(x => x.LuotThich)
                .Take(20)
                .ToListAsync();

            ViewBag.TopNoiBat = topNoiBat;
            ViewBag.TopXepHang = topXepHang;

            return View();
        }


        public async Task<IActionResult> TheLoai(int id)
        {
            var data = await _context.Truyens
                .Include(t => t.MatheloaiNavigation)
                .Where(t => t.Matheloai == id)
                .OrderByDescending(t => t.LuotDoc)
                .ToListAsync();

            var theLoai = await _context.TheoLoais

                .FirstOrDefaultAsync(x => x.Matheloai == id);
            ViewBag.TenTheLoai = theLoai?.Tentheloa;

            return View(data);

        }

        // ─── TRUYỆN MỚI CẬP NHẬT ─────────────────────
        public async Task<IActionResult> MoiCapNhat()
        {
            ViewData["ActivePage"] = "MoiCapNhat";

            var ngayBatDau = DateTime.Now.AddDays(-30);

            var truyenMoi = await _context.Truyens
                .Include(t => t.MatheloaiNavigation)
                .Where(t => t.Ngaydang >= ngayBatDau)
                .OrderByDescending(t => t.Ngaydang)
                .ToListAsync();

            ViewBag.TruyenMoi = truyenMoi;

            return View(truyenMoi);
        }

        [HttpGet]

        public async Task<IActionResult> TimKiem(string q)
        {


            if (string.IsNullOrEmpty(q))
            {

                TempData["Error"] = "Vui lòng nhập từ khóa tìm kiếm";

                return RedirectToAction("TrangChu");

            }



            var truyen = await _context.Truyens

                .Include(t => t.MatheloaiNavigation)

                .FirstOrDefaultAsync(t =>


                    t.Tentruyen.Contains(q)

                    ||

                    t.Tacgia.Contains(q)


                    ||

                    t.MatheloaiNavigation.Tentheloa.Contains(q)

                );





            if (truyen == null)
            {

                TempData["Error"] =
                $"Không tìm thấy truyện với từ khóa: {q}";


                return RedirectToAction("TrangChu");

            }




            return RedirectToAction(
                "ChiTiet",
                new { id = truyen.Matruyen }
            );


        }

        // ─── CHI TIẾT TRUYỆN ─────────────────────────

        public async Task<IActionResult> ChiTiet(int id)
        {

            var truyen = await _context.Truyens

                .Include(t => t.MatheloaiNavigation)

                .Include(t => t.Chuongs)

                .Include(t => t.DanhGias)

                .FirstOrDefaultAsync(t => t.Matruyen == id);



            if (truyen == null)
            {
                return NotFound();
            }




            // tính điểm đánh giá trung bình

            double diemDanhGia = 0;


            if (truyen.DanhGias != null && truyen.DanhGias.Any())
            {

                diemDanhGia = truyen.DanhGias
                    .Average(x => x.Sosao);

            }



            ViewBag.DiemDanhGia = Math.Round(diemDanhGia, 1);




            // lấy chương mới nhất

            var chuongMoi = truyen.Chuongs

                .OrderByDescending(x => x.Thutu)

                .FirstOrDefault();



            ViewBag.ChuongMoi = chuongMoi;




            return View(truyen);

        }
    }
}
