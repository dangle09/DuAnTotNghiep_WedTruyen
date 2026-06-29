using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeenNovel_WED.Data;
using TeenNovel_WED.Models;

namespace TeenNovel_WED.Controllers
{
    public class QuanLyController : Controller
    {
        private readonly TeenNovelDbContext _context;
        private const int PAGE_SIZE = 10;

        public QuanLyController(TeenNovelDbContext context)
        {
            _context = context;
        }

        // ─── INJECT SIDEBAR BADGE ─────────────────────────
        private void SetSidebarData()
        {
            ViewData["SoBinhLuanCho"] = _context.BinhLuans.Count(b => b.TrangThai == 0);
            ViewData["SoHoTroCho"] = _context.HoTros.Count(h => h.TrangThai == 0);
            ViewData["TongThongBao"] = (int)ViewData["SoBinhLuanCho"]! + (int)ViewData["SoHoTroCho"]!;
        }

        //_________Dashboard_________
        public IActionResult Dashboard()
        {
            return View();
        }

        // =====================================================
        //  DANH SÁCH TRUYỆN
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> QuanLyTruyen(string? search, int? theloai, string? trangthai, int page = 1)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "Truyen";
            ViewData["Title"] = "Quản lý truyện";
            ViewData["Breadcrumb"] = "Quản lý truyện";

            var query = _context.Truyens
                .Include(t => t.MatheloaiNavigation)
                .Include(t => t.ManvNavigation)
                .AsQueryable();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Tentruyen.Contains(search) ||
                                         (t.Tacgia != null && t.Tacgia.Contains(search)));

