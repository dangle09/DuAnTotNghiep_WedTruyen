-- ============================================================
--  WebTruyen - WEB TRUYEN | DATABASE SCRIPT (SQL Server)
--  Phien ban hoan chinh - Database First
-- ============================================================

CREATE DATABASE WebTruyen;
GO

USE WebTruyen;
GO

-- ============================================================
--  1. VAITRO - Vai tro / phan quyen
-- ============================================================
CREATE TABLE VaiTro (
    MaVaiTro    INT           PRIMARY KEY IDENTITY(1,1),
    TenVaiTro   VARCHAR(30)   NOT NULL UNIQUE  -- 'docgia' | 'nhanvien' | 'quantrivien'
);
GO

-- ============================================================
--  2. TAIKHOAN - Thong tin dang nhap (chung cho moi vai tro)
-- ============================================================
CREATE TABLE TaiKhoan (
    Matk            INT             PRIMARY KEY IDENTITY(1,1),
    MaVaiTro        INT             NOT NULL,
    Email           VARCHAR(150)    NOT NULL UNIQUE,
    Matkhau         VARCHAR(255)    NOT NULL,
    Tendangnhap     NVARCHAR(50)    NOT NULL UNIQUE,
    Trangthai       NVARCHAR(20)    NOT NULL DEFAULT N'hoatdong', -- hoatdong | khoatai
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- ============================================================
--  3. DOCGIA - Thong tin ca nhan cua doc gia
-- ============================================================
CREATE TABLE DocGia (
    MaDocGia        INT             PRIMARY KEY IDENTITY(1,1),
    Matk            INT             NOT NULL UNIQUE,
    Ten             NVARCHAR(100)   NOT NULL,
    Sodienthoai     VARCHAR(15),
    Avatar          NVARCHAR(500),
    Soxu            INT             NOT NULL DEFAULT 0,
    Ngaytao         DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (Matk) REFERENCES TaiKhoan(Matk)
);
GO

-- ============================================================
--  4. NHANVIEN - Thong tin ca nhan cua nhan vien / quan tri
-- ============================================================
CREATE TABLE NhanVien (
    Manv            INT             PRIMARY KEY IDENTITY(1,1),
    Matk            INT             NOT NULL UNIQUE,
    Ten             NVARCHAR(100)   NOT NULL,
    Sodienthoai     VARCHAR(15),
    Gioitinh        NVARCHAR(10),
    Ngaysinh        DATETIME,
    Ngayvaolamm     DATETIME        NOT NULL DEFAULT GETDATE(),
    Trangthai       NVARCHAR(20)    NOT NULL DEFAULT N'hoatdong',
    FOREIGN KEY (Matk) REFERENCES TaiKhoan(Matk)
);
GO

-- ============================================================
--  5. THEOLOAI - The loai truyen
-- ============================================================
CREATE TABLE TheoLoai (
    Matheloai       INT             PRIMARY KEY IDENTITY(1,1),
    Tentheloa       NVARCHAR(100)   NOT NULL UNIQUE,
    Mota            NVARCHAR(500)
);
GO

-- ============================================================
--  6. TRUYEN - Truyen
-- ============================================================
CREATE TABLE Truyen (
    Matruyen        INT             PRIMARY KEY IDENTITY(1,1),
    Manv            INT             NOT NULL,   -- nhan vien dang truyen
    Matheloai       INT             NOT NULL,
    Tentruyen       NVARCHAR(200)   NOT NULL,
    Mota            NVARCHAR(2000),
    Tacgia          NVARCHAR(100),
    AnhBia          NVARCHAR(500),              -- anh bia truyen
    Theloai         NVARCHAR(200),              -- the loai phu / tag
    LuotDoc         INT             NOT NULL DEFAULT 0,
    LuotThich       INT             NOT NULL DEFAULT 0,
    Trangthai       NVARCHAR(30)    NOT NULL DEFAULT N'Đang ra', -- dangra | hoanthanh | tamngung
    Ngaydang        DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (Manv)      REFERENCES NhanVien(Manv),
    FOREIGN KEY (Matheloai) REFERENCES TheoLoai(Matheloai)
);
GO

-- ============================================================
--  7. CHUONG - Chuong truyen
-- ============================================================
CREATE TABLE Chuong (
    Machuong        INT             PRIMARY KEY IDENTITY(1,1),
    Matruyen        INT             NOT NULL,
    Tenchuong       NVARCHAR(200)   NOT NULL,
    Noidung         NVARCHAR(MAX)   NOT NULL,
    Giaxu           INT             NOT NULL DEFAULT 0,  -- 0 = mien phi
    Thutu           INT             NOT NULL DEFAULT 1,
    LuotDoc         INT             NOT NULL DEFAULT 0,
    Ngaydang        DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (Matruyen) REFERENCES Truyen(Matruyen)
);
GO

-- ============================================================
--  8. THEODOI - Doc gia theo doi truyen
-- ============================================================
CREATE TABLE TheoDoi (
    Matheodoi       INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Matruyen        INT             NOT NULL,
    Ngaytheodoi     DATETIME        NOT NULL DEFAULT GETDATE(),
    UNIQUE (MaDocGia, Matruyen),
    FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    FOREIGN KEY (Matruyen)  REFERENCES Truyen(Matruyen)
);
GO

-- ============================================================
--  9. LICHSUDOC - Luu chuong doc gan nhat cua doc gia
-- ============================================================
CREATE TABLE LichSuDoc (
    Malichsu        INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Machuong        INT             NOT NULL,
    Ngaydoc         DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    FOREIGN KEY (Machuong)  REFERENCES Chuong(Machuong)
);
GO

-- ============================================================
-- 10. BOOKMARK - Danh dau dong doc thu cong
-- ============================================================
CREATE TABLE Bookmark (
    MaBookmark      INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Matruyen        INT             NOT NULL,   -- de query nhanh khong can JOIN
    Machuong        INT             NOT NULL,
    SoDong          INT             NOT NULL,   -- so dong duoc ghim trong chuong
    GhiChu          NVARCHAR(200),              -- ghi chu tuy chon cua nguoi dung
    Ngaytao         DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    FOREIGN KEY (Matruyen)  REFERENCES Truyen(Matruyen),
    FOREIGN KEY (Machuong)  REFERENCES Chuong(Machuong)
);
GO

-- ============================================================
-- 11. DANHGIA - Danh gia / xep hang truyen
-- ============================================================
CREATE TABLE DanhGia (
    Madanhgia       INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Matruyen        INT             NOT NULL,
    Sosao           TINYINT         NOT NULL CHECK (Sosao BETWEEN 1 AND 5),
    Nhanxet         NVARCHAR(1000),
    Ngaydanhgia     DATETIME        NOT NULL DEFAULT GETDATE(),
    UNIQUE (MaDocGia, Matruyen),    -- moi doc gia chi danh gia 1 lan / truyen
    FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    FOREIGN KEY (Matruyen)  REFERENCES Truyen(Matruyen)
);
GO

-- ============================================================
-- 12. BINHLUAN - Binh luan trong truyen
-- ============================================================
CREATE TABLE BinhLuan (
    Mabinhluan      INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Matruyen        INT             NOT NULL,
    Noidung         NVARCHAR(1000)  NOT NULL,
    TrangThai       TINYINT         NOT NULL DEFAULT 1, -- 0=bi an | 1=hien thi
    Ngaybinhluan    DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    FOREIGN KEY (Matruyen)  REFERENCES Truyen(Matruyen)
);
GO

-- ============================================================
-- 13. NAPXU - Nap xu vao tai khoan doc gia
-- ============================================================
CREATE TABLE NapXu (
    Manap           INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Soxunhan        INT             NOT NULL,
    Sotien          DECIMAL(12,2)   NOT NULL,
    Phuongthuc      VARCHAR(50)     NOT NULL DEFAULT 'momo', -- momo | vnpay | banking
    Trangthai       NVARCHAR(20)    NOT NULL DEFAULT N'thanhcong', -- thanhcong | thatbai | choxuly
    Ngaynap         DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MaDocGia) REFERENCES DocGia(MaDocGia)
);
GO

