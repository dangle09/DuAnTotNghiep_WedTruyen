using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TeenNovel_WED.Data;
using TeenNovel_WED.Filters;
using TeenNovel_WED.Models;

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

            // Inject danh sách thể loại vào navbar dropdown
            ViewBag.NavTheLoai = _context.TheoLoais
                .OrderBy(t => t.Tentheloa)
                .ToList();
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

            // Danh sách đánh giá
            var danhGias = await _context.DanhGias
                .Include(d => d.MaDocGiaNavigation)
                .Where(d => d.Matruyen == id)
                .OrderByDescending(d => d.Ngaydanhgia)
                .ToListAsync();

            double diemTb = danhGias.Any()
                ? Math.Round(danhGias.Average(d => (double)d.Sosao), 1)
                : 0;



            bool daTheoDoi = false;
            bool daThich = false;
            int? saoCuaToi = null;
            string? nxCuaToi = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
                if (int.TryParse(maDocGiaClaim, out int maDocGia))
                {
                    daTheoDoi = await _context.TheoDois
                        .AnyAsync(t => t.MaDocGia == maDocGia && t.Matruyen == id);

                    daThich = await _context.TheoDois
                        .AnyAsync(t => t.MaDocGia == maDocGia && t.Matruyen == id);

                    var dgCuaToi = await _context.DanhGias
                        .FirstOrDefaultAsync(d => d.MaDocGia == maDocGia && d.Matruyen == id);

                    if (dgCuaToi != null)
                    {
                        saoCuaToi = dgCuaToi.Sosao;
                        nxCuaToi = dgCuaToi.Nhanxet;
                    }
                }
            }


            ViewBag.ChuongMoi = chuongMoi;
            ViewBag.MaTruyen = id;
            ViewBag.DanhGias = danhGias;
            ViewBag.DiemTb = diemTb;
            ViewBag.DaTheoDoi = daTheoDoi;
            ViewBag.SaoCuaToi = saoCuaToi;
            ViewBag.NxCuaToi = nxCuaToi;

            return View(truyen);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TheoDoi(int matruyen)
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register",
                       new { returnUrl = $"/DocGia/ChiTiet/{matruyen}" });

            var existing = await _context.TheoDois
                .FirstOrDefaultAsync(t => t.MaDocGia == maDocGia && t.Matruyen == matruyen);

            if (existing != null)
            {
                // Bỏ theo dõi
                _context.TheoDois.Remove(existing);

                // Giảm lượt thích
                var truyen = await _context.Truyens.FindAsync(matruyen);
                if (truyen != null && truyen.LuotThich > 0)
                    truyen.LuotThich--;

                TempData["Success"] = "Đã bỏ theo dõi truyện.";
            }
            else
            {
                // Theo dõi mới
                _context.TheoDois.Add(new TeenNovel_WED.Models.TheoDoi
                {
                    MaDocGia = maDocGia,
                    Matruyen = matruyen,
                    Ngaytheodoi = DateTime.Now
                });

                // Tăng lượt thích
                var truyen = await _context.Truyens.FindAsync(matruyen);
                if (truyen != null) truyen.LuotThich++;

                TempData["Success"] = "Đã theo dõi truyện!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ChiTiet", new { id = matruyen });
        }

        // ─── ĐÁNH GIÁ TRUYỆN ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhGiaTruyen(int matruyen, int sosao, string? nhanxet)
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register",
                       new { returnUrl = $"/DocGia/ChiTiet/{matruyen}" });

            if (sosao < 1 || sosao > 5)
            {
                TempData["Error"] = "Vui lòng chọn số sao từ 1 đến 5.";
                return RedirectToAction("ChiTiet", new { id = matruyen });
            }

            // Kiểm tra đã đánh giá chưa
            var existing = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaDocGia == maDocGia && d.Matruyen == matruyen);

            if (existing != null)
            {
                // Cập nhật đánh giá cũ
                existing.Sosao = (byte)sosao;
                existing.Nhanxet = nhanxet?.Trim();
                existing.Ngaydanhgia = DateTime.Now;
                TempData["Success"] = "Đã cập nhật đánh giá của bạn!";
            }
            else
            {
                // Tạo đánh giá mới
                _context.DanhGias.Add(new TeenNovel_WED.Models.DanhGia
                {
                    MaDocGia = maDocGia,
                    Matruyen = matruyen,
                    Sosao = (byte)sosao,
                    Nhanxet = nhanxet?.Trim(),
                    Ngaydanhgia = DateTime.Now
                });
                TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ChiTiet", new { id = matruyen });
        }

        // ─── THỂ LOẠI ─────────────────────────────────────
        // /DocGia/TheLoai/{id}
        public async Task<IActionResult> TheLoai(int id)
        {
            ViewData["ActivePage"] = "TheLoai";

            var theloai = await _context.TheoLoais.FindAsync(id);
            if (theloai == null)
            {
                TempData["Error"] = "Không tìm thấy thể loại này.";
                return RedirectToAction("TrangChu");
            }

            ViewData["Title"] = theloai.Tentheloa;
            ViewBag.TheLoaiName = theloai.Tentheloa;
            ViewBag.TheLoaiId = id;

            var truyens = await _context.Truyens
                .Where(t => t.Matheloai == id)
                .OrderByDescending(t => t.LuotDoc)
                .ToListAsync();

            return View(truyens);
        }

        public async Task<IActionResult> TheoDoi()
        {
            ViewData["ActivePage"] = "TheoDoi";

            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Login_Register");

            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            var dsTheoDoi = await _context.TheoDois

                .Include(td => td.MatruyenNavigation)
                    .ThenInclude(t => t.MatheloaiNavigation)

                .Where(td => td.MaDocGia == maDocGia)

                .OrderByDescending(td => td.Ngaytheodoi)

                .ToListAsync();

            return View(dsTheoDoi);
        }

        //==============================================================
        // ĐỌC CHƯƠNG
        //==============================================================

        public async Task<IActionResult> DocChuong(int id)
        {
            ViewData["ActivePage"] = "";

            var chuong = await _context.Chuongs

                .Include(c => c.MatruyenNavigation)
                    .ThenInclude(t => t.MatheloaiNavigation)

                .FirstOrDefaultAsync(c => c.Machuong == id);

            if (chuong == null)
                return NotFound();

            var binhLuans = await _context.BinhLuans
                .Include(x => x.MaDocGiaNavigation)
                .Where(x => x.Matruyen == chuong.Matruyen)
                .Where(x => x.TrangThai == 1) // chỉ hiện bình luận đã duyệt
                .OrderByDescending(x => x.Ngaybinhluan)
                .ToListAsync();

            ViewBag.BinhLuans = binhLuans;



            //---------------------------------------
            // Tăng lượt đọc
            //---------------------------------------

            chuong.LuotDoc++;

            chuong.MatruyenNavigation.LuotDoc++;

            await _context.SaveChangesAsync();



            //---------------------------------------
            // Chương trước
            //---------------------------------------

            var chuongTruoc = await _context.Chuongs

                .Where(c => c.Matruyen == chuong.Matruyen &&
                            c.Thutu < chuong.Thutu)

                .OrderByDescending(c => c.Thutu)

                .FirstOrDefaultAsync();



            //---------------------------------------
            // Chương sau
            //---------------------------------------

            var chuongSau = await _context.Chuongs

                .Where(c => c.Matruyen == chuong.Matruyen &&
                            c.Thutu > chuong.Thutu)

                .OrderBy(c => c.Thutu)

                .FirstOrDefaultAsync();



            //---------------------------------------
            // Danh sách chương
            //---------------------------------------

            var dsChuong = await _context.Chuongs

                .Where(c => c.Matruyen == chuong.Matruyen)

                .OrderBy(c => c.Thutu)

                .ToListAsync();



            ViewBag.ChuongTruoc = chuongTruoc;

            ViewBag.ChuongSau = chuongSau;

            ViewBag.DanhSachChuong = dsChuong;



            return View(chuong);
        }

        [HttpPost]
        public async Task<IActionResult> ThemBinhLuan(int maTruyen, string noiDung)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Login_Register");

            if (string.IsNullOrWhiteSpace(noiDung))
                return Redirect(Request.Headers["Referer"].ToString());

            int maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            var bl = new BinhLuan
            {
                MaDocGia = maDocGia,
                Matruyen = maTruyen,
                Noidung = noiDung.Trim(),
                TrangThai = 1,
                Ngaybinhluan = DateTime.Now
            };

            _context.BinhLuans.Add(bl);

            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
