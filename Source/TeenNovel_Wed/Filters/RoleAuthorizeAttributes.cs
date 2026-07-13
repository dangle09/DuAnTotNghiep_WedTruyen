using Microsoft.AspNetCore.Authorization;

namespace TeenNovel_Wed.Filters
{
    // ─── Chỉ độc giả ──────────────────────────────────────
    public class DocGiaAuthorizeAttribute : AuthorizeAttribute
    {
        public DocGiaAuthorizeAttribute()
        {
            Roles = "docgia";
        }
    }

    // ─── Chỉ nhân viên hoặc quản trị viên ─────────────────
    public class NhanVienAuthorizeAttribute : AuthorizeAttribute
    {
        public NhanVienAuthorizeAttribute()
        {
            Roles = "nhanvien,quantrivien";
        }
    }

    // ─── Chỉ quản trị viên ────────────────────────────────
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            Roles = "quantrivien";
        }
    }

    // ─── Đã đăng nhập (mọi vai trò) ──────────────────────
    public class LoginRequiredAttribute : AuthorizeAttribute
    {
        public LoginRequiredAttribute()
        {
            // Không giới hạn role, chỉ cần đăng nhập
        }
    }
}