-- ============================================================
-- 14. DOANHTHU - Doanh thu thong ke theo thang
-- ============================================================
CREATE TABLE DoanhThu (
    Madoanhthu      INT             PRIMARY KEY IDENTITY(1,1),
    Thang           INT         NOT NULL CHECK (Thang BETWEEN 1 AND 12),
    Nam             INT        NOT NULL,
    Tongtien        DECIMAL(15,2)   NOT NULL DEFAULT 0,
    Tongxunap       INT             NOT NULL DEFAULT 0,
    LuotNap         INT             NOT NULL DEFAULT 0   -- so lan nap trong thang
);
GO

-- ============================================================
-- 15. HOTRO - Ho tro / lien he CSKH
-- ============================================================
CREATE TABLE HoTro (
    MaHoTro         INT             PRIMARY KEY IDENTITY(1,1),
    MaDocGia        INT             NOT NULL,
    Manv            INT,                        -- NULL = chua phan cong
    TieuDe          NVARCHAR(200)   NOT NULL,
    NoiDung         NVARCHAR(MAX)   NOT NULL,
    PhanHoi         NVARCHAR(MAX),
    TrangThai       TINYINT         NOT NULL DEFAULT 0, -- 0=cho | 1=dangxuly | 2=xong
    NgayGui         DATETIME        NOT NULL DEFAULT GETDATE(),
    NgayXuLy        DATETIME,
    FOREIGN KEY (MaDocGia)  REFERENCES DocGia(MaDocGia),
    FOREIGN KEY (Manv)      REFERENCES NhanVien(Manv)
);
GO


-- ============================================================
--  DU LIEU MAU
-- ============================================================

-- ---------- VaiTro ----------
INSERT INTO VaiTro (TenVaiTro) VALUES
('quantrivien'),
('nhanvien'),
('docgia');
GO

-- ---------- TaiKhoan ----------
-- MaVaiTro: 1=quantrivien | 2=nhanvien | 3=docgia
INSERT INTO TaiKhoan (MaVaiTro, Email, Matkhau, Tendangnhap, Trangthai)
VALUES
(1, 'danglntb02258@gmail.com',    '$2b$10$HASH_ADMIN',  'admin',        N'hoatdong'),
(2, 'binh1.nv@gmail.com',  '$2b$10$HASH_NV1',   'nhanvien01',   N'hoatdong'),
(2, 'cam2.nv@gmail.com',   '$2b$10$HASH_NV2',   'nhanvien02',   N'hoatdong'),
(3, 'duc1.lm@gmail.com',     '$2b$10$HASH_DG1',   'leMinh',       N'hoatdong'),
(3, 'hoa2.pt@gmail.com',     '$2b$10$HASH_DG2',   'phamHoa',      N'hoatdong'),
(3, 'hung3.nq@gmail.com',    '$2b$10$HASH_DG3',   'hungNQ',       N'hoatdong'),
(3, 'lan4.vt@gmail.com',     '$2b$10$HASH_DG4',   'lanVT',        N'hoatdong'),
(3, 'nam5.dh@gmail.com',     '$2b$10$HASH_DG5',   'namDH',        N'hoatdong'),
(3, 'ngoc6.bt@gmail.com',    '$2b$10$HASH_DG6',   'buitngoc',     N'hoatdong'),
(3, 'phuc7.hv@gmail.com',    '$2b$10$HASH_DG7',   'hoangPhuc',    N'khoatai');
GO

-- ---------- NhanVien ----------
INSERT INTO NhanVien (Matk, Ten, Sodienthoai, Gioitinh, Ngaysinh, Ngayvaolamm, Trangthai)
VALUES
(1, N'Quản Trị Viên',   '0901000001', N'Nam', '1990-01-01', '2024-01-01', N'hoatdong'),
(2, N'Nguyễn Văn Bình', '0901000002', N'Nam', '1995-06-15', '2024-01-05', N'hoatdong'),
(3, N'Trần Thị Cẩm',    '0901000003', N'Nữ',  '1998-09-20', '2024-01-05', N'hoatdong');
GO

-- ---------- DocGia ----------
INSERT INTO DocGia (Matk, Ten, Sodienthoai, Avatar, Soxu, Ngaytao)
VALUES
(4,  N'Lê Minh Đức',      '0908111001', NULL, 200,  '2024-02-10'),
(5,  N'Phạm Thị Hoa',     '0908111002', NULL, 500,  '2024-02-15'),
(6,  N'Nguyễn Quốc Hùng', '0908111003', NULL, 50,   '2024-03-01'),
(7,  N'Võ Thanh Lan',     '0908111004', NULL, 0,    '2024-03-10'),
(8,  N'Đặng Hoàng Nam',   '0908111005', NULL, 300,  '2024-04-01'),
(9,  N'Bùi Thị Ngọc',     '0908111006', NULL, 100,  '2024-04-20'),
(10, N'Hoàng Văn Phúc',   '0908111007', NULL, 0,    '2024-05-05');
GO

-- ---------- TheoLoai ----------
INSERT INTO TheoLoai (Tentheloa, Mota)
VALUES
(N'Ngôn Tình',              N'Truyện kể về các câu chuyện tình yêu lãng mạn'),
(N'Tiên Hiệp',              N'Truyện hành động, chiến đấu, tu luyện trong thế giới huyền ảo'),
(N'Huyền Huyễn',            N'Truyện về cuộc chiến giữa các nhân vật ma quỷ và anh hùng'),
(N'Manga',                  N'Truyện tranh chuyển thể sang chữ, thường mang phong cách Nhật Bản'),
(N'Trinh Thám',             N'Truyện trinh thám, điều tra, phá án'),
(N'Lịch Sử',               N'Truyện về lịch sử Việt Nam và thế giới'),
(N'Hài Hước',               N'Truyện hài hước, vui nhộn'),
(N'Khoa Học Viễn Tưởng',   N'Truyện khoa học viễn tưởng, công nghệ tương lai'),
(N'Kinh Dị',               N'Truyện kinh dị, rùng rợn, ma quái'),
(N'Thể Thao',              N'Truyện thể thao, thi đấu, tranh tài');
GO

-- ---------- Truyen ----------
-- Manv: 1=admin | 2=binh | 3=cam
INSERT INTO Truyen (Manv, Matheloai, Tentruyen, Mota, Tacgia, AnhBia, Theloai, LuotDoc, LuotThich, Trangthai, Ngaydang)
VALUES
(2, 1, N'Cạm Bẫy Ngọt Ngào',
    N'Cô gái nghèo bỗng dưng trở thành vợ hợp đồng của tổng tài lạnh lùng. Cuộc hôn nhân không tình yêu liệu có thể nảy sinh những cảm xúc thật sự?',
    N'Mộng Vô Tình', NULL, N'Ngôn Tình, Hiện Đại, Tổng Tài', 1520, 234, N'dangtien',   '2024-03-01'),

