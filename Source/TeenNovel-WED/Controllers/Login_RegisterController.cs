using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TeenNovel_WED.Models;
using System.Security.Claims;
using TeenNovel_WED.Data;
using BCrypt.Net;

namespace TeenNovel_WED.Controllers
{
    public class Login_RegisterController : Controller
    {
        private readonly TeenNovelDbContext _context;

        public Login_RegisterController(TeenNovelDbContext context)
        {
            _context = context;
        }

        // ─── GET: /Auth/Login ─────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển hướng luôn
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ─── POST: /Auth/Login ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            ViewBag.Email = email;

            // 1. Kiểm tra input rỗng
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return View();
            }

            // 2. Tìm tài khoản theo email
            var taikhoan = _context.TaiKhoans
                .FirstOrDefault(t => t.Email == email.Trim().ToLower());

            if (taikhoan == null || taikhoan.Matkhau != password)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
                return View();
            }

            // 3. Kiểm tra tài khoản có bị khoá không
            if (taikhoan.Trangthai == "khoatai")
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khoá. Vui lòng liên hệ CSKH.";
                return View();
            }

            // 4. Lấy vai trò
            var vaitro = _context.VaiTros
                .FirstOrDefault(v => v.MaVaiTro == taikhoan.MaVaiTro);
            var roleName = vaitro?.TenVaiTro ?? "docgia";

            // 5. Xác định thời gian session theo vai trò
            //    - Admin/Nhân viên: hết hạn khi đóng trình duyệt (session cookie)
            //    - Độc giả: persistent 1 giờ
            bool isAdmin = roleName == "quantrivien" || roleName == "nhanvien";

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = !isAdmin,                        // admin: session cookie
                ExpiresUtc = isAdmin
                                    ? (DateTimeOffset?)null        // hết hạn khi đóng browser
                                    : DateTimeOffset.UtcNow.AddHours(1), // docgia: 1 giờ
                AllowRefresh = !isAdmin                         // docgia: tự gia hạn khi dùng
            };

            // 6. Tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taikhoan.Matk.ToString()),
                new Claim(ClaimTypes.Email,          taikhoan.Email),
                new Claim(ClaimTypes.Name,           taikhoan.Tendangnhap),
                new Claim(ClaimTypes.Role,           roleName),
                new Claim("Matk",                   taikhoan.Matk.ToString())
            };

            // Thêm claim MaDocGia hoặc Manv tuỳ vai trò
            if (roleName == "docgia")
            {
                var docgia = _context.DocGias
                    .FirstOrDefault(d => d.Matk == taikhoan.Matk);
                if (docgia != null)
                    claims.Add(new Claim("MaDocGia", docgia.MaDocGia.ToString()));
            }
            else
            {
                var nhanvien = _context.NhanViens
                    .FirstOrDefault(n => n.Matk == taikhoan.Matk);
                if (nhanvien != null)
                    claims.Add(new Claim("Manv", nhanvien.Manv.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties
            );

            // 7. Chuyển hướng theo vai trò
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectByRole(roleName);
        }

        // ─── GET: /Auth/Register ──────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            return View();
        }

        // ─── POST: /Auth/Register ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string tendangnhap, string email, string matkhau)
        {
            ViewBag.Tendangnhap = tendangnhap;
            ViewBag.Email = email;

            // 1. Validate cơ bản
            if (string.IsNullOrWhiteSpace(tendangnhap) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(matkhau))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            if (!email.Contains('@'))
            {
                ViewBag.Error = "Email không hợp lệ, phải chứa ký tự @.";
                return View();
            }

            if (matkhau.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View();
            }

            // 2. Kiểm tra email / tên đăng nhập đã tồn tại chưa
            if (_context.TaiKhoans.Any(t => t.Email == email.Trim().ToLower()))
            {
                ViewBag.Error = "Email này đã được đăng ký.";
                return View();
            }

            if (_context.TaiKhoans.Any(t => t.Tendangnhap == tendangnhap.Trim()))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            // 3. Lấy MaVaiTro của "docgia"
            var roleDocGia = _context.VaiTros.FirstOrDefault(v => v.TenVaiTro == "docgia");
            if (roleDocGia == null)
            {
                ViewBag.Error = "Lỗi hệ thống, vui lòng thử lại sau.";
                return View();
            }

            // 4. Tạo TaiKhoan
            var taikhoan = new TaiKhoan
            {
                MaVaiTro = roleDocGia.MaVaiTro,
                Email = email.Trim().ToLower(),
                Matkhau = BCrypt.Net.BCrypt.HashPassword(matkhau),
                Tendangnhap = tendangnhap.Trim(),
                Trangthai = "hoatdong"
            };
            _context.TaiKhoans.Add(taikhoan);
            await _context.SaveChangesAsync();

            // 5. Tạo DocGia tương ứng
            var docgia = new DocGia
            {
                Matk = taikhoan.Matk,
                Ten = tendangnhap.Trim(),
                Soxu = 0,
                Ngaytao = DateTime.Now
            };
            _context.DocGias.Add(docgia);
            await _context.SaveChangesAsync();

            // 6. Tự động đăng nhập sau khi đăng ký
            TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với TeenNovel.";
            return RedirectToAction("Login");
        }

        // ─── GET/POST: /Auth/Logout ───────────────────────
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ─── HELPER ───────────────────────────────────────
        private IActionResult RedirectByRole(string? role = null)
        {
            role ??= User.FindFirst(ClaimTypes.Role)?.Value ?? "docgia";

            return role switch
            {
                "quantrivien" => RedirectToAction("Dashboard", "QuanLy"),
                "nhanvien" => RedirectToAction("Dashboard", "QuanLy"),
                _ => RedirectToAction("TrangChu", "DocGia")
            };
        }
    }
}
