using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using TeenNovel_Wed.Data;
using TeenNovel_Wed.Models;

namespace TeenNovel_WED.Controllers
{
    public class Login_RegisterController : Controller
    {
        private readonly TeenNovelDbContext _context;

        private const string SESSION_OTP_EMAIL = "OTP_Email";
        private const string SESSION_OTP_CODE = "OTP_Code";
        private const string SESSION_OTP_EXPIRE = "OTP_Expire";
        private const string SESSION_OTP_VERIFIED = "OTP_Verified";

        public Login_RegisterController(TeenNovelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult LoginGoogle(string? returnUrl = null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleResponse), new { returnUrl })
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction(nameof(Login));
            }

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal?.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không lấy được email từ Google.";
                return RedirectToAction(nameof(Login));
            }

            //-----------------------------------------
            // Kiểm tra tài khoản
            //-----------------------------------------

            var taiKhoan = _context.TaiKhoans
                .FirstOrDefault(x => x.Email == email.ToLower());

            //-----------------------------------------
            // Email đã tồn tại nhưng là Local
            //-----------------------------------------

            if (taiKhoan != null && string.Equals(taiKhoan.LoaiDangNhap?.Trim(), "Local", StringComparison.OrdinalIgnoreCase))
            {
                // Đăng xuất session hiện tại (nếu có) để không bị auto-redirect ở trang Login
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["Error"] = "Email này đã được đăng ký bằng tài khoản TeenNovel. Vui lòng đăng nhập bằng email và mật khẩu.";
                return RedirectToAction(nameof(Login));
            }

            //-----------------------------------------
            // Nếu chưa có thì tạo
            //-----------------------------------------

            if (taiKhoan == null)
            {
                var roleDocGia = _context.VaiTros
                    .First(v => v.TenVaiTro == "docgia");

                taiKhoan = new TaiKhoan
                {
                    Email = email.ToLower(),
                    Tendangnhap = name ?? email,
                    Matkhau = "",
                    Trangthai = true,
                    MaVaiTro = roleDocGia.MaVaiTro,
                    LoaiDangNhap = "Google"
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var docGia = new DocGia
                {
                    Matk = taiKhoan.Matk,
                    Ten = name ?? email,
                    Soxu = 0,
                    Ngaytao = DateTime.Now
                };

                _context.DocGias.Add(docGia);
                await _context.SaveChangesAsync();
            }

            //-----------------------------------------
            // Lấy MaDocGia
            //-----------------------------------------

            var dg = _context.DocGias
                .FirstOrDefault(x => x.Matk == taiKhoan.Matk);

            //-----------------------------------------
            // Cookie của TeenNovel
            //-----------------------------------------

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.Matk.ToString()),
                new Claim(ClaimTypes.Email, taiKhoan.Email),
                new Claim(ClaimTypes.Name, taiKhoan.Tendangnhap),
                new Claim(ClaimTypes.Role,"docgia"),
                new Claim("Matk",taiKhoan.Matk.ToString())
            };

            if (dg != null)
            {
                claims.Add(new Claim("MaDocGia", dg.MaDocGia.ToString()));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("TrangChu", "DocGia");
        }

        // ─── GET: /Auth/Login ─────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển hướng luôn
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

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

            if (taikhoan == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
                return View();
            }

            if (taikhoan.LoaiDangNhap == "Google")
            {
                ViewBag.Error = "Tài khoản này chỉ có thể đăng nhập bằng Google.";
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, taikhoan.Matkhau))
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
                return View();
            }

            // 3. Kiểm tra tài khoản có bị khoá không
            if (taikhoan.Trangthai == false)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khoá. Vui lòng liên hệ CSKH.";
                return View();
            }

            // 4. Lấy vai trò
            var vaitro = _context.VaiTros
                .FirstOrDefault(v => v.MaVaiTro == taikhoan.MaVaiTro);
            var roleName = NormalizeRole(vaitro?.TenVaiTro);

            if (roleName == null)
            {
                ViewBag.Error = "Tài khoản chưa được gán vai trò hợp lệ. Vui lòng liên hệ quản trị viên.";
                return View();
            }

            // 5. Xác định thời gian session theo vai trò
            //    - Admin/Nhân viên: hết hạn khi đóng trình duyệt (session cookie)
            //    - Độc giả: persistent 1 giờ
            bool isAdmin = roleName == "admin" || roleName == "nhanvien";

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
            var tk = _context.TaiKhoans
                .FirstOrDefault(t => t.Email == email.Trim().ToLower());

            if (tk != null)
            {
                if (tk.LoaiDangNhap == "Google")
                {
                    ViewBag.Error = "Email này đã được đăng ký bằng Google. Vui lòng đăng nhập bằng Google.";
                }
                else
                {
                    ViewBag.Error = "Email này đã được đăng ký.";
                }

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
                Trangthai = true,
                LoaiDangNhap = "Local",
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

        // ─── BƯỚC 1: GET /Login_Register/QuenMatKhau ─────────────
        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        // ─── BƯỚC 1: POST — gửi mã code về email ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                ViewBag.Error = "Vui lòng nhập email hợp lệ.";
                return View();
            }

            var taikhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.Email == email.Trim().ToLower());

            if (taikhoan == null)
            {
                ViewBag.Error = "Email này chưa được đăng ký trong hệ thống.";
                return View();
            }

            // Tạo mã OTP 6 số ngẫu nhiên
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu vào Session — hết hạn sau 5 phút
            HttpContext.Session.SetString(SESSION_OTP_EMAIL, taikhoan.Email);
            HttpContext.Session.SetString(SESSION_OTP_CODE, otp);
            HttpContext.Session.SetString(SESSION_OTP_EXPIRE, DateTime.Now.AddMinutes(5).ToString("o"));
            HttpContext.Session.Remove(SESSION_OTP_VERIFIED);

            // Gửi email chứa mã OTP
            try
            {
                await GuiEmailOtp(taikhoan.Email, otp);
            }
            catch (Exception)
            {
                ViewBag.Error = "Không thể gửi email lúc này. Vui lòng thử lại sau.";
                return View();
            }

            TempData["EmailDaGui"] = MaskEmail(taikhoan.Email);
            return RedirectToAction("XacNhanMa");
        }

        // ─── BƯỚC 2: GET /Login_Register/XacNhanMa ───────────────
        // Chặn truy cập trực tiếp — chỉ vào được sau khi đã gửi OTP
        [HttpGet]
        public IActionResult XacNhanMa()
        {
            var email = HttpContext.Session.GetString(SESSION_OTP_EMAIL);
            if (string.IsNullOrEmpty(email))
            {
                // Không có phiên OTP hợp lệ → không cho vào thẳng bằng link
                TempData["Error"] = "Phiên xác thực đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("QuenMatKhau");
            }

            ViewBag.EmailMask = MaskEmail(email);
            return View();
        }

        // ─── BƯỚC 2: POST — xác minh mã code ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XacNhanMa(string macode)
        {
            var email = HttpContext.Session.GetString(SESSION_OTP_EMAIL);
            var otpDung = HttpContext.Session.GetString(SESSION_OTP_CODE);
            var expireStr = HttpContext.Session.GetString(SESSION_OTP_EXPIRE);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpDung))
            {
                TempData["Error"] = "Phiên xác thực đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("QuenMatKhau");
            }

            // Kiểm tra hết hạn
            if (DateTime.TryParse(expireStr, out DateTime expire) && DateTime.Now > expire)
            {
                HttpContext.Session.Remove(SESSION_OTP_EMAIL);
                HttpContext.Session.Remove(SESSION_OTP_CODE);
                TempData["Error"] = "Mã xác thực đã hết hạn. Vui lòng gửi lại.";
                return RedirectToAction("QuenMatKhau");
            }

            if (string.IsNullOrWhiteSpace(macode) || macode.Trim() != otpDung)
            {
                ViewBag.Error = "Mã xác thực không đúng. Vui lòng thử lại.";
                ViewBag.EmailMask = MaskEmail(email);
                return View();
            }

            // Xác thực thành công — đánh dấu để bước 3 kiểm tra
            HttpContext.Session.SetString(SESSION_OTP_VERIFIED, "true");

            return RedirectToAction("DoiMatKhau");
        }

        // ─── BƯỚC 2: GỬI LẠI MÃ ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiLaiMa()
        {
            var email = HttpContext.Session.GetString(SESSION_OTP_EMAIL);
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên xác thực đã hết hạn.";
                return RedirectToAction("QuenMatKhau");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString(SESSION_OTP_CODE, otp);
            HttpContext.Session.SetString(SESSION_OTP_EXPIRE, DateTime.Now.AddMinutes(5).ToString("o"));

            await GuiEmailOtp(email, otp);

            TempData["Success"] = "Đã gửi lại mã xác thực.";
            return RedirectToAction("XacNhanMa");
        }

        // ─── BƯỚC 3: GET /Login_Register/DoiMatKhau ──────────────
        // CHẶN TRUY CẬP TRỰC TIẾP — chỉ vào được sau khi XacNhanMa thành công
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            var daXacThuc = HttpContext.Session.GetString(SESSION_OTP_VERIFIED);
            var email = HttpContext.Session.GetString(SESSION_OTP_EMAIL);

            if (daXacThuc != "true" || string.IsNullOrEmpty(email))
            {
                // Chưa xác thực OTP → không cho vào bằng link trực tiếp
                TempData["Error"] = "Bạn cần xác thực mã trước khi đổi mật khẩu.";
                return RedirectToAction("QuenMatKhau");
            }

            return View();
        }

        // ─── BƯỚC 3: POST — cập nhật mật khẩu mới ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(string matkhaumoi, string xacnhanmatkhau)
        {
            var daXacThuc = HttpContext.Session.GetString(SESSION_OTP_VERIFIED);
            var email = HttpContext.Session.GetString(SESSION_OTP_EMAIL);

            if (daXacThuc != "true" || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên xác thực đã hết hạn. Vui lòng thực hiện lại.";
                return RedirectToAction("QuenMatKhau");
            }

            if (string.IsNullOrWhiteSpace(matkhaumoi) || matkhaumoi.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }

            if (matkhaumoi != xacnhanmatkhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == email);
            if (taikhoan == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("QuenMatKhau");
            }

            taikhoan.Matkhau = BCrypt.Net.BCrypt.HashPassword(matkhaumoi);
            await _context.SaveChangesAsync();

            // Xoá sạch session OTP sau khi đổi thành công
            HttpContext.Session.Remove(SESSION_OTP_EMAIL);
            HttpContext.Session.Remove(SESSION_OTP_CODE);
            HttpContext.Session.Remove(SESSION_OTP_EXPIRE);
            HttpContext.Session.Remove(SESSION_OTP_VERIFIED);

            // Trả về JSON để JS hiện popup rồi tự chuyển hướng
            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        // ─── HELPER: GỬI EMAIL OTP ────────────────────────────────
        private async Task GuiEmailOtp(string toEmail, string otp)
        {
            // Cấu hình SMTP — thay bằng thông tin email thật của bạn trong appsettings.json
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var smtpUser = "danglntb02258@gmail.com";       // ← email gửi đi
            var smtpPass = "bhdt fzeq cyfl pedc";          // ← App Password của Gmail

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, "TeenNovel"),
                Subject = "Mã xác thực đổi mật khẩu - TeenNovel",
                Body = $@"
            <div style='font-family:sans-serif;padding:20px'>
                <h2 style='color:#7c3aed'>TeenNovel</h2>
                <p>Mã xác thực của bạn là:</p>
                <div style='font-size:32px;font-weight:bold;letter-spacing:8px;color:#7c3aed'>{otp}</div>
                <p style='color:#888;font-size:13px'>Mã có hiệu lực trong 5 phút. Không chia sẻ mã này cho bất kỳ ai.</p>
            </div>",
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }

        // ─── HELPER: CHE MỘT PHẦN EMAIL ──────────────────────────
        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;

            var local = parts[0];
            var domain = parts[1];

            string maskedLocal = local.Length <= 2
                ? local[0] + "*"
                : local.Substring(0, 2) + new string('*', Math.Max(local.Length - 2, 3));

            return $"{maskedLocal}@{domain}";
        }


        // ─── HELPER ───────────────────────────────────────
        private IActionResult RedirectByRole(string? role = null)
        {
            role = NormalizeRole(role ?? User.FindFirst(ClaimTypes.Role)?.Value) ?? "docgia";

            return role switch
            {
                "quantrivien" => RedirectToAction("Dashboard", "QuanLy"),
                "nhanvien" => RedirectToAction("Dashboard", "QuanLy"),
                _ => RedirectToAction("TrangChu", "DocGia")
            };
        }

        // Chuẩn hoá giá trị VaiTro lấy từ cơ sở dữ liệu trước khi dùng cho claim/điều hướng.
        // Ví dụ: "Admin", "Quản trị viên" và "quantrivien" đều là quản trị viên.
        private static string? NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return null;

            var decomposed = role.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var key = new string(decomposed
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .Where(char.IsLetterOrDigit)
                .ToArray())
                .Replace('đ', 'd');

            return key switch
            {
                "admin" or "administrator" or "quantrivien" => "quantrivien",
                "staff" or "employee" or "nhanvien" => "nhanvien",
                "reader" or "customer" or "docgia" => "docgia",
                _ => null
            };
        }
    }
}