(2, 2, N'Vạn Cổ Thần Vương',
    N'Thiếu niên bình thường nhặt được một viên đá kỳ lạ, từ đó bắt đầu hành trình tu tiên, trở thành đệ nhất cường giả thống trị thiên hạ.',
    N'Lão Hổ Mèo',  NULL, N'Tiên Hiệp, Tu Luyện, Mạnh Mẽ',   3840, 512, N'dangtien',   '2024-02-15'),

(2, 3, N'Dị Giới Chi Vương',
    N'Người hiện đại xuyên qua dị giới, sở hữu hệ thống siêu cấp, từng bước xưng bá một phương trời.',
    N'Thiên Tàm Thổ Đậu', NULL, N'Huyền Huyễn, Xuyên Không, Hệ Thống', 2100, 178, N'hoanthanh', '2024-01-10'),

(2, 5, N'Bí Ẩn Căn Phòng 404',
    N'Thám tử Minh Khoa nhận điều tra vụ mất tích bí ẩn tại chung cư cũ. Càng đào sâu, anh càng phát hiện những sự thật kinh hoàng ẩn giấu sau những cánh cửa khóa.',
    N'Hải Đăng',    NULL, N'Trinh Thám, Bí Ẩn, Tâm Lý',       980,  143, N'dangtien',   '2024-04-01'),

(2, 1, N'Em Là Ánh Nắng Của Anh',
    N'Câu chuyện tình yêu trong sáng giữa hai người bạn thời thơ ấu sau bao năm xa cách gặp lại nhau.',
    N'Kẹo Bông Gòn', NULL, N'Ngôn Tình, Thanh Xuân, Campus',   1760, 290, N'hoanthanh',  '2024-01-20'),

(3, 9, N'Nhà Số 7 Bí Ẩn',
    N'Căn nhà bỏ hoang từ 30 năm trước bỗng sáng đèn trở lại. Nhóm bạn trẻ tò mò khám phá và dần nhận ra nơi đây ẩn chứa điều gì đó không phải con người.',
    N'Ma Mị',       NULL, N'Kinh Dị, Ma, Tâm Linh',            640,  98,  N'dangtien',   '2024-05-01'),

(3, 7, N'Ông Chủ Khó Đỡ',
    N'Sếp lớn nghiêm khắc VS nhân viên lém lỉnh. Cuộc chiến văn phòng hài hước mỗi ngày.',
    N'Cười Thả Ga', NULL, N'Hài Hước, Công Sở, Lãng Mạn',     2340, 387, N'dangtien',   '2024-04-15'),

(2, 8, N'Năm 2387 - Ký Sự Tận Thế',
    N'Trái đất năm 2387, văn minh nhân loại gần như bị xóa sổ. Nhóm người sống sót cuối cùng tìm kiếm hi vọng trong vũ trụ bao la.',
    N'Vũ Trụ Ký',  NULL, N'Khoa Học Viễn Tưởng, Hậu Tận Thế', 890,  120, N'dangtien',   '2024-03-20');
GO

-- ---------- Chuong ----------
INSERT INTO Chuong (Matruyen, Tenchuong, Noidung, Giaxu, Thutu, LuotDoc, Ngaydang)
VALUES
-- Truyen 1: Cam Bay Ngot Ngao
(1, N'Chương 1: Cuộc Gặp Gỡ Định Mệnh',
    N'Mưa rơi tầm tã trên con phố vắng. Linh cúi đầu che chiếc ví rách, bước nhanh về phía bến xe buýt. 
Cô vừa mất việc, trong túi chỉ còn vài đồng bạc lẻ đủ mua một bữa ăn đơn giản. 
Đôi giày cũ đã thấm nước nhưng cô vẫn cố bước tiếp vì không muốn bỏ lỡ chuyến xe cuối cùng. 
Đúng lúc đó, một chiếc xe Bentley đen bóng lao qua vũng nước, té ướt toàn bộ bộ quần áo duy nhất còn lành lặn của cô. 
Linh đứng sững người, tức giận nhìn theo chiếc xe sang trọng đang dừng lại phía trước. 
Cửa xe hạ xuống, lộ ra khuôn mặt lạnh băng của người đàn ông trẻ tuổi. 
Ánh mắt anh ta bình thản nhìn cô gái đang run rẩy giữa cơn mưa. 
"Tôi xin lỗi." Giọng nói trầm thấp vang lên nhưng không hề có cảm xúc. 
Linh cắn môi, không muốn nhận sự thương hại từ một người xa lạ. 
"Cảm ơn, nhưng tôi tự lo được." 
Người đàn ông im lặng vài giây rồi nhìn chiếc ví cũ trong tay cô. 
"Minh Khải. Tôi cần một người có thể giúp tôi giải quyết một vấn đề." 
Linh không hiểu ý anh ta nhưng linh cảm rằng cuộc gặp này sẽ thay đổi cuộc đời mình. 
"Đừng từ chối vội. Có thể cô sẽ cần lời đề nghị này." 
Chiếc xe rời đi để lại Linh đứng giữa màn mưa lạnh giá. 
Cô không biết rằng người đàn ông kia chính là người thừa kế tập đoàn lớn nhất thành phố.',
    0, 1, 520, '2024-03-01'),

(1, N'Chương 2: Hợp Đồng Hôn Nhân',
    N'Văn phòng tổng giám đốc tầng 30 nhìn xuống toàn bộ thành phố. 
Linh khẽ nuốt nước bọt khi đặt bút ký vào tờ giấy trước mặt. 
Cô chưa từng nghĩ có ngày mình sẽ ngồi trong căn phòng xa hoa như thế này. 
Minh Khải ngồi đối diện, khuôn mặt không biểu lộ bất kỳ cảm xúc nào. 
"Hôn nhân hợp đồng, thời hạn một năm." 
Anh đẩy tập tài liệu về phía cô. 
"Cô sẽ đóng vai vợ tôi tại các sự kiện quan trọng." 
Linh nhìn những dòng chữ trên giấy, cảm thấy mọi thứ quá khó tin. 
"Vì sao lại là tôi?" 
Minh Khải im lặng một lúc rồi trả lời. 
"Vì cô không sợ tôi." 
Câu trả lời khiến Linh bất ngờ. 
Những người xung quanh anh luôn tìm cách lấy lòng hoặc lợi dụng anh. 
Nhưng cô gái trước mặt lại dám thẳng thắn từ chối sự giúp đỡ. 
"Đổi lại, mọi khoản nợ của gia đình cô sẽ được xóa sạch." 
Linh nhớ đến căn nhà nhỏ và những khó khăn đang chờ phía trước. 
Cô cầm bút lên, ký tên vào bản hợp đồng. 
Từ khoảnh khắc đó, cuộc sống bình thường của cô chính thức kết thúc.',
    0, 2, 480, '2024-03-03'),

