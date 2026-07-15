using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeenNovel_Wed.Data;
using TeenNovel_Wed.Models;

namespace TeenNovel_Wed.Controllers
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
                .Include(t => t.MaTheLoais)
                .Include(t => t.ManvNavigation)
                .AsQueryable();

            // Danh sách tác giả
            var dsTacGia = await _context.TacGias
                .OrderBy(t => t.TenTacGia)
                .ToListAsync();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Tentruyen.Contains(search) ||
                                         (t.MaTacGiaNavigation.TenTacGia != null && t.MaTacGiaNavigation.TenTacGia.Contains(search)));

            // Lọc theo thể loại
            if (theloai.HasValue)
                query = query.Where(t => t.MaTheLoais.Any(x => x.Matheloai == theloai.Value));
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
            ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai", theloai);
            ViewBag.Search = search;
            ViewBag.TheLoai = theloai;
            ViewBag.TrangThai = trangthai;
            ViewBag.DsTacGia = dsTacGia;
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
                .Include(t => t.MaTheLoais)
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

            ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemTruyen(
            string tentruyen, List<int> matheloai, string? mota,
            int? matacgia, string? theloai, string trangthai,
            IFormFile? anhbia)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(tentruyen))
            {
                TempData["Error"] = "Tên truyện không được để trống.";
                ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai", matheloai);
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
                    ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai", matheloai);
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

            if (matacgia == null)
            {
                TempData["Error"] = "Vui lòng chọn tác giả.";
                return View();
            }

            var truyen = new Truyen
            {
                Manv = manv,
                MaTacGia = matacgia.Value,
                Tentruyen = tentruyen.Trim(),
                Mota = mota?.Trim(),
                AnhBia = anhBiaPath,
                Trangthai = trangthai,
                LuotDoc = 0,
                LuotThich = 0,
                Ngaydang = DateTime.Now
            };

            var dsTheLoai = await _context.TheLoais
                .Where(x => matheloai.Contains(x.Matheloai))
                .ToListAsync();

            foreach (var tl in dsTheLoai)
            {
                truyen.MaTheLoais.Add(tl);
            }

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

            ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai", truyen.MaTheLoais);
            return View(truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaTruyen(
            int id, string tentruyen, List<int> matheloai,
            string? mota, int? matacgia,
            string trangthai, IFormFile? anhbia)
        {
            var truyen = await _context.Truyens
                .Include(x => x.MaTheLoais)
                .FirstOrDefaultAsync(x => x.Matruyen == id);
            if (truyen == null)
            {
                TempData["Error"] = "Không tìm thấy truyện.";
                return RedirectToAction("QuanLyTruyen");
            }

            if (string.IsNullOrWhiteSpace(tentruyen))
            {
                TempData["Error"] = "Tên truyện không được để trống.";
                ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai", matheloai);
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
                    ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai", matheloai);
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
            var theLoai = await _context.TheLoais.FindAsync(matheloai);

            truyen.MaTheLoais.Clear();

            var dsTheLoai = await _context.TheLoais
                .Where(x => matheloai.Contains(x.Matheloai))
                .ToListAsync();

            foreach (var tl in dsTheLoai)
            {
                truyen.MaTheLoais.Add(tl);
            }

            if (matacgia == null)
            {
                TempData["Error"] = "Vui lòng chọn tác giả.";
                return View(truyen);
            }

            truyen.Mota = mota?.Trim();
            truyen.MaTacGia = matacgia.Value;
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

        // =====================================================
        //  DANH SÁCH GÓI NẠP
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> QuanLyGoiNap(string? search, bool? trangthai, int page = 1)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "GoiNap";
            ViewData["Title"] = "Quản lý gói nạp";
            ViewData["Breadcrumb"] = "Quản lý gói nạp";

            var query = _context.GoiNapXus
                .AsQueryable();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.TenGoi.Contains(search));
            }

            // Lọc theo trạng thái
            if (trangthai.HasValue)
                query = query.Where(t => t.HienThi == trangthai.Value);

            // Tổng số bản ghi
            int total = await query.CountAsync();

            // Phân trang
            var goinaps = await query
                .OrderByDescending(t => t.SoTien)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            // Thể loại cho dropdown lọc
            ViewBag.Search = search;
            ViewBag.TrangThai = trangthai;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            ViewBag.Total = total;

            return View(goinaps);
        }

        // =====================================================
        //  THÊM GÓI NẠP
        // =====================================================
        [HttpGet]
        public IActionResult ThemGoiNap()
        {
            SetSidebarData();
            ViewData["ActivePage"] = "GoiNap";
            ViewData["Title"] = "Thêm gói nạp";
            ViewData["Breadcrumb"] = "Thêm gói nạp";

            ViewBag.DsTheLoai = new SelectList(_context.TheLoais, "Matheloai", "Tentheloai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemGoiNap(
            string tengoinap, int soxu, decimal giatien, string? khuyenmai, bool? trangthai,
            IFormFile? anhbia)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(tengoinap))
            {
                TempData["Error"] = "Tên gói nạp không được để trống.";
                SetSidebarData();
                return View();
            }

            // Xử lý upload ảnh bìa
            string? anhBiaPath = null;
            if (anhbia != null && anhbia.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(anhbia.ContentType))
                {
                    TempData["Error"] = "Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.";
                    SetSidebarData();
                    return View();
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(anhbia.FileName)}";
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "goinap");
                Directory.CreateDirectory(uploadDir);
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await anhbia.CopyToAsync(stream);
                anhBiaPath = $"/uploads/goinap/{fileName}";
            }

            var goiNap = new GoiNapXu
            {
                TenGoi = tengoinap.Trim(),
                SoXuNhan = soxu,
                SoTien = giatien,
                KhuyenMai = khuyenmai?.Trim(),
                Anh = anhBiaPath,
                HienThi = trangthai ?? true
            };

            _context.GoiNapXus.Add(goiNap);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm gói nạp \"{tengoinap}\" thành công!";
            return RedirectToAction("QuanLyGoiNap");
        }

        // =====================================================
        //  SỬA GÓI NẠP
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> SuaGoiNap(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "GoiNap";
            ViewData["Title"] = "Sửa gói nạp";
            ViewData["Breadcrumb"] = "Sửa gói nạp";

            var goiNap = await _context.GoiNapXus.FindAsync(id);
            if (goiNap == null)
            {
                TempData["Error"] = "Không tìm thấy gói nạp.";
                return RedirectToAction("QuanLyGoiNap");
            }

            return View(goiNap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaGoiNap(
            string tengoinap, int soxu, decimal giatien, string? khuyenmai,
            IFormFile? anhbia, int id)
        {
            var goiNap = await _context.GoiNapXus.FindAsync(id);
            if (goiNap == null)
            {
                TempData["Error"] = "Không tìm thấy gói nạp.";
                return RedirectToAction("QuanLyGoiNap");
            }

            if (string.IsNullOrWhiteSpace(tengoinap))
            {
                TempData["Error"] = "Tên gói nạp không được để trống.";
                SetSidebarData();
                return View(goiNap);
            }

            // Xử lý ảnh bìa mới (nếu có)
            if (anhbia != null && anhbia.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(anhbia.ContentType))
                {
                    TempData["Error"] = "Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.";
                    SetSidebarData();
                    return View(goiNap);
                }

                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(goiNap.Anh))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                               goiNap.Anh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(anhbia.FileName)}";
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "goinap");
                Directory.CreateDirectory(uploadDir);
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await anhbia.CopyToAsync(stream);
                goiNap.Anh = $"/uploads/goinap/{fileName}";
            }

            goiNap.TenGoi = tengoinap.Trim();
            goiNap.SoXuNhan = soxu;
            goiNap.SoTien = giatien;
            goiNap.KhuyenMai = khuyenmai?.Trim();
            goiNap.HienThi = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật gói nạp \"{tengoinap}\" thành công!";
            return RedirectToAction("QuanLyGoiNap", new { id });
        }

        // =====================================================
        //  XOÁ GÓI NẠP
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaGoiNap(int id)
        {
            var goiNap = await _context.GoiNapXus.FindAsync(id);

            if (goiNap == null)
            {
                TempData["Error"] = "Không tìm thấy gói nạp.";
                return RedirectToAction("QuanLyGoiNap");
            }

            string tenGoiNap = goiNap.TenGoi;

            // Xoá ảnh bìa nếu có
            if (!string.IsNullOrEmpty(goiNap.Anh))
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                           goiNap.Anh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }

            _context.GoiNapXus.Remove(goiNap);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xoá gói nạp \"{tenGoiNap}\".";
            return RedirectToAction("QuanLyGoiNap");
        }

        // ================================================================
        //  QUẢN LÝ NHÂN VIÊN
        // ================================================================

        // ─── DANH SÁCH ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> QuanLyNhanVien(string? search, bool? trangthai, int page = 1)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "NhanVien";
            ViewData["Title"] = "Quản lý nhân viên";
            ViewData["Breadcrumb"] = "Quản lý nhân viên";

            var query = _context.NhanViens
                .Include(n => n.MatkNavigation)               // navigation đến TaiKhoan
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(n => n.Ten.Contains(search) ||
                                          (n.MatkNavigation.Email != null && n.MatkNavigation.Email.Contains(search)));

            if(trangthai.HasValue)
                query = query.Where(d => d.MatkNavigation.Trangthai == trangthai.Value);

            int total = await query.CountAsync();

            var nhanviens = await query
                .OrderByDescending(n => n.Ngayvaolamm)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TrangThai = trangthai;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            ViewBag.Total = total;

            return View(nhanviens);
        }

        // ─── CHI TIẾT ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ChiTietNhanVien(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "NhanVien";
            ViewData["Title"] = "Chi tiết nhân viên";
            ViewData["Breadcrumb"] = "Chi tiết nhân viên";

            var nv = await _context.NhanViens
                .Include(n => n.MatkNavigation)
                .FirstOrDefaultAsync(n => n.Manv == id);

            if (nv == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("QuanLyNhanVien");
            }

            // Số truyện nhân viên này đã đăng
            ViewBag.SoTruyenDaDang = await _context.Truyens.CountAsync(t => t.Manv == id);

            // Số yêu cầu CSKH đã xử lý
            ViewBag.SoHoTroDaXuLy = await _context.HoTros.CountAsync(h => h.Manv == id);

            return View(nv);
        }

        // ─── THÊM ─────────────────────────────────────────────────
        [HttpGet]
        public IActionResult ThemNhanVien()
        {
            SetSidebarData();
            ViewData["ActivePage"] = "NhanVien";
            ViewData["Title"] = "Thêm nhân viên";
            ViewData["Breadcrumb"] = "Thêm nhân viên";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNhanVien(
            string ten, string email, string matkhau, string? sodienthoai,
            string? gioitinh, DateTime? ngaysinh)
        {
            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(matkhau))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ họ tên, email và mật khẩu.";
                SetSidebarData();
                return View();
            }

            if (!email.Contains('@'))
            {
                TempData["Error"] = "Email không hợp lệ.";
                SetSidebarData();
                return View();
            }

            if (matkhau.Length < 6)
            {
                TempData["Error"] = "Mật khẩu phải có ít nhất 6 ký tự.";
                SetSidebarData();
                return View();
            }

            if (await _context.TaiKhoans.AnyAsync(t => t.Email == email.Trim().ToLower()))
            {
                TempData["Error"] = "Email này đã được sử dụng.";
                SetSidebarData();
                return View();
            }

            // Lấy MaVaiTro "nhanvien"
            var roleNv = await _context.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "nhanvien");
            if (roleNv == null)
            {
                TempData["Error"] = "Lỗi hệ thống: không tìm thấy vai trò nhân viên.";
                return RedirectToAction("QuanLyNhanVien");
            }

            // Tạo tài khoản đăng nhập
            var tendangnhap = email.Split('@')[0] + new Random().Next(100, 999);
            var taikhoan = new TeenNovel_Wed.Models.TaiKhoan
            {
                MaVaiTro = roleNv.MaVaiTro,
                Email = email.Trim().ToLower(),
                Matkhau = BCrypt.Net.BCrypt.HashPassword(matkhau),
                Tendangnhap = tendangnhap,
                Trangthai = true
            };
            _context.TaiKhoans.Add(taikhoan);
            await _context.SaveChangesAsync();

            // Tạo nhân viên
            var nhanvien = new TeenNovel_Wed.Models.NhanVien
            {
                Matk = taikhoan.Matk,
                Ten = ten.Trim(),
                Sodienthoai = sodienthoai?.Trim(),
                Gioitinh = gioitinh,
                Ngaysinh = ngaysinh,
                Ngayvaolamm = DateTime.Now
            };
            _context.NhanViens.Add(nhanvien);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm nhân viên \"{ten}\" thành công! Tài khoản: {tendangnhap}";
            return RedirectToAction("QuanLyNhanVien");
        }

        // ─── SỬA ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SuaNhanVien(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "NhanVien";
            ViewData["Title"] = "Sửa nhân viên";
            ViewData["Breadcrumb"] = "Sửa nhân viên";

            var nv = await _context.NhanViens.Include(n => n.MatkNavigation)
                .FirstOrDefaultAsync(n => n.Manv == id);

            if (nv == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("QuanLyNhanVien");
            }

            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaNhanVien(
            int id, string ten, string? sodienthoai, string? gioitinh,
            DateTime? ngaysinh, bool trangthai)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("QuanLyNhanVien");
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                TempData["Error"] = "Họ tên không được để trống.";
                return RedirectToAction("SuaNhanVien", new { id });
            }

            nv.Ten = ten.Trim();
            nv.Sodienthoai = sodienthoai?.Trim();
            nv.Gioitinh = gioitinh;
            nv.Ngaysinh = ngaysinh;
            nv.MatkNavigation.Trangthai = trangthai;

            // Đồng bộ trạng thái tài khoản đăng nhập
            var tk = await _context.TaiKhoans.FindAsync(nv.Matk);
            if (tk != null)
                tk.Trangthai = trangthai == true ? true : false;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật thông tin nhân viên \"{ten}\".";
            return RedirectToAction("ChiTietNhanVien", new { id });
        }

        // ─── XOÁ ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaNhanVien(int id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("QuanLyNhanVien");
            }

            // Không cho xoá nếu đã đăng truyện (tránh mất dữ liệu ràng buộc)
            bool coTruyen = await _context.Truyens.AnyAsync(t => t.Manv == id);
            if (coTruyen)
            {
                TempData["Error"] = "Không thể xoá vì nhân viên này đã đăng truyện. Hãy khoá tài khoản thay vì xoá.";
                return RedirectToAction("QuanLyNhanVien");
            }

            string ten = nv.Ten;
            int matk = nv.Matk;

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();

            // Xoá luôn tài khoản đăng nhập
            var tk = await _context.TaiKhoans.FindAsync(matk);
            if (tk != null)
            {
                _context.TaiKhoans.Remove(tk);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Đã xoá nhân viên \"{ten}\".";
            return RedirectToAction("QuanLyNhanVien");
        }

        // ─── KHOÁ / MỞ KHOÁ ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleKhoaNhanVien(int id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("QuanLyNhanVien");
            }

            bool dangKhoa = nv.MatkNavigation.Trangthai == false;
            nv.MatkNavigation.Trangthai = dangKhoa ? true : false;

            var tk = await _context.TaiKhoans.FindAsync(nv.Matk);
            if (tk != null) tk.Trangthai = nv.MatkNavigation.Trangthai;

            await _context.SaveChangesAsync();

            TempData["Success"] = dangKhoa
                ? $"Đã mở khoá tài khoản \"{nv.Ten}\"."
                : $"Đã khoá tài khoản \"{nv.Ten}\".";

            return RedirectToAction("QuanLyNhanVien");
        }


        // ================================================================
        //  QUẢN LÝ KHÁCH HÀNG (ĐỘC GIẢ)
        // ================================================================

        // ─── DANH SÁCH ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> QuanLyKhachHang(string? search, bool? trangthai, int page = 1)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "KhachHang";
            ViewData["Title"] = "Quản lý khách hàng";
            ViewData["Breadcrumb"] = "Quản lý khách hàng";

            var query = _context.DocGias
                .Include(d => d.MatkNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d => d.Ten.Contains(search) ||
                                          (d.MatkNavigation.Email != null && d.MatkNavigation.Email.Contains(search)));

            if (trangthai.HasValue)
                query = query.Where(d => d.MatkNavigation.Trangthai == trangthai.Value);

            int total = await query.CountAsync();

            var docgias = await query
                .OrderByDescending(d => d.Ngaytao)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TrangThai = trangthai;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            ViewBag.Total = total;

            return View(docgias);
        }

        // ─── CHI TIẾT ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ChiTietKhachHang(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "KhachHang";
            ViewData["Title"] = "Chi tiết khách hàng";
            ViewData["Breadcrumb"] = "Chi tiết khách hàng";

            var dg = await _context.DocGias
                .Include(d => d.MatkNavigation)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (dg == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("QuanLyKhachHang");
            }

            ViewBag.SoTruyenTheoDoi = await _context.TheoDois.CountAsync(t => t.MaDocGia == id);
            ViewBag.SoBinhLuan = await _context.BinhLuans.CountAsync(b => b.MaDocGia == id);
            ViewBag.SoDanhGia = await _context.DanhGias.CountAsync(d => d.MaDocGia == id);
            ViewBag.TongXuNap = await _context.NapXus
                .Where(n => n.MaDocGia == id && n.Trangthai == "thanhcong")
                .SumAsync(n => (int?)n.Soxunhan) ?? 0;

            // Lịch sử nạp gần nhất
            ViewBag.LichSuNap = await _context.NapXus
                .Where(n => n.MaDocGia == id)
                .OrderByDescending(n => n.Ngaynap)
                .Take(5)
                .ToListAsync();

            return View(dg);
        }

        // ─── SỬA ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SuaKhachHang(int id)
        {
            SetSidebarData();
            ViewData["ActivePage"] = "KhachHang";
            ViewData["Title"] = "Sửa khách hàng";
            ViewData["Breadcrumb"] = "Sửa khách hàng";

            var dg = await _context.DocGias.Include(d => d.MatkNavigation)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (dg == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("QuanLyKhachHang");
            }

            return View(dg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaKhachHang(int id, string ten, string? sodienthoai, int soxu)
        {
            var dg = await _context.DocGias.FindAsync(id);
            if (dg == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("QuanLyKhachHang");
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                TempData["Error"] = "Họ tên không được để trống.";
                return RedirectToAction("SuaKhachHang", new { id });
            }

            if (soxu < 0)
            {
                TempData["Error"] = "Số xu không được âm.";
                return RedirectToAction("SuaKhachHang", new { id });
            }

            dg.Ten = ten.Trim();
            dg.Sodienthoai = sodienthoai?.Trim();
            dg.Soxu = soxu;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật thông tin khách hàng \"{ten}\".";
            return RedirectToAction("ChiTietKhachHang", new { id });
        }

        // ─── XOÁ ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKhachHang(int id)
        {
            var dg = await _context.DocGias.FindAsync(id);
            if (dg == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("QuanLyKhachHang");
            }

            string ten = dg.Ten;
            int matk = dg.Matk;

            // Xoá dữ liệu liên quan trước (theo dõi, bình luận, đánh giá, bookmark, lịch sử đọc)
            _context.TheoDois.RemoveRange(_context.TheoDois.Where(t => t.MaDocGia == id));
            _context.BinhLuans.RemoveRange(_context.BinhLuans.Where(b => b.MaDocGia == id));
            _context.DanhGias.RemoveRange(_context.DanhGias.Where(d => d.MaDocGia == id));

            _context.DocGias.Remove(dg);
            await _context.SaveChangesAsync();

            var tk = await _context.TaiKhoans.FindAsync(matk);
            if (tk != null)
            {
                _context.TaiKhoans.Remove(tk);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Đã xoá khách hàng \"{ten}\".";
            return RedirectToAction("QuanLyKhachHang");
        }

        // ─── KHOÁ / MỞ KHOÁ ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleKhoaKhachHang(int id)
        {
            var dg = await _context.DocGias.Include(d => d.MatkNavigation)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (dg == null || dg.MatkNavigation == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng.";
                return RedirectToAction("QuanLyKhachHang");
            }

            bool dangKhoa = dg.MatkNavigation.Trangthai == true;
            dg.MatkNavigation.Trangthai = dangKhoa ? false : true;

            await _context.SaveChangesAsync();

            TempData["Success"] = dangKhoa
                ? $"Đã mở khoá tài khoản \"{dg.Ten}\"."
                : $"Đã khoá tài khoản \"{dg.Ten}\".";

            return RedirectToAction("QuanLyKhachHang");
        }

    }
}
