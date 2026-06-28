using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TeenNovel_WED.Data;

namespace TeenNovel_WED
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<TeenNovelDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ─── MVC ──────────────────────────────────────────────────
            builder.Services.AddControllersWithViews();

            // ─── Authentication + Cookie ──────────────────────────────
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Trang đăng nhập khi chưa xác thực
                    options.LoginPath = "/Auth/Login";
                    // Trang báo lỗi khi không đủ quyền
                    options.AccessDeniedPath = "/Auth/AccessDenied";

                    // ── Thời gian cookie mặc định ──────────────────
                    // Độc giả: 1 giờ (IsPersistent=true trong Login action)
                    // Admin  : session cookie (IsPersistent=false, đóng browser = logout)
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);

                    // Tự gia hạn cookie nếu độc giả còn dùng trong vòng 20 phút cuối
                    options.SlidingExpiration = true;

                    // ── Bảo mật cookie ────────────────────────────
                    options.Cookie.Name = "TeenNovel.Auth";
                    options.Cookie.HttpOnly = true;     // JS không đọc được
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

                    // ── Xử lý hết hạn: redirect về login ──────────
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            // Nếu là API request thì trả 401, không redirect
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.Response.StatusCode = 401;
                                return Task.CompletedTask;
                            }
                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        },

                        OnRedirectToAccessDenied = context =>
                        {
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.Response.StatusCode = 403;
                                return Task.CompletedTask;
                            }
                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });

            // ─── Authorization ────────────────────────────────────────
            builder.Services.AddAuthorization();

            // ─── Session (dùng thêm nếu cần lưu dữ liệu tạm) ─────────
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ─── Build ────────────────────────────────────────────────
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();       // ← wwwroot
            app.UseRouting();
            app.UseSession();           // ← trước Authentication
            app.UseAuthentication();    // ← xác thực
            app.UseAuthorization();     // ← phân quyền

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=DocGia}/{action=TrangChu}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