(1, N'Chương 3: Ngôi Nhà Lạnh Giá',
    N'Biệt thự rộng lớn đến mức Linh có thể lạc đường trong đó. 
Người quản gia dẫn cô lên phòng - rộng gấp ba lần căn hộ nhà cô, nhưng lạnh lẽo như băng. 
Không một bức ảnh, không một vật dụng cá nhân. 
Mọi thứ đều hoàn hảo nhưng thiếu đi hơi ấm của một gia đình. 
"Cô Linh, đây là phòng của cô." 
Người quản gia cúi đầu rồi rời đi. 
Linh đặt chiếc vali nhỏ xuống sàn, cảm thấy mình giống một vị khách xa lạ. 
Buổi tối đầu tiên, cô ngồi một mình bên bàn ăn lớn. 
Minh Khải về nhà rất muộn. 
Anh tháo áo khoác, nhìn thấy cô vẫn đang chờ. 
"Cô không cần đợi tôi." 
"Tôi chỉ nghĩ chúng ta nên làm quen vì còn phải đóng vai vợ chồng." 
Anh hơi ngạc nhiên trước sự thẳng thắn của cô. 
Trong căn biệt thự lạnh lẽo ấy, lần đầu tiên có người nói chuyện với anh như một người bình thường. 
Minh Khải quay đi nhưng khóe môi khẽ thay đổi.',
    5, 3, 310, '2024-03-07'),

(1, N'Chương 4: Bữa Ăn Đầu Tiên',
    N'Bếp trưởng dọn lên bàn tiệc đầy đủ sơn hào hải vị. 
Hai người ngồi hai đầu chiếc bàn dài mười mét. 
Không khí im lặng đến mức có thể nghe rõ tiếng dao nĩa chạm vào đĩa. 
Linh nhìn những món ăn trước mặt nhưng chỉ gắp vài món đơn giản. 
"Cô không ăn được hải sản à?" Minh Khải đột nhiên lên tiếng. 
Linh ngẩng đầu ngạc nhiên - anh ta để ý? 
"Tôi bị dị ứng nhẹ." 
Minh Khải lập tức gọi quản gia đổi toàn bộ món ăn. 
Linh nhìn anh khó hiểu. 
"Tôi tưởng anh không quan tâm." 
"Tôi chỉ không muốn người sống trong nhà tôi gặp vấn đề." 
Dù lời nói lạnh lùng nhưng hành động lại hoàn toàn khác. 
Linh bắt đầu nhận ra người đàn ông này không giống vẻ ngoài của anh. 
Bên trong sự lạnh lùng ấy có thể vẫn tồn tại sự quan tâm. 
Bữa ăn đầu tiên của họ kết thúc trong một bầu không khí khác hẳn.',
    5, 4, 210, '2024-03-10'),

-- Truyen 2: Van Co Than Vuong
(2, N'Chương 1: Viên Đá Kỳ Lạ',
    N'Ngọn núi sau làng luôn bị cấm đặt chân từ khi Thiên Vũ còn nhỏ. 
Người trong làng nói rằng nơi đó chứa những bí mật không ai được phép biết. 
Nhưng hôm nay, vì đuổi theo con dê của nhà, cậu vô tình lạc vào sâu bên trong. 
Cây cối nơi này cao lớn hơn bình thường, ánh sáng gần như không thể xuyên qua. 
Dưới gốc cây cổ thụ nghìn năm tuổi, có một viên đá màu đen nhỏ bằng nắm tay đang phát ra ánh sáng kỳ lạ. 
Thiên Vũ tò mò tiến lại gần. 
Ngay khi bàn tay chạm vào viên đá, một luồng năng lượng mạnh mẽ truyền vào cơ thể cậu. 
Cậu nghe thấy một giọng nói xa lạ vang lên trong đầu. 
"Người kế thừa đã xuất hiện." 
Thiên Vũ hoảng sợ lùi lại nhưng viên đá đã biến mất. 
Một dấu ấn kỳ lạ xuất hiện trên lòng bàn tay. 
Cậu không biết rằng thứ mình vừa chạm vào chính là bảo vật thất truyền hàng nghìn năm. 
Từ ngày hôm đó, số phận của cậu bắt đầu thay đổi. 
Một con đường tu luyện chưa từng có đang chờ phía trước.',
    0, 1, 980, '2024-02-15'),

(2, N'Chương 2: Thức Tỉnh Thiên Phú',
    N'Ba ngày ba đêm Thiên Vũ nằm mê man bất tỉnh. 
Khi tỉnh dậy, cậu phát hiện mình đang nằm trong căn nhà nhỏ của gia đình. 
Nhưng mọi thứ xung quanh đã thay đổi. 
Cậu có thể nhìn thấy những luồng linh khí vô hình đang chuyển động trong không khí. 
Một giọng nói máy móc vang lên trong đầu. 
[Cung Chúc Túc Hạ Thức Tỉnh Vạn Cổ Thần Thể - Cấp Bậc: Thượng Cổ Thần Vương]. 
Thiên Vũ không hiểu chuyện gì đang xảy ra. 
Cậu thử vận chuyển sức mạnh trong cơ thể và cảm nhận nguồn năng lượng khổng lồ. 
Những vết thương trước đây cũng nhanh chóng biến mất. 
Cha mẹ cậu nghĩ rằng đó chỉ là may mắn. 
Nhưng Thiên Vũ biết mình đã bước sang một thế giới hoàn toàn khác. 
Cậu bắt đầu luyện tập mỗi ngày. 
Từ một thiếu niên bình thường, sức mạnh của cậu dần vượt xa người khác. 
Một bí mật cổ xưa đang dần được hé lộ.',
    0, 2, 870, '2024-02-18'),
(2, N'Chương 3: Cuộc Đấu Đầu Tiên',
    N'Lý Hổ lại chặn đường đòi tiền như mọi ngày. 
Hắn là người mạnh nhất trong đám thiếu niên trong làng, luôn bắt nạt những người yếu hơn. 
Trước đây Thiên Vũ chỉ có thể nhẫn nhịn vì bản thân không có sức mạnh. 
Nhưng hôm nay mọi chuyện đã khác. 
Lý Hổ cười khinh thường khi nhìn thấy Thiên Vũ đứng chắn trước mặt. 
"Một kẻ yếu như ngươi cũng dám chống lại ta sao?" 
Thiên Vũ không trả lời, chỉ lặng lẽ bước lên phía trước. 
Trong cơ thể cậu, linh khí bắt đầu vận chuyển. 
Một cú đấm đơn giản nhưng chứa sức mạnh khủng khiếp được tung ra. 
Lý Hổ bay ngược ra ba mét, ngã xuống đất bất tỉnh. 
Xung quanh im phắc, không ai dám tin vào cảnh tượng trước mắt. 
Thiên Vũ nhìn bàn tay của mình, chính cậu cũng bất ngờ với sức mạnh hiện tại. 
Từ ngày hôm đó, mọi người trong làng bắt đầu nhìn cậu bằng ánh mắt khác. 
Tin tức về một thiếu niên đánh bại Lý Hổ nhanh chóng lan truyền khắp nơi. 
Nhưng Thiên Vũ biết đây mới chỉ là bước đầu tiên trên con đường trở nên mạnh mẽ.',
    0, 3, 760, '2024-02-22'),

