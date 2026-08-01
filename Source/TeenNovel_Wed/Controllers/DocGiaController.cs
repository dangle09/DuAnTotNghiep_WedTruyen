using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TeenNovel_Wed.Data;
using TeenNovel_Wed.Filters;
using TeenNovel_Wed.Models;
using TeenNovel_Wed.ViewModels;

namespace TeenNovel_Wed.Controllers
{
    public class DocGiaController : Controller
    {
        protected readonly TeenNovelDbContext _context;
        private readonly IConfiguration _configuration;
        private const int PAGE_SIZE_NAPXU = 3;

        public DocGiaController(TeenNovelDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            ViewBag.NavTheLoai = _context.TheLoais
                .OrderBy(t => t.Tentheloai)
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
            var dsTheLoai = await _context.TheLoais
                .OrderBy(t => t.Tentheloai)
                .ToListAsync();

            // Truyện nổi bật — top 10 lượt đọc cao nhất
            var truyenNoiBat = await _context.Truyens
                .Include(t => t.MaTheLoais)
                .OrderByDescending(t => t.LuotDoc)
                .Take(10)
                .ToListAsync();

            // Xếp hạng tuần — top 8 lượt đọc
            var truyenXepHang = await _context.Truyens
                .Include(t => t.MaTheLoais)
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
                .Include(t => t.MaTheLoais)
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

                .Include(t => t.MaTheLoais)

                .FirstOrDefaultAsync(t =>


                    t.Tentruyen.Contains(q)

                    ||

                    t.MaTacGiaNavigation.TenTacGia.Contains(q)

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
                .Include(t => t.MaTacGiaNavigation)
                .Include(t => t.MaTheLoais)
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

            HashSet<int> daMua = new();

            if (User.Identity!.IsAuthenticated)
            {
                int maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

                daMua = await _context.SuDungXus
                    .Where(x => x.MaDocGia == maDocGia)
                    .Select(x => x.Machuong)
                    .ToHashSetAsync();
            }

            ViewBag.DaMua = daMua;
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
                _context.TheoDois.Add(new TeenNovel_Wed.Models.TheoDoi
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
                _context.DanhGias.Add(new TeenNovel_Wed.Models.DanhGia
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

            var theloai = await _context.TheLoais.FindAsync(id);
            if (theloai == null)
            {
                TempData["Error"] = "Không tìm thấy thể loại này.";
                return RedirectToAction("TrangChu");
            }

            ViewData["Title"] = theloai.Tentheloai;
            ViewBag.TheLoaiName = theloai.Tentheloai;
            ViewBag.TheLoaiId = id;

            var truyens = await _context.Truyens
                .Where(t => t.MaTheLoais.Any(x => x.Matheloai == id))
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
                    .ThenInclude(t => t.MaTheLoais)

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

            int maDocGia = 0;

            if (User.Identity!.IsAuthenticated)
            {
                maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);
            }

            var chuong = await _context.Chuongs

                .Include(c => c.MatruyenNavigation)
                    .ThenInclude(t => t.MaTheLoais)

                .FirstOrDefaultAsync(c => c.Machuong == id);

            if (chuong == null)
                return NotFound();

            // Nếu chương có giá
            if (chuong.Giaxu > 0)
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Auth");
                }

                bool daMua = await _context.SuDungXus.AnyAsync(x =>
                    x.MaDocGia == maDocGia &&
                    x.Machuong == id);

                if (!daMua)
                {
                    TempData["Error"] = "Bạn cần mua chương này trước.";

                    return RedirectToAction("ChiTiet",
                        new
                        {
                            id = chuong.Matruyen
                        });
                }
            }

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
            // Lưu lịch sử đọc
            //---------------------------------------

            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (int.TryParse(maDocGiaClaim, out maDocGia))
            {
                var lichSu = await _context.LichSuDocs
                    .FirstOrDefaultAsync(x =>
                        x.MaDocGia == maDocGia &&
                        x.Machuong == chuong.Machuong);

                if (lichSu == null)
                {
                    lichSu = new LichSuDoc
                    {
                        MaDocGia = maDocGia,
                        Machuong = chuong.Machuong,
                        Ngaydoc = DateTime.Now
                    };

                    _context.LichSuDocs.Add(lichSu);
                }
                else
                {
                    lichSu.Ngaydoc = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

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

            List<Bookmark> dsBookmark = new();

            if (maDocGia > 0)
            {
                dsBookmark = await _context.Bookmarks
                    .Where(x => x.MaDocGia == maDocGia &&
                                x.Machuong == chuong.Machuong)
                    .OrderBy(x => x.SoDong)
                    .ToListAsync();
            }

            ViewBag.Bookmarks = dsBookmark;

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

        public async Task<IActionResult> LichSuDoc()
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            var lichSu = await _context.LichSuDocs
                .Where(x => x.MaDocGia == maDocGia)
                .Include(x => x.MachuongNavigation)
                    .ThenInclude(c => c.MatruyenNavigation)
                .OrderByDescending(x => x.Ngaydoc)
                .ToListAsync();

            // Mỗi truyện chỉ lấy lần đọc mới nhất
            var model = lichSu
                .GroupBy(x => x.MachuongNavigation.Matruyen)
                .Select(x => x.First())
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBookmark(
                                                        int maTruyen,
                                                        int maChuong,
                                                        int soDong,
                                                        string? ghiChu)
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            // Không cho bookmark trùng cùng dòng
            bool daTonTai = await _context.Bookmarks.AnyAsync(x =>
                x.MaDocGia == maDocGia &&
                x.Machuong == maChuong &&
                x.SoDong == soDong);

            if (daTonTai)
            {
                TempData["Error"] = "Dòng này đã được bookmark.";
                return RedirectToAction("DocChuong", new
                {
                    id = maChuong,
                    line = soDong
                });
            }

            var bookmark = new Bookmark
            {
                MaDocGia = maDocGia,
                Matruyen = maTruyen,
                Machuong = maChuong,
                SoDong = soDong,
                GhiChu = ghiChu?.Trim(),
                Ngaytao = DateTime.Now
            };

            _context.Bookmarks.Add(bookmark);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo bookmark.";

            return RedirectToAction("DocChuong", new
            {
                id = maChuong,
                line = soDong
            });
        }

        public async Task<IActionResult> Bookmark()
        {
            ViewData["ActivePage"] = "Bookmark";

            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            var bookmarks = await _context.Bookmarks
                .Include(x => x.MatruyenNavigation)
                .Include(x => x.MachuongNavigation)
                .Where(x => x.MaDocGia == maDocGia)
                .OrderByDescending(x => x.Ngaytao)
                .ToListAsync();

            return View(bookmarks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaBookmark(int maBookmark)
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(x =>
                    x.MaBookmark == maBookmark &&
                    x.MaDocGia == maDocGia);

            if (bookmark == null)
            {
                TempData["Error"] = "Không tìm thấy bookmark.";
                return RedirectToAction("Bookmark");
            }

            int maChuong = bookmark.Machuong;
            int soDong = bookmark.SoDong;

            _context.Bookmarks.Remove(bookmark);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa bookmark.";

            return RedirectToAction("DocChuong", new
            {
                id = maChuong,
                line = soDong
            });
        }

        //==============================================================
        // THÔNG TIN CÁ NHÂN
        //==============================================================
        [HttpGet]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            ViewData["ActivePage"] = "ThongTin";

            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            var docGia = await _context.DocGias
                .Include(x => x.MatkNavigation)
                .Include(x => x.NapXus)
                .FirstOrDefaultAsync(x => x.MaDocGia == maDocGia);

            if (docGia == null)
                return RedirectToAction("Login", "Login_Register");

            ViewBag.TongXuNap = docGia.NapXus
                 .Where(x => x.Trangthai == "DaThanhToan")
                 .Sum(x => x.Soxunhan);

            return View(docGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatThongTin(
                                                        string ten,
                                                        string? sodienthoai,
                                                        IFormFile? avatar)
        {
                var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;

                if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                    return RedirectToAction("Login", "Login_Register");

                var docGia = await _context.DocGias
                    .Include(x => x.MatkNavigation)
                    .FirstOrDefaultAsync(x => x.MaDocGia == maDocGia);

                if (docGia == null)
                    return RedirectToAction("Login", "Login_Register");

                docGia.Ten = ten.Trim();
                docGia.Sodienthoai = sodienthoai?.Trim();

                if (avatar != null && avatar.Length > 0)
                {
                    var allow = new[]
                    {
                "image/jpeg",
                "image/png",
                "image/webp"
                };

                if (!allow.Contains(avatar.ContentType))
                {
                    TempData["Error"] = "Avatar phải là JPG, PNG hoặc WEBP.";
                    return RedirectToAction(nameof(ThongTinCaNhan));
                }

                if (!string.IsNullOrEmpty(docGia.Avatar))
                {
                    var old = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        docGia.Avatar.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(old))
                        System.IO.File.Delete(old);
                }

                var fileName =
                    Guid.NewGuid() +
                    Path.GetExtension(avatar.FileName);

                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "avatar");

                Directory.CreateDirectory(folder);

                var path = Path.Combine(folder, fileName);

                using var stream =
                    new FileStream(path, FileMode.Create);

                await avatar.CopyToAsync(stream);

                docGia.Avatar =
                    "/uploads/avatar/" + fileName;
                }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật thông tin.";

            return RedirectToAction(nameof(ThongTinCaNhan));
        

        }

        // ─── LỊCH SỬ NẠP XU ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> LichSuNap(DateTime? tu, DateTime? den, int soLuong = PAGE_SIZE_NAPXU)
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            ViewData["ActivePage"] = "LichSuNap";
            ViewData["Title"] = "Lịch sử nạp xu";

            var docGia = await _context.DocGias
                .Include(d => d.MatkNavigation)
                .FirstOrDefaultAsync(d => d.MaDocGia == maDocGia);

            var query = _context.NapXus
                .Where(n => n.MaDocGia == maDocGia)
                .AsQueryable();

            if (tu.HasValue)
                query = query.Where(n => n.Ngaynap >= tu.Value.Date);
            if (den.HasValue)
                query = query.Where(n => n.Ngaynap < den.Value.Date.AddDays(1));

            int total = await query.CountAsync();

            var giaoDich = await query
                .OrderByDescending(n => n.Ngaynap)
                .Take(soLuong)
                .ToListAsync();

            ViewBag.DocGia = docGia;
            ViewBag.Tu = tu?.ToString("yyyy-MM-dd");
            ViewBag.Den = den?.ToString("yyyy-MM-dd");
            ViewBag.SoLuong = soLuong;
            ViewBag.Total = total;
            ViewBag.ConThem = total > soLuong;

            return View(giaoDich);
        }

        // ─── CHI TIẾT NẠP XU ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ChiTietLSNap(int id)
        {
            var maDocGiaClaim = User.FindFirst("MaDocGia")?.Value;
            if (!int.TryParse(maDocGiaClaim, out int maDocGia))
                return RedirectToAction("Login", "Login_Register");

            ViewData["ActivePage"] = "LichSuNap";
            ViewData["Title"] = "Chi tiết nạp xu";

            var giaoDich = await _context.NapXus
                .FirstOrDefaultAsync(n => n.Manap == id && n.MaDocGia == maDocGia);

            if (giaoDich == null)
            {
                TempData["Error"] = "Không tìm thấy giao dịch.";
                return RedirectToAction("LichSuNap");
            }

            var docGia = await _context.DocGias
                .FirstOrDefaultAsync(d => d.MaDocGia == maDocGia);

            ViewBag.DocGia = docGia;

            return View(giaoDich);
        }

        // ================================================================
        // TRUNG TÂM HỖ TRỢ
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> HoTro()
        {
            ViewData["Title"] = "Trung tâm hỗ trợ";
            ViewData["ActivePage"] = "HoTro";

            return View();
        }


        // ================================================================
        // GỬI YÊU CẦU
        // ================================================================

        [HttpGet]
        public IActionResult GuiYeuCau()
        {
            ViewData["Title"] = "Gửi yêu cầu";
            ViewData["ActivePage"] = "HoTro";

            return View(new HoTro());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiYeuCau(string tieude, string noidung)
        {
            var maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            if (string.IsNullOrWhiteSpace(tieude))
            {
                TempData["Error"] = "Vui lòng nhập tiêu đề.";
                return RedirectToAction(nameof(HoTro));
            }

            if (string.IsNullOrWhiteSpace(noidung))
            {
                TempData["Error"] = "Vui lòng nhập nội dung.";
                return RedirectToAction(nameof(HoTro));
            }

            HoTro hoTro = new HoTro()
            {
                MaDocGia = maDocGia,
                TieuDe = tieude.Trim(),
                NoiDung = noidung.Trim(),
                TrangThai = 0,
                NgayGui = DateTime.Now
            };

            _context.HoTros.Add(hoTro);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi yêu cầu hỗ trợ.";

            return RedirectToAction(nameof(LichSuHoTro));
        }

        // ================================================================
        // LỊCH SỬ HỖ TRỢ
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> LichSuHoTro()
        {
            ViewData["Title"] = "Lịch sử hỗ trợ";
            ViewData["ActivePage"] = "HoTro";

            var maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            var ds = await _context.HoTros

                .Where(x => x.MaDocGia == maDocGia)

                .OrderByDescending(x => x.NgayGui)

                .ToListAsync();

            return View(ds);
        }

        // ================================================================
        // CHI TIẾT HỖ TRỢ
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> ChiTietHoTro(int id)
        {
            ViewData["Title"] = "Chi tiết hỗ trợ";
            ViewData["ActivePage"] = "HoTro";

            var maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            var hoTro = await _context.HoTros

                .Include(x => x.ManvNavigation)

                .FirstOrDefaultAsync(x => x.MaHoTro == id &&
                                          x.MaDocGia == maDocGia);

            if (hoTro == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu.";
                return RedirectToAction(nameof(LichSuHoTro));
            }

            return View(hoTro);
        }

        // ================================================================
        // NẠP XU
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> NapXu()
        {
            ViewData["Title"] = "Nạp xu";
            ViewData["ActivePage"] = "NapXu";

            int maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            await XoaDonNapHetHan(maDocGia);

            var goiNap = await _context.GoiNapXus
                .Where(x => x.HienThi)
                .OrderBy(x => x.SoTien)
                .ToListAsync();

            return View(goiNap);
        }

        // ================================================================
        // CHI TIẾT NẠP
        // ================================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChiTietNap(int id)
        {
            var goiNap = await _context.GoiNapXus
                .FirstOrDefaultAsync(x => x.MaGoiNap == id && x.HienThi);

            if (goiNap == null)
                return NotFound();

            return View(goiNap);
        }

        // ================================================================
        // TẠO ĐƠN NẠP
        // ================================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoDonNap(int id)
        {
            var goiNap = await _context.GoiNapXus
                .FirstOrDefaultAsync(x => x.MaGoiNap == id && x.HienThi);

            if (goiNap == null)
            {
                return Json(new { success = false, message = "Không tìm thấy gói nạp." });
            }

            var maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            await XoaDonNapHetHan(maDocGia);

            // Tối ưu: nếu người dùng đã có đơn "ChoThanhToan" y hệt gói này chưa quá hạn (ví dụ 15 phút),
            // dùng lại đơn cũ thay vì tạo rác trong DB khi họ bấm nút nhiều lần / F5 lại trang.
            var existing = await _context.NapXus
                .FirstOrDefaultAsync(x =>
                    x.MaDocGia == maDocGia &&
                    x.MaGoiNap == goiNap.MaGoiNap &&
                    x.Trangthai == "ChoThanhToan");

            NapXu napXu;

            if (existing != null)
            {
                napXu = existing;
            }
            else
            {
                napXu = new NapXu
                {
                    MaDocGia = maDocGia,
                    MaGoiNap = goiNap.MaGoiNap,
                    Sotien = goiNap.SoTien,
                    Soxunhan = goiNap.SoXuNhan,
                    Phuongthuc = "Banking",
                    NoiDungChuyenKhoan = "",
                    Trangthai = "ChoThanhToan",
                    Ngaynap = DateTime.Now
                };

                _context.NapXus.Add(napXu);
                await _context.SaveChangesAsync(); // sinh Manap

                // Dùng chính ID vừa sinh ra làm nội dung chuyển khoản, thêm hậu tố để giảm rủi ro
                // bị match nhầm do Contains() (VD: CK1 khớp nhầm với CK15)
                string random = $"{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
    
                napXu.NoiDungChuyenKhoan = $"CK{napXu.Manap}{random}";
                await _context.SaveChangesAsync();
            }

            Console.WriteLine($"Nội dung chuyển khoản: {napXu.NoiDungChuyenKhoan}");

            string bankBin = _configuration["VietQR:BankBin"]!;
            string bankName = _configuration["VietQR:BankName"]!;
            string accountNumber = _configuration["VietQR:AccountNumber"]!;
            string accountName = _configuration["VietQR:AccountName"]!;

            long amount = Convert.ToInt64(napXu.Sotien);

            string qrUrl =
                $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact2.png" +
                $"?amount={amount}" +
                $"&addInfo={Uri.EscapeDataString(napXu.NoiDungChuyenKhoan)}" +
                $"&accountName={Uri.EscapeDataString(accountName ?? "")}";

            return Json(new
            {
                success = true,
                maNap = napXu.Manap,
                qrUrl,
                noiDung = napXu.NoiDungChuyenKhoan,
                soTien = napXu.Sotien,
                tenGoi = goiNap.TenGoi,
                soTaiKhoan = accountNumber,
                tenTaiKhoan = accountName,
                nganHang = bankName
            });
        }

        // ================================================================
        // KIỂM TRA TRẠNG THÁI
        // ================================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckStatus(int id)
        {
            var maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            var nap = await _context.NapXus
                .FirstOrDefaultAsync(x => x.Manap == id && x.MaDocGia == maDocGia);

            if (nap == null)
                return Json(new { status = "khongtimthay" });

            return Json(new { status = nap.Trangthai });
        }


        // ================================================================
        // MUA TRUYỆN TIÊU XU
        // ================================================================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MuaChuong(int maChuong)
        {
            int maDocGia = int.Parse(User.FindFirst("MaDocGia")!.Value);

            var chuong = await _context.Chuongs
                .Include(x => x.MatruyenNavigation)
                .FirstOrDefaultAsync(x => x.Machuong == maChuong);

            if (chuong == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy chương."
                });
            }

            // Chương miễn phí
            if (chuong.Giaxu <= 0)
            {
                return Json(new
                {
                    success = true,
                    redirect = Url.Action("DocChuong", new { id = maChuong })
                });
            }

            // Đã mua chưa
            bool daMua = await _context.SuDungXus.AnyAsync(x =>
                x.MaDocGia == maDocGia &&
                x.Machuong == maChuong);

            if (daMua)
            {
                return Json(new
                {
                    success = true,
                    redirect = Url.Action("DocChuong", new { id = maChuong })
                });
            }

            var docGia = await _context.DocGias
                .FirstAsync(x => x.MaDocGia == maDocGia);

            // Không đủ xu
            if (docGia.Soxu < chuong.Giaxu)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn không đủ xu để mua chương."
                });
            }

            // Trừ xu
            docGia.Soxu -= chuong.Giaxu;

            // Lưu lịch sử
            _context.SuDungXus.Add(new SuDungXu
            {
                MaDocGia = maDocGia,
                Machuong = maChuong,
                SoXu = chuong.Giaxu,
                NgaySuDung = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                redirect = Url.Action("DocChuong", new { id = maChuong }),
                soXu = docGia.Soxu
            });
        }

        private async Task XoaDonNapHetHan(int maDocGia)
        {
            var dsHetHan = await _context.NapXus
                .Where(x => x.MaDocGia == maDocGia
                         && x.Trangthai == "ChoThanhToan"
                         && x.TgHetHan < DateTime.Now)
                .ToListAsync();

            if (dsHetHan.Any())
            {
                _context.NapXus.RemoveRange(dsHetHan);
                await _context.SaveChangesAsync();
            }
        }
    }
}
