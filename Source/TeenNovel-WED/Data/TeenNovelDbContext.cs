using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TeenNovel_WED.Models;

namespace TeenNovel_WED.Data;

public partial class TeenNovelDbContext : DbContext
{
    public TeenNovelDbContext()
    {
    }

    public TeenNovelDbContext(DbContextOptions<TeenNovelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BinhLuan> BinhLuans { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Chuong> Chuongs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DoanhThu> DoanhThus { get; set; }

    public virtual DbSet<DocGium> DocGia { get; set; }

    public virtual DbSet<HoTro> HoTros { get; set; }

    public virtual DbSet<LichSuDoc> LichSuDocs { get; set; }

    public virtual DbSet<NapXu> NapXus { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TheoDoi> TheoDois { get; set; }

    public virtual DbSet<TheoLoai> TheoLoais { get; set; }

    public virtual DbSet<Truyen> Truyens { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-6FR8KT4\\SQLEXPRESS;Database=WebTruyen;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.Mabinhluan).HasName("PK__BinhLuan__CB94A9F8D435C2CB");

            entity.Property(e => e.Ngaybinhluan).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.BinhLuans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BinhLuan__MaDocG__628FA481");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.BinhLuans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BinhLuan__Matruy__6383C8BA");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.MaBookmark).HasName("PK__Bookmark__76B33EDC23549B40");

            entity.Property(e => e.Ngaytao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.Bookmarks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmark__MaDocG__5535A963");

            entity.HasOne(d => d.MachuongNavigation).WithMany(p => p.Bookmarks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmark__Machuo__571DF1D5");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Bookmarks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmark__Matruy__5629CD9C");
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.HasKey(e => e.Machuong).HasName("PK__Chuong__37E67D4AD35BDF51");

            entity.Property(e => e.Ngaydang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Thutu).HasDefaultValue(1);

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Chuongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chuong__Matruyen__46E78A0C");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.Madanhgia).HasName("PK__DanhGia__5FA2512DC78D2AF6");

            entity.Property(e => e.Ngaydanhgia).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.DanhGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaDocGi__5CD6CB2B");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.DanhGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__Matruye__5DCAEF64");
        });

        modelBuilder.Entity<DoanhThu>(entity =>
        {
            entity.HasKey(e => e.Madoanhthu).HasName("PK__DoanhThu__FE27492938DFE3AB");
        });

        modelBuilder.Entity<DocGium>(entity =>
        {
            entity.HasKey(e => e.MaDocGia).HasName("PK__DocGia__F165F94585318669");

            entity.Property(e => e.Ngaytao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MatkNavigation).WithOne(p => p.DocGium)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DocGia__Matk__300424B4");
        });

        modelBuilder.Entity<HoTro>(entity =>
        {
            entity.HasKey(e => e.MaHoTro).HasName("PK__HoTro__13DBD49D2FB6BB1A");

            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.HoTros)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoTro__MaDocGia__73BA3083");

            entity.HasOne(d => d.ManvNavigation).WithMany(p => p.HoTros).HasConstraintName("FK__HoTro__Manv__74AE54BC");
        });

        modelBuilder.Entity<LichSuDoc>(entity =>
        {
            entity.HasKey(e => e.Malichsu).HasName("PK__LichSuDo__620F0ABAC57DAEA4");

            entity.Property(e => e.Ngaydoc).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.LichSuDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDoc__MaDoc__5070F446");

            entity.HasOne(d => d.MachuongNavigation).WithMany(p => p.LichSuDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDoc__Machu__5165187F");
        });

        modelBuilder.Entity<NapXu>(entity =>
        {
            entity.HasKey(e => e.Manap).HasName("PK__NapXu__339AA937474A2C4B");

            entity.Property(e => e.Ngaynap).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Phuongthuc).HasDefaultValue("momo");
            entity.Property(e => e.Trangthai).HasDefaultValue("thanhcong");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.NapXus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NapXu__MaDocGia__693CA210");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.Manv).HasName("PK__NhanVien__2724CB028F98A40F");

            entity.Property(e => e.Ngayvaolamm).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Trangthai).HasDefaultValue("hoatdong");

            entity.HasOne(d => d.MatkNavigation).WithOne(p => p.NhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__Matk__35BCFE0A");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Matk).HasName("PK__TaiKhoan__272404386E4CD8F3");

            entity.Property(e => e.Trangthai).HasDefaultValue("hoatdong");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiKhoan__MaVaiT__2A4B4B5E");
        });

        modelBuilder.Entity<TheoDoi>(entity =>
        {
            entity.HasKey(e => e.Matheodoi).HasName("PK__TheoDoi__AA89480DB297A179");

            entity.Property(e => e.Ngaytheodoi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.TheoDois)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheoDoi__MaDocGi__4BAC3F29");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.TheoDois)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheoDoi__Matruye__4CA06362");
        });

        modelBuilder.Entity<TheoLoai>(entity =>
        {
            entity.HasKey(e => e.Matheloai).HasName("PK__TheoLoai__8E255930C1AA8552");
        });

        modelBuilder.Entity<Truyen>(entity =>
        {
            entity.HasKey(e => e.Matruyen).HasName("PK__Truyen__987067655F4F35DD");

            entity.Property(e => e.Ngaydang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Trangthai).HasDefaultValue("Đang ra");

            entity.HasOne(d => d.ManvNavigation).WithMany(p => p.Truyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Truyen__Manv__3F466844");

            entity.HasOne(d => d.MatheloaiNavigation).WithMany(p => p.Truyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Truyen__Matheloa__403A8C7D");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF6FF9233B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