(2, N'Chương 4: Môn Phái Thiên Kiếm',
    N'Tin đồn về thiếu niên đánh bại Lý Hổ lan ra như lửa. 
Ba ngày sau, một lão già áo trắng xuất hiện trước cửa nhà Thiên Vũ. 
Ông có mái tóc bạc dài và ánh mắt sắc bén như nhìn thấu mọi bí mật. 
"Ta là Trưởng Lão Vân Tiêu của Thiên Kiếm Tông." 
Thiên Vũ kinh ngạc khi nghe đến cái tên này. 
Thiên Kiếm Tông là môn phái mạnh nhất trong khu vực, vô số người mơ ước được gia nhập. 
"Ta muốn thâu ngươi làm đệ tử trực truyền." 
Thiên Vũ im lặng suy nghĩ. 
Cậu biết đây là cơ hội thay đổi cả cuộc đời. 
Nhưng đồng thời cũng hiểu rằng bước vào thế giới tu luyện đồng nghĩa với việc đối mặt nguy hiểm. 
Vân Tiêu nhìn thấy sự do dự trong mắt cậu. 
"Muốn đạt được sức mạnh, ngươi phải chấp nhận thử thách." 
Cuối cùng Thiên Vũ quyết định rời khỏi ngôi làng nhỏ. 
Cậu tạm biệt cha mẹ và bắt đầu hành trình mới. 
Phía trước đang chờ đợi cậu là một thế giới rộng lớn hơn rất nhiều.',
    10, 4, 630, '2024-02-26'),

(2, N'Chương 5: Vào Tông Môn',
    N'Thiên Kiếm Tông tọa lạc trên đỉnh núi Bạch Vân, mây mù bao phủ quanh năm. 
Lần đầu tiên đặt chân lên đây, Thiên Vũ há hốc miệng nhìn những lâu đài nguy nga và các đệ tử bay lượn trên không trung. 
Đây là thế giới mà trước đây cậu chỉ có thể nghe kể trong những câu chuyện. 
Vân Tiêu dẫn cậu đi qua đại môn của tông môn. 
Hàng nghìn đệ tử đang luyện kiếm trên quảng trường rộng lớn. 
Ánh mắt của nhiều người dừng lại trên người Thiên Vũ vì tò mò. 
Một đệ tử mới như cậu không thể tránh khỏi sự nghi ngờ. 
"Chỉ là một người từ thôn nhỏ, liệu có đủ tư cách vào đây?" 
Những lời bàn tán vang lên xung quanh. 
Thiên Vũ không phản bác, chỉ âm thầm ghi nhớ. 
Cậu biết cách duy nhất để khiến người khác công nhận chính là chứng minh bằng thực lực. 
Trong đêm đầu tiên tại Thiên Kiếm Tông, Thiên Vũ ngồi tu luyện dưới ánh trăng. 
Nguồn linh khí nơi này mạnh hơn bên ngoài rất nhiều. 
Cậu cảm nhận được sức mạnh trong cơ thể đang không ngừng tăng lên. 
Con đường trở thành cường giả của Thiên Vũ chính thức bắt đầu.',
    10, 5, 600, '2024-03-01'),

-- Truyen 3: Di Gioi Chi Vuong
(3, N'Chương 1: Xuyên Không',
    N'Chu Hạo - kỹ sư lập trình 28 tuổi - chết vì làm việc quá sức. 
Khi mở mắt ra, anh thấy mình nằm giữa một cánh đồng hoang vu dưới bầu trời có hai mặt trăng. 
Không còn tiếng xe cộ, không còn ánh đèn thành phố quen thuộc. 
Xung quanh chỉ là những ngọn cỏ cao và một khu rừng xa lạ. 
Chu Hạo ngồi dậy, cố nhớ lại chuyện gì đã xảy ra. 
"Hệ thống... đây là đâu?" 
Một giọng nói máy móc đột nhiên vang lên trong đầu. 
[Hệ Thống Chuyển Sinh Siêu Cấp đã kích hoạt]. 
[Đang phân tích thế giới hiện tại]. 
Chu Hạo thở dài: "Thôi được, để tôi debug cái này..." 
Với kinh nghiệm của một lập trình viên, anh bắt đầu quan sát mọi thứ xung quanh. 
Anh nhận ra thế giới này tồn tại những thứ không thể giải thích bằng khoa học hiện đại. 
Năng lượng lạ trong không khí, những sinh vật chưa từng thấy và những bí ẩn chưa được khám phá. 
Một bảng thông tin hiện lên trước mắt. 
[Chúc mừng ký chủ nhận được kỹ năng: Phân Tích Vạn Vật]. 
Chu Hạo biết rằng muốn sống sót, anh phải nhanh chóng thích nghi với thế giới mới này.',
    0, 1, 540, '2024-01-10'),

(3, N'Chương 2: Làng Nhỏ Và Hệ Thống',
    N'May mắn tìm được một ngôi làng nhỏ trước khi trời tối. 
Người dân nơi đây sử dụng những công cụ kỳ lạ mà Chu Hạo chưa từng nhìn thấy. 
Ban đầu mọi người đều cảnh giác với người lạ xuất hiện từ khu rừng. 
Nhờ hệ thống tự động dịch ngôn ngữ, anh có thể giao tiếp với họ. 
Một ông lão trong làng đã cho anh thức ăn và chỗ nghỉ qua đêm. 
Chu Hạo bắt đầu tìm hiểu về thế giới này. 
Anh phát hiện đây là một nơi tồn tại ma pháp, quái vật và các quốc gia khác nhau. 
Hệ thống đột nhiên đưa ra nhiệm vụ đầu tiên. 
[Nhiệm vụ phụ: Giúp 5 người dân trong làng]. 
[Phần thưởng: 100 điểm kinh nghiệm]. 
Chu Hạo bật cười. 
"À, giống game quest." 
Anh bắt đầu giúp sửa chữa nhà cửa, tìm đồ thất lạc và hỗ trợ người dân. 
Mọi người dần thay đổi thái độ với anh. 
Từ một người xa lạ, Chu Hạo trở thành người được cả ngôi làng tin tưởng. 
Nhưng anh biết rằng thế giới này còn rất nhiều bí mật đang chờ mình khám phá.',
    0, 2, 410, '2024-01-14'),

-- Truyen 4: Bi An Can Phong 404
(4, N'Chương 1: Vụ Án Đầu Tiên',
    N'Điện thoại reo lúc 3 giờ sáng. 
Âm thanh vang lên giữa căn phòng yên tĩnh khiến Minh Khoa giật mình tỉnh giấc. 
"Anh ơi, em biến mất rồi." 
Giọng phụ nữ thì thào qua điện thoại rồi im bặt. 
Minh Khoa gọi lại nhiều lần nhưng không ai trả lời. 
Lần theo số điện thoại, anh tìm đến chung cư Hòa Bình. 
Tòa nhà cũ kỹ nằm giữa khu phố gần như bỏ hoang. 
Căn phòng 404 cửa hé mở như đang chờ đợi ai đó bước vào. 
Bên trong không có ai. 
Chỉ có một chiếc bàn phủ bụi và một tờ giấy viết tay. 
Trên đó chỉ có một dòng chữ: "Họ đến rồi." 
Minh Khoa cảm thấy một luồng lạnh chạy qua người. 
Anh bắt đầu kiểm tra từng góc nhỏ trong căn phòng. 
Những dấu hiệu kỳ lạ xuất hiện khiến vụ việc càng trở nên bí ẩn. 
Đây không giống một vụ mất tích bình thường. 
Và Minh Khoa biết mình vừa bước vào một vụ án nguy hiểm.',
    0, 1, 320, '2024-04-01'),

