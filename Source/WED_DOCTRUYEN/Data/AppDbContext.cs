using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WED_DOCTRUYEN.Models;

namespace WED_DOCTRUYEN.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Binhluan> Binhluans { get; set; }

    public virtual DbSet<Chuong> Chuongs { get; set; }

    public virtual DbSet<Danhgium> Danhgia { get; set; }

    public virtual DbSet<Doanhthu> Doanhthus { get; set; }

    public virtual DbSet<HoTro> HoTros { get; set; }

    public virtual DbSet<Lichsudoc> Lichsudocs { get; set; }

    public virtual DbSet<Napxu> Napxus { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    public virtual DbSet<Taikhoan> Taikhoans { get; set; }

    public virtual DbSet<Theodoi> Theodois { get; set; }

    public virtual DbSet<Theoloai> Theoloais { get; set; }

    public virtual DbSet<Truyen> Truyens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-6FR8KT4\\SQLEXPRESS;Database=WebTruyen;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Binhluan>(entity =>
        {
            entity.HasKey(e => e.Mabinhluan).HasName("PK__Binhluan__CB94A9F861B88F8F");

            entity.Property(e => e.Ngaybinhluan).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.Binhluans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Binhluan__Matk__656C112C");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Binhluans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Binhluan__Matruy__66603565");
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.HasKey(e => e.Machuong).HasName("PK__Chuong__37E67D4AACA12D1A");

            entity.Property(e => e.Ngaydang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Thutu).HasDefaultValue(1);

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Chuongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chuong__Matruyen__5070F446");
        });

        modelBuilder.Entity<Danhgium>(entity =>
        {
            entity.HasKey(e => e.Madanhgia).HasName("PK__Danhgia__5FA2512D8942D511");

            entity.Property(e => e.Ngaydanhgia).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.Danhgia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Danhgia__Matk__60A75C0F");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Danhgia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Danhgia__Matruye__619B8048");
        });

        modelBuilder.Entity<Doanhthu>(entity =>
        {
            entity.HasKey(e => e.Madoanhthu).HasName("PK__Doanhthu__FE274929D1B554E1");
        });

        modelBuilder.Entity<HoTro>(entity =>
        {
            entity.HasKey(e => e.MaHoTro).HasName("PK__HoTro__13DBD49D70EF4D4C");

            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ManvNavigation).WithMany(p => p.HoTros).HasConstraintName("FK__HoTro__Manv__76969D2E");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.HoTros)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoTro__Matk__75A278F5");
        });

        modelBuilder.Entity<Lichsudoc>(entity =>
        {
            entity.HasKey(e => e.Malichsu).HasName("PK__Lichsudo__620F0ABAAD66F79D");

            entity.Property(e => e.Ngaydoc).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Vitridoc).HasDefaultValue(1);

            entity.HasOne(d => d.MachuongNavigation).WithMany(p => p.Lichsudocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lichsudoc__Machu__5BE2A6F2");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.Lichsudocs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lichsudoc__Matk__5AEE82B9");
        });

        modelBuilder.Entity<Napxu>(entity =>
        {
            entity.HasKey(e => e.Manap).HasName("PK__Napxu__339AA937C95C4F0E");

            entity.Property(e => e.Ngaynap).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Phuongthuc).HasDefaultValue("momo");
            entity.Property(e => e.Trangthai).HasDefaultValue("thanhcong");

            entity.HasOne(d => d.MataikhoanNavigation).WithMany(p => p.Napxus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Napxu__Mataikhoa__6C190EBB");
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.HasKey(e => e.Manv).HasName("PK__Nhanvien__2724CB02859F221A");

            entity.Property(e => e.Ngayvaolamm).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Trangthai).HasDefaultValue("hoatdong");
            entity.Property(e => e.Valtro).HasDefaultValue("nhanvien");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.Nhanviens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nhanvien__Matk__4222D4EF");
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.Matk).HasName("PK__Taikhoan__2724043857BC2FEA");

            entity.Property(e => e.Ngaytao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Trangthai).HasDefaultValue("hoatdong");
            entity.Property(e => e.Valtro).HasDefaultValue("docgia");
        });

        modelBuilder.Entity<Theodoi>(entity =>
        {
            entity.HasKey(e => e.Matheodoi).HasName("PK__Theodoi__AA89480D4E3BAECD");

            entity.Property(e => e.Ngaytheodoi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.Theodois)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Theodoi__Matk__5535A963");

            entity.HasOne(d => d.MatruyenNavigation).WithMany(p => p.Theodois)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Theodoi__Matruye__5629CD9C");
        });

        modelBuilder.Entity<Theoloai>(entity =>
        {
            entity.HasKey(e => e.Matheloai).HasName("PK__Theoloai__8E255930A2D1551E");
        });

        modelBuilder.Entity<Truyen>(entity =>
        {
            entity.HasKey(e => e.Matruyen).HasName("PK__Truyen__9870676517561FA7");

            entity.Property(e => e.Ngaydang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Trangthai).HasDefaultValue("dangtien");

            entity.HasOne(d => d.MatheloaiNavigation).WithMany(p => p.Truyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Truyen__Matheloa__4AB81AF0");

            entity.HasOne(d => d.MatkNavigation).WithMany(p => p.Truyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Truyen__Matk__49C3F6B7");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
