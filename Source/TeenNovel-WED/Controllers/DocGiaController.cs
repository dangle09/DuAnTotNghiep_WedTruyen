using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

        public IActionResult TrangChu()
        {
            return View();
        }
    }
}