(4, N'Chương 2: Những Người Hàng Xóm',
    N'Minh Khoa bắt đầu điều tra những người sống xung quanh căn phòng 404. 
Anh gõ cửa từng phòng trên tầng 4 để tìm kiếm manh mối. 
Phòng 401: một cụ già sống một mình, gần như không nghe thấy gì. 
Phòng 402: một đôi vợ chồng trẻ luôn khóa cửa và từ chối nói chuyện. 
Phòng 403: căn phòng bỏ trống suốt hai năm nhưng vẫn có tiếng động mỗi đêm. 
Phòng 406: một người đàn ông trung niên nói rằng mình thường nghe thấy tiếng ai đó khóc. 
Mỗi người đều có một câu chuyện kỳ lạ. 
Những lời khai tưởng như không liên quan lại dần tạo thành một bức tranh đáng sợ. 
Minh Khoa phát hiện tất cả đều từng nhìn thấy một cô gái giống người mất tích. 
Nhưng điều kỳ lạ là không ai biết cô ấy chuyển đến từ khi nào. 
Camera của chung cư cũng không ghi lại hình ảnh cô gái bước vào. 
Như thể cô ấy chưa từng tồn tại. 
Minh Khoa nhìn cánh cửa phòng 404 một lần nữa. 
Anh quyết định phải tìm ra sự thật phía sau căn phòng bí ẩn này.',
    0, 2, 280, '2024-04-05'),

-- Truyen 5: Em La Anh Nang
(5, N'Chương 1: Ngày Tái Ngộ',
    N'Sân bay Tân Sơn Nhất, 2 giờ chiều. 
Hà Vy kéo vali bước nhanh giữa dòng người đông đúc. 
Cô vừa trở về sau nhiều năm học tập và làm việc ở nước ngoài. 
Mọi thứ trong thành phố đã thay đổi rất nhiều nhưng cảm giác quen thuộc vẫn còn đó. 
Trong lúc vội vàng, cô vô tình va phải một người đàn ông cao lớn. 
Chiếc vali rơi xuống đất, vài món đồ bên trong rơi ra ngoài. 
"Cô có sao không?" 
Giọng nói quen thuộc khiến Hà Vy sững người. 
Cô ngẩng đầu lên và nhìn thấy khuôn mặt mà mình đã từng rất quen thuộc. 
"Tuấn...?" 
Người đàn ông cũng bất ngờ. 
"Vy...?" 
Họ đã không gặp nhau mười năm, kể từ ngày anh theo gia đình sang Mỹ. 
Ngày đó cả hai chỉ là những đứa trẻ chưa hiểu được khoảng cách sẽ thay đổi nhiều thứ như thế nào. 
Nhưng khoảnh khắc gặp lại, những ký ức cũ lại ùa về. 
Một cuộc gặp tình cờ mở ra một câu chuyện mới giữa hai người.',
    0, 1, 430, '2024-01-20'),

(5, N'Chương 2: Kỷ Niệm Xưa',
    N'Hai người ngồi uống cà phê tại quán quen thuộc ngày xưa. 
Nơi này gần như không thay đổi sau nhiều năm. 
Chiếc bàn gỗ cũ vẫn nằm ở góc quen thuộc bên cửa sổ. 
Tuấn nhìn xung quanh rồi mỉm cười. 
"Mày còn nhớ không? Hồi lớp 5, tao vẽ con tàu ở đây rồi bị ông chủ quán đuổi." 
Vy bật cười khi nhớ lại chuyện cũ. 
Khoảng cách mười năm dường như tan biến chỉ sau vài câu nói. 
Họ kể cho nhau nghe về cuộc sống trong những năm qua. 
Tuấn nói về những ngày đầu khó khăn khi sang nước ngoài. 
Vy kể về những thay đổi của bản thân sau khi trưởng thành. 
Cả hai nhận ra dù thời gian trôi qua, một số ký ức vẫn không thể quên. 
Nhưng bên cạnh niềm vui gặp lại là những câu hỏi chưa có lời giải. 
Liệu họ có thể trở lại như trước đây? 
Hay mười năm xa cách đã tạo nên hai con người hoàn toàn khác?',
    0, 2, 390, '2024-01-24'),


-- Truyen 6: Nha So 7
(6, N'Chương 1: Căn Nhà Sáng Đèn',
    N'Ba mươi năm căn nhà số 7 không một bóng người. 
Người dân trong khu phố luôn truyền tai nhau rằng nơi đó đã bị bỏ hoang từ rất lâu. 
Không ai biết chủ nhân cũ của căn nhà đã đi đâu. 
Cánh cổng sắt hoen gỉ luôn đóng chặt, lớp bụi phủ kín mọi thứ. 
Rồi một đêm tháng Bảy, ánh đèn vàng le lói sau ô cửa sổ tầng hai. 
"Mình thấy đèn bật." 
Thảo nắm lấy tay Minh, giọng nói có chút lo lắng. 
Cả hai đứng trước căn nhà cũ và nhìn vào bên trong. 
Không hiểu vì tò mò hay một lý do nào khác, chân Minh đã bước về phía cổng sắt. 
Tiếng cánh cửa mở ra vang lên giữa đêm yên tĩnh. 
Bên trong căn nhà lạnh hơn rất nhiều so với bên ngoài. 
Những món đồ cũ vẫn còn nguyên vị trí như chưa từng có ai rời đi. 
Nhưng điều khiến họ sợ hãi là một bức ảnh mới xuất hiện trên bàn. 
Một bức ảnh chụp chính hai người họ.',
    0, 1, 210, '2024-05-01'),


-- Truyen 7: Ong Chu Kho Do
(7, N'Chương 1: Ngày Đầu Đi Làm',
    N'Thư Anh bước vào thang máy và không kịp đóng nút. 
Một người đàn ông cao lớn mặc vest đen bước vào, mang theo khí chất lạnh lùng. 
Anh ta nhìn xuống cô gái trẻ trước mặt. 
"Tân nhân viên?" 
"Vâng, em là Thư Anh ạ!" 
Người đàn ông nhìn cô từ đầu đến chân. 
"Tóc không gọn. Giày bị xước. Túi sai dress code." 
Anh ta nói từng câu ngắn gọn rồi bước ra khỏi thang máy. 
Thư Anh đứng im vài giây vì chưa hiểu chuyện gì xảy ra. 
Đến khi vào phòng làm việc, cô mới biết người vừa gặp chính là tổng giám đốc của công ty. 
Một người nổi tiếng khó tính và chưa từng hài lòng với nhân viên nào. 
Ngày đầu tiên đi làm của Thư Anh đã bắt đầu bằng một màn gặp mặt không mấy thuận lợi. 
Nhưng cô không biết rằng chính sự thẳng thắn của mình đã khiến vị tổng giám đốc kia chú ý.',
    0, 1, 680, '2024-04-15'),


-- Truyen 8: Nam 2387
(8, N'Chương 1: Ngày Trái Đất Im Lặng',
    N'Năm 2387. 
Không còn tiếng xe cộ, không còn tiếng trẻ em cười đùa trên những con phố đông người. 
Thành phố từng rực rỡ giờ chỉ còn lại những tòa nhà đổ nát. 
Kha ngồi trên mái một tòa nhà cũ, nhìn những đám mây màu vàng độc hại trôi trên bầu trời. 
Cô bé Nhi 10 tuổi đứng bên cạnh và nhìn lên khoảng không rộng lớn. 
"Anh Kha, bầu trời ngày xưa màu gì?" 
Câu hỏi khiến Kha im lặng rất lâu. 
Anh sinh ra trong một thế giới đã không còn màu xanh. 
Nhưng từ những câu chuyện của thế hệ trước, anh biết Trái Đất từng rất đẹp. 
"Màu xanh. Rất xanh, Nhi ạ." 
Nhi cố tưởng tượng một bầu trời khác với hiện tại. 
Một nơi có cây cối, biển cả và những cơn mưa tự nhiên. 
Kha nhìn về phía chân trời xa. 
Anh biết nhiệm vụ của thế hệ mình là tìm lại thứ đã mất. 
Dù con đường phía trước đầy nguy hiểm, họ vẫn phải tiếp tục.',
    0, 1, 290, '2024-03-20');
