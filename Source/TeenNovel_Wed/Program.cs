using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using TeenNovel_Wed.Data;

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
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // Trang đăng nhập khi chưa xác thực
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/AccessDenied";

                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;

                options.Cookie.Name = "TeenNovel.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
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
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                // lấy email, avatar...
                options.Scope.Add("email");
                options.Scope.Add("profile");

                options.SaveTokens = true;
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

            app.MapControllers();

            app.Run();
        }
    }
}
