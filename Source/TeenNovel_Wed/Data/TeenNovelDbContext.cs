using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TeenNovel_Wed.Models;

namespace TeenNovel_Wed.Data;

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

    public virtual DbSet<DanhGia> DanhGias { get; set; }

    public virtual DbSet<DocGia> DocGias { get; set; }

    public virtual DbSet<HoTro> HoTros { get; set; }

    public virtual DbSet<LichSuDoc> LichSuDocs { get; set; }

    public virtual DbSet<NapXu> NapXus { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<SuDungXu> SuDungXus { get; set; }

    public virtual DbSet<TacGia> TacGias { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<TheoDoi> TheoDois { get; set; }

    public virtual DbSet<Truyen> Truyens { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<GoiNapXu> GoiNapXus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-6FR8KT4\\SQLEXPRESS;Database=WebTruyen_TeenNovel;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.Mabinhluan).HasName("PK__BinhLuan__CB94A9F876B068BD");

            entity.Property(e => e.Ngaybinhluan).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.BinhLuans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BinhLuan__MaDocG__797309D9");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.BinhLuans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BinhLuan__Matruy__7A672E12");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.MaBookmark).HasName("PK__Bookmark__76B33EDC84A35C7D");

            entity.Property(e => e.Ngaytao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.Bookmarks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmark__MaDocG__6C190EBB");

            entity.HasOne(d => d.MachuongNavigation).WithMany(p => p.Bookmarks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmark__Machuo__6E01572D");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Bookmarks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmark__Matruy__6D0D32F4");
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.HasKey(e => e.Machuong).HasName("PK__Chuong__37E67D4AAB243834");

            entity.Property(e => e.Ngaydang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Thutu).HasDefaultValue(1);

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Chuongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chuong__Matruyen__5DCAEF64");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.Madanhgia).HasName("PK__DanhGia__5FA2512DA82903D3");

            entity.Property(e => e.Ngaydanhgia).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.DanhGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaDocGi__73BA3083");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.DanhGias)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__Matruye__74AE54BC");
        });

        modelBuilder.Entity<DocGia>(entity =>
        {
            entity.HasKey(e => e.MaDocGia).HasName("PK__DocGia__F165F945D9CB87BB");

            entity.Property(e => e.Ngaytao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MatkNavigation).WithOne(p => p.DocGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DocGia__Matk__4222D4EF");
        });

        modelBuilder.Entity<HoTro>(entity =>
        {
            entity.HasKey(e => e.MaHoTro).HasName("PK__HoTro__13DBD49DFAB95202");

            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.HoTros)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoTro__MaDocGia__04E4BC85");

            entity.HasOne(d => d.ManvNavigation).WithMany(p => p.HoTros).HasConstraintName("FK__HoTro__Manv__05D8E0BE");
        });

        modelBuilder.Entity<LichSuDoc>(entity =>
        {
            entity.HasKey(e => e.Malichsu).HasName("PK__LichSuDo__620F0ABA95715D8F");

            entity.Property(e => e.Ngaydoc).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.LichSuDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDoc__MaDoc__6754599E");

            entity.HasOne(d => d.MachuongNavigation).WithMany(p => p.LichSuDocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDoc__Machu__68487DD7");
        });

        modelBuilder.Entity<NapXu>(entity =>
        {
            entity.HasKey(e => e.Manap).HasName("PK__NapXu__339AA9378FD1D10B");

            entity.Property(e => e.Ngaynap).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Phuongthuc).HasDefaultValue("momo");
            entity.Property(e => e.Trangthai).HasDefaultValue("thành công");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.NapXus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NapXu__MaDocGia__00200768");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.Manv).HasName("PK__NhanVien__2724CB0255B50B92");

            entity.Property(e => e.Ngayvaolamm).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MatkNavigation).WithOne(p => p.NhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__Matk__46E78A0C");
        });

        modelBuilder.Entity<SuDungXu>(entity =>
        {
            entity.HasKey(e => e.MaSuDungXu).HasName("PK__SuDungXu__8ABD60C8D04AA72A");

            entity.Property(e => e.NgaySuDung).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.SuDungXus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuDungXu__MaDocG__0A9D95DB");

            entity.HasOne(d => d.MachuongNavigation).WithMany(p => p.SuDungXus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuDungXu__Machuo__0B91BA14");
        });

        modelBuilder.Entity<TacGia>(entity =>
        {
            entity.HasKey(e => e.MaTacGia).HasName("PK__TacGia__F24E6756108A779A");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Matk).HasName("PK__TaiKhoan__272404384FD08895");

            entity.Property(e => e.Trangthai).HasDefaultValue(true);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiKhoan__MaVaiT__3C69FB99");
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.Matheloai).HasName("PK__TheLoai__8E2559304293B607");
        });

        modelBuilder.Entity<TheoDoi>(entity =>
        {
            entity.HasKey(e => e.Matheodoi).HasName("PK__TheoDoi__AA89480D0EF39542");

            entity.Property(e => e.Ngaytheodoi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MaDocGiaNavigation).WithMany(p => p.TheoDois)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheoDoi__MaDocGi__628FA481");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.TheoDois)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheoDoi__Matruye__6383C8BA");
        });

        modelBuilder.Entity<Truyen>(entity =>
        {
            entity.HasKey(e => e.Matruyen).HasName("PK__Truyen__98706765FC1987B4");

            entity.Property(e => e.Ngaydang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Trangthai).HasDefaultValue("Đang ra");

            entity.HasOne(d => d.MaTacGiaNavigation).WithMany(p => p.Truyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Truyen__MaTacGia__534D60F1");

            entity.HasOne(d => d.ManvNavigation).WithMany(p => p.Truyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Truyen__Manv__52593CB8");

            entity.HasMany(d => d.MaTheLoais).WithMany(p => p.MaTruyens)
                .UsingEntity<Dictionary<string, object>>(
                    "TruyenTheLoai",
                    r => r.HasOne<TheLoai>().WithMany()
                        .HasForeignKey("MaTheLoai")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Truyen_Th__MaThe__571DF1D5"),
                    l => l.HasOne<Truyen>().WithMany()
                        .HasForeignKey("MaTruyen")
                        .HasConstraintName("FK__Truyen_Th__MaTru__5629CD9C"),
                    j =>
                    {
                        j.HasKey("MaTruyen", "MaTheLoai").HasName("PK__Truyen_T__D7A1F57FA56A644F");
                        j.ToTable("Truyen_TheLoai");
                    });
        });

        modelBuilder.Entity<GoiNapXu>(entity =>
        {
            entity.HasKey(e => e.MaGoiNap);

            entity.ToTable("GoiNapXu");

            entity.Property(e => e.TenGoi)
                .HasMaxLength(100);

            entity.Property(e => e.SoTien)
                .HasColumnType("decimal(12,2)");

            entity.Property(e => e.Anh)
                .HasMaxLength(500);

            entity.Property(e => e.KhuyenMai)
                .HasMaxLength(100);

            entity.Property(e => e.HienThi)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF3E35D75D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