GO

-- ---------- TheoDoi ----------
-- MaDocGia: 1=Duc | 2=Hoa | 3=Hung | 4=Lan | 5=Nam | 6=Ngoc | 7=Phuc
INSERT INTO TheoDoi (MaDocGia, Matruyen, Ngaytheodoi)
VALUES
(1, 1, '2024-03-05'), (1, 2, '2024-02-20'),
(2, 1, '2024-03-02'), (2, 3, '2024-01-12'), (2, 5, '2024-01-22'),
(3, 2, '2024-02-16'), (3, 4, '2024-04-03'),
(4, 1, '2024-03-08'), (4, 7, '2024-04-17'),
(5, 2, '2024-02-20'), (5, 3, '2024-01-15'), (5, 8, '2024-03-22'),
(6, 4, '2024-04-05'), (6, 6, '2024-05-03');
GO

-- ---------- LichSuDoc ----------
-- Machuong: 1-4=Truyen1 | 5-9=Truyen2 | 10-11=Truyen3 | 12-13=Truyen4 | ...
INSERT INTO LichSuDoc (MaDocGia, Machuong, Ngaydoc)
VALUES
(1, 1,  '2024-03-05 08:30:00'),
(1, 2,  '2024-03-06 09:00:00'),
(1, 3,  '2024-03-08 21:00:00'),
(1, 5,  '2024-02-20 10:00:00'),
(1, 6,  '2024-02-22 20:30:00'),
(2, 1,  '2024-03-02 15:00:00'),
(2, 2,  '2024-03-03 16:00:00'),
(2, 10, '2024-01-12 11:00:00'),
(2, 14, '2024-01-22 14:00:00'),
(3, 5,  '2024-02-16 19:00:00'),
(3, 7,  '2024-02-22 20:00:00'),
(3, 12, '2024-04-03 22:00:00'),
(5, 5,  '2024-02-20 08:00:00'),
(5, 6,  '2024-02-22 09:00:00'),
(5, 10, '2024-01-15 14:00:00'),
(5, 18, '2024-03-22 20:00:00'),
(6, 12, '2024-04-05 21:00:00'),
(6, 16, '2024-05-03 20:00:00');
GO

-- ---------- Bookmark ----------
INSERT INTO Bookmark (MaDocGia, Matruyen, Machuong, SoDong, GhiChu, Ngaytao)
VALUES
(1, 1, 3, 47,  N'Đoạn Linh khóc hay quá',          '2024-03-08 22:00:00'),
(1, 2, 6, 112, N'Thiên Vũ thức tỉnh - đọc lại',    '2024-02-22 21:00:00'),
(2, 1, 2, 33,  N'Đoạn ký hợp đồng',                '2024-03-04 20:00:00'),
(3, 2, 7, 88,  N'Cú đấm đỉnh quá',                 '2024-02-23 21:00:00'),
(5, 8, 18, 25, N'Đoạn bầu trời xanh',              '2024-03-22 20:30:00'),
(6, 4, 12, 56, N'Tờ giấy bí ẩn',                   '2024-04-05 22:00:00');
GO

-- ---------- DanhGia ----------
INSERT INTO DanhGia (MaDocGia, Matruyen, Sosao, Nhanxet, Ngaydanhgia)
VALUES
(1, 1, 5, N'Truyện hay quá, tác giả viết rất cảm xúc. Chờ update tiếp!',              '2024-03-10'),
(1, 2, 4, N'Cốt truyện cuốn hút, nhân vật chính rất ngầu. Mong ra chương mới nhanh.', '2024-02-25'),
(2, 1, 5, N'Cặp đôi này dễ thương quá! Đọc không thể ngừng.',                         '2024-03-05'),
(2, 3, 3, N'Ổn thôi, hơi giống nhiều truyện khác nhưng đọc cũng được.',               '2024-01-20'),
(2, 5, 5, N'Thanh xuân ơi! Đọc mà nhớ lại ngày xưa quá.',                             '2024-01-28'),
(3, 2, 5, N'Đỉnh nhất hệ thống mình đọc từ trước đến nay!',                           '2024-02-28'),
(3, 4, 4, N'Không khí căng thẳng, hồi hộp từ đầu đến cuối.',                          '2024-04-10'),
(5, 2, 5, N'Thiên Vũ là nhân vật yêu thích của mình, siêu ngầu!',                     '2024-03-05'),
(5, 8, 4, N'Bối cảnh xây dựng tốt, nội dung sâu sắc. Rất đáng đọc.',                  '2024-04-01'),
(6, 4, 5, N'Ai viết hay vậy? Đọc xong muốn tiếp ngay!',                               '2024-04-15');
GO

-- ---------- BinhLuan ----------
INSERT INTO BinhLuan (MaDocGia, Matruyen, Noidung, TrangThai, Ngaybinhluan)
VALUES
(1, 1, N'Chương 3 hay quá tác giả ơi! Update nhanh lên nhé!!',                       1, '2024-03-08 22:00:00'),
(2, 1, N'Team Linh x Minh Khải!!! Đẹp đôi quá đi!!!',                                1, '2024-03-04 20:00:00'),
(3, 2, N'Thiên Vũ đánh Lý Hổ đoạn đó đỉnh thật sự. Hồi hộp cả buổi chiều.',         1, '2024-02-23 21:30:00'),
(1, 2, N'Mong tác giả ra thêm chương mới, đang hay tới đây mà ngừng thì tiếc quá.',  1, '2024-03-01 08:00:00'),
(5, 4, N'Phòng 402 vợ chồng trẻ không mở cửa - chắc chắn có bí ẩn ở đây!',          1, '2024-04-07 23:00:00'),
(6, 4, N'Tác giả ơi update đi, không ngủ được vì đang hồi hộp!',                    1, '2024-04-08 01:00:00'),
(4, 7, N'Haha sếp khó đỡ thật sự, Thư Anh nhịn được là giỏi đó.',                   1, '2024-04-18 19:00:00'),
(2, 5, N'Đọc mà nhớ lại crush hồi cấp 1. Sao thanh xuân qua nhanh quá.',            1, '2024-01-25 22:00:00'),
(3, 2, N'Spam quảng cáo giày dép giá rẻ!!!',                                          0, '2024-03-10 10:00:00'); -- bi an vi spam
GO

-- ---------- NapXu ----------
INSERT INTO NapXu (MaDocGia, Soxunhan, Sotien, Phuongthuc, Trangthai, Ngaynap)
VALUES
(1, 100,  20000,  'momo',    N'thanhcong', '2024-02-20 09:00:00'),
(1, 200,  40000,  'vnpay',   N'thanhcong', '2024-03-01 10:00:00'),
(2, 500,  100000, 'momo',    N'thanhcong', '2024-02-10 14:00:00'),
(2, 200,  40000,  'banking', N'thanhcong', '2024-03-05 15:00:00'),
(3, 50,   10000,  'momo',    N'thanhcong', '2024-03-02 08:00:00'),
(5, 100,  20000,  'vnpay',   N'thanhcong', '2024-02-18 11:00:00'),
(5, 200,  40000,  'momo',    N'thanhcong', '2024-03-10 16:00:00'),
(6, 100,  20000,  'banking', N'thanhcong', '2024-04-01 09:00:00'),
(1, 50,   10000,  'momo',    N'thatbai',   '2024-04-15 10:00:00'),
(4, 200,  40000,  'vnpay',   N'thanhcong', '2024-04-20 13:00:00');
GO