            // Lọc theo thể loại
            if (theloai.HasValue)
                query = query.Where(t => t.MatheloaiNavigation.Matheloai == theloai.Value);

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangthai))
                query = query.Where(t => t.Trangthai == trangthai);

            // Tổng số bản ghi
            int total = await query.CountAsync();

            // Phân trang
            var truyens = await query
                .OrderByDescending(t => t.Ngaydang)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            // Thể loại cho dropdown lọc
            ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa", theloai);
            ViewBag.Search = search;
            ViewBag.TheLoai = theloai;
            ViewBag.TrangThai = trangthai;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            ViewBag.Total = total;

            return View(truyens);
        }

        // =====================================================
        //  CHI TIẾT TRUYỆN
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> ChiTietTruyen(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "Truyen";
            ViewData["Title"] = "Chi tiết truyện";
            ViewData["Breadcrumb"] = "Chi tiết truyện";

            var truyen = await _context.Truyens
                .Include(t => t.MatheloaiNavigation)
                .Include(t => t.ManvNavigation)
                .FirstOrDefaultAsync(t => t.Matruyen == id);

            if (truyen == null)
            {
                TempData["Error"] = "Không tìm thấy truyện.";
                return RedirectToAction("QuanLyTruyen");
            }

            // Danh sách chương
            var chuongs = await _context.Chuongs
                .Where(c => c.Matruyen == id)
                .OrderBy(c => c.Thutu)
                .ToListAsync();

            // Đánh giá
            var danhgias = await _context.DanhGias
                .Include(d => d.MaDocGiaNavigation)
                .Where(d => d.Matruyen == id)
                .OrderByDescending(d => d.Ngaydanhgia)
                .Take(10)
                .ToListAsync();

            double diemTb = danhgias.Any()
                ? Math.Round(danhgias.Average(d => (double)d.Sosao), 1)
                : 0;

            ViewBag.Chuongs = chuongs;
            ViewBag.DanhGias = danhgias;
            ViewBag.DiemTb = diemTb;
            ViewBag.SoChuong = chuongs.Count;

            return View(truyen);
        }

        // =====================================================
        //  THÊM TRUYỆN
        // =====================================================
        [HttpGet]
        public IActionResult ThemTruyen()
        {
            SetSidebarData();
            ViewData["ActivePage"] = "Truyen";
            ViewData["Title"] = "Thêm truyện";
            ViewData["Breadcrumb"] = "Thêm truyện";

            ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemTruyen(
            string tentruyen, int matheloai, string? mota,
            string? tacgia, string? theloai, string trangthai,
            IFormFile? anhbia)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(tentruyen))
            {
                TempData["Error"] = "Tên truyện không được để trống.";
                ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa", matheloai);
                SetSidebarData();
                return View();
            }

            // Lấy Manv từ Claims
            var manvClaim = User.FindFirst("Manv")?.Value;
            if (!int.TryParse(manvClaim, out int manv))
            {
                TempData["Error"] = "Không xác định được nhân viên đăng truyện.";
                return RedirectToAction("QuanLyTruyen");
            }

            // Xử lý upload ảnh bìa
            string? anhBiaPath = null;
            if (anhbia != null && anhbia.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(anhbia.ContentType))
                {
                    TempData["Error"] = "Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.";
                    ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa", matheloai);
                    SetSidebarData();
                    return View();
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(anhbia.FileName)}";
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bia");
                Directory.CreateDirectory(uploadDir);
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await anhbia.CopyToAsync(stream);
                anhBiaPath = $"/uploads/bia/{fileName}";
            }

            var truyen = new Truyen
            {
                Manv = manv,
                Matheloai = matheloai,
                Tentruyen = tentruyen.Trim(),
                Mota = mota?.Trim(),
                Tacgia = tacgia?.Trim(),
                Theloai = theloai?.Trim(),
                AnhBia = anhBiaPath,
                Trangthai = trangthai,
                LuotDoc = 0,
                LuotThich = 0,
                Ngaydang = DateTime.Now
            };

            _context.Truyens.Add(truyen);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm truyện \"{tentruyen}\" thành công!";
            return RedirectToAction("QuanLyTruyen");
        }

        // =====================================================
        //  SỬA TRUYỆN
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> SuaTruyen(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "Truyen";
            ViewData["Title"] = "Sửa truyện";
            ViewData["Breadcrumb"] = "Sửa truyện";

            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen == null)
            {
                TempData["Error"] = "Không tìm thấy truyện.";
                return RedirectToAction("QuanLyTruyen");
            }

            ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa", truyen.Matheloai);
            return View(truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaTruyen(
            int id, string tentruyen, int matheloai,
            string? mota, string? tacgia, string? theloai,
            string trangthai, IFormFile? anhbia)
        {
            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen == null)
            {
                TempData["Error"] = "Không tìm thấy truyện.";
                return RedirectToAction("QuanLyTruyen");
            }

            if (string.IsNullOrWhiteSpace(tentruyen))
            {
                TempData["Error"] = "Tên truyện không được để trống.";
                ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa", matheloai);
                SetSidebarData();
                return View(truyen);
            }

            // Xử lý ảnh bìa mới (nếu có)
            if (anhbia != null && anhbia.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(anhbia.ContentType))
                {
                    TempData["Error"] = "Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.";
                    ViewBag.DsTheLoai = new SelectList(_context.TheoLoais, "Matheloai", "Tentheloa", matheloai);
                    SetSidebarData();
                    return View(truyen);
                }

                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(truyen.AnhBia))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                               truyen.AnhBia.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(anhbia.FileName)}";
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bia");
                Directory.CreateDirectory(uploadDir);
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await anhbia.CopyToAsync(stream);
                truyen.AnhBia = $"/uploads/bia/{fileName}";
            }

            truyen.Tentruyen = tentruyen.Trim();
            truyen.Matheloai = matheloai;
            truyen.Mota = mota?.Trim();
            truyen.Tacgia = tacgia?.Trim();
            truyen.Theloai = theloai?.Trim();
            truyen.Trangthai = trangthai;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật truyện \"{tentruyen}\" thành công!";
            return RedirectToAction("ChiTietTruyen", new { id });
        }

        // =====================================================
        //  XOÁ TRUYỆN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTruyen(int id)
        {
            var truyen = await _context.Truyens
                .Include(t => t.Chuongs)
                .FirstOrDefaultAsync(t => t.Matruyen == id);

            if (truyen == null)
            {
                TempData["Error"] = "Không tìm thấy truyện.";
                return RedirectToAction("QuanLyTruyen");
            }

            string tenTruyen = truyen.Tentruyen;

            // Xoá ảnh bìa nếu có
            if (!string.IsNullOrEmpty(truyen.AnhBia))
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                           truyen.AnhBia.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }

            _context.Truyens.Remove(truyen);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xoá truyện \"{tenTruyen}\".";
            return RedirectToAction("QuanLyTruyen");
        }
    }
}