-- ---------- DoanhThu ----------
INSERT INTO DoanhThu (Thang, Nam, Tongtien, Tongxunap, LuotNap)
VALUES
(1, 2024,  150000,  750,  12),
(2, 2024,  210000, 1050,  18),
(3, 2024,  320000, 1600,  27),
(4, 2024,  180000,  900,  15),
(5, 2024,   95000,  475,   8);
GO

-- ---------- HoTro ----------
INSERT INTO HoTro (MaDocGia, Manv, TieuDe, NoiDung, PhanHoi, TrangThai, NgayGui, NgayXuLy)
VALUES
(1, 2,
    N'Không đọc được chương đã mua',
    N'Em đã mua chương 3 truyện Cạm Bẫy Ngọt Ngào nhưng vào đọc thì báo lỗi.',
    N'Chúng tôi đã kiểm tra và khôi phục quyền đọc cho bạn. Xin lỗi vì sự bất tiện.',
    2, '2024-03-09 09:00:00', '2024-03-09 11:00:00'),

(3, 2,
    N'Nạp xu không vào tài khoản',
    N'Em nạp 10k qua momo lúc sáng nhưng xu chưa vào. Mã giao dịch: MM202403020001',
    N'Chúng tôi đã xác nhận và cộng 50 xu vào tài khoản của bạn.',
    2, '2024-03-02 10:00:00', '2024-03-02 14:00:00'),

(4, NULL,
    N'Báo cáo bình luận vi phạm',
    N'Có người đang spam quảng cáo trong phần bình luận truyện số 7.',
    NULL,
    0, '2024-05-05 20:00:00', NULL),

(6, 3,
    N'Góp ý về tính năng tìm kiếm',
    N'Tính năng tìm kiếm theo tên tác giả không hoạt động chính xác.',
    N'Cảm ơn góp ý! Chúng tôi đã ghi nhận và sẽ cải thiện trong phiên bản tới.',
    2, '2024-04-10 15:00:00', '2024-04-11 09:00:00'),

(2, NULL,
    N'Muốn đề xuất thêm thể loại',
    N'Mình muốn đề xuất thêm thể loại Truyện Ma riêng biệt vì hiện tại gộp chung với Kinh Dị hơi khó lọc.',
    NULL,
    1, '2024-05-10 08:00:00', NULL);
GO


-- ============================================================
--  INDEXES
-- ============================================================
CREATE INDEX IX_TaiKhoan_MaVaiTro     ON TaiKhoan(MaVaiTro);
CREATE INDEX IX_DocGia_Matk           ON DocGia(Matk);
CREATE INDEX IX_NhanVien_Matk         ON NhanVien(Matk);
CREATE INDEX IX_Truyen_Matheloai      ON Truyen(Matheloai);
CREATE INDEX IX_Truyen_Manv           ON Truyen(Manv);
CREATE INDEX IX_Truyen_LuotDoc        ON Truyen(LuotDoc DESC);
CREATE INDEX IX_Chuong_Matruyen       ON Chuong(Matruyen);
CREATE INDEX IX_TheoDoi_MaDocGia      ON TheoDoi(MaDocGia);
CREATE INDEX IX_LichSuDoc_MaDocGia    ON LichSuDoc(MaDocGia);
CREATE INDEX IX_Bookmark_MaDocGia     ON Bookmark(MaDocGia);
CREATE INDEX IX_Bookmark_Machuong     ON Bookmark(Machuong);
CREATE INDEX IX_BinhLuan_Matruyen     ON BinhLuan(Matruyen);
CREATE INDEX IX_DanhGia_Matruyen      ON DanhGia(Matruyen);
CREATE INDEX IX_NapXu_MaDocGia        ON NapXu(MaDocGia);
CREATE INDEX IX_HoTro_MaDocGia        ON HoTro(MaDocGia);
GO


-- ============================================================
--  KIEM TRA DU LIEU
-- ============================================================
SELECT 'VaiTro'     AS Bang, COUNT(*) AS SoBanGhi FROM VaiTro     UNION ALL
SELECT 'TaiKhoan'   AS Bang, COUNT(*) AS SoBanGhi FROM TaiKhoan   UNION ALL
SELECT 'DocGia'     AS Bang, COUNT(*) AS SoBanGhi FROM DocGia      UNION ALL
SELECT 'NhanVien'   AS Bang, COUNT(*) AS SoBanGhi FROM NhanVien    UNION ALL
SELECT 'TheoLoai'   AS Bang, COUNT(*) AS SoBanGhi FROM TheoLoai    UNION ALL
SELECT 'Truyen'     AS Bang, COUNT(*) AS SoBanGhi FROM Truyen      UNION ALL
SELECT 'Chuong'     AS Bang, COUNT(*) AS SoBanGhi FROM Chuong      UNION ALL
SELECT 'TheoDoi'    AS Bang, COUNT(*) AS SoBanGhi FROM TheoDoi     UNION ALL
SELECT 'LichSuDoc'  AS Bang, COUNT(*) AS SoBanGhi FROM LichSuDoc   UNION ALL
SELECT 'Bookmark'   AS Bang, COUNT(*) AS SoBanGhi FROM Bookmark    UNION ALL
SELECT 'DanhGia'    AS Bang, COUNT(*) AS SoBanGhi FROM DanhGia     UNION ALL
SELECT 'BinhLuan'   AS Bang, COUNT(*) AS SoBanGhi FROM BinhLuan    UNION ALL
SELECT 'NapXu'      AS Bang, COUNT(*) AS SoBanGhi FROM NapXu       UNION ALL
SELECT 'DoanhThu'   AS Bang, COUNT(*) AS SoBanGhi FROM DoanhThu    UNION ALL
SELECT 'HoTro'      AS Bang, COUNT(*) AS SoBanGhi FROM HoTro;
GO

-- ADMIN (mật khẩu: Admin@123)
UPDATE TaiKhoan SET Matkhau = '$2a$11$qjrhdnicY0D93PSJ.ziOxeESOfQJ0owwlX2simwUHXxidvmdovrG6' WHERE Email = 'danglntb02258@gmail.com';

-- NHÂN VIÊN (mật khẩu: Nhanvien@123)
UPDATE TaiKhoan SET Matkhau = '$2a$11$YQh2KynpvwoucWCZWr05leNdwm/5sOw5X0pLYLHO6YgtM.KnmmM7K' WHERE Email = 'binh1.nv@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$YQh2KynpvwoucWCZWr05leNdwm/5sOw5X0pLYLHO6YgtM.KnmmM7K' WHERE Email = 'cam2.nv@gmail.com';

-- ĐỘC GIẢ (mật khẩu: Docgia@123)
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'duc1.lm@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'hoa2.pt@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'hung3.nq@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'lan4.vt@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'nam5.dh@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'ngoc6.bt@gmail.com';
UPDATE TaiKhoan SET Matkhau = '$2a$11$ABSZRY0N10wLMZncres1aul3LdOJ8bX/FKDWFau/08vPyS4H6NBI.' WHERE Email = 'phuc7.hv@gmail.com';