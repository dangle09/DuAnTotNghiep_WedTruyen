using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("Truyen")]
[Index("LuotDoc", Name = "IX_Truyen_LuotDoc", AllDescending = true)]
[Index("Manv", Name = "IX_Truyen_Manv")]
[Index("Matheloai", Name = "IX_Truyen_Matheloai")]
public partial class Truyen
{
    [Key]
    public int Matruyen { get; set; }

    public int Manv { get; set; }

    public int Matheloai { get; set; }

    [StringLength(200)]
    public string Tentruyen { get; set; } = null!;

    [StringLength(2000)]
    public string? Mota { get; set; }

    [StringLength(100)]
    public string? Tacgia { get; set; }

    [StringLength(500)]
    public string? AnhBia { get; set; }

    [StringLength(200)]
    public string? Theloai { get; set; }

    public int LuotDoc { get; set; }

    public int LuotThich { get; set; }

    [StringLength(30)]
    public string Trangthai { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Ngaydang { get; set; }

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<Chuong> Chuongs { get; set; } = new List<Chuong>();

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

    [ForeignKey("Manv")]
    [InverseProperty("Truyens")]
    public virtual NhanVien ManvNavigation { get; set; } = null!;

    [ForeignKey("Matheloai")]
    [InverseProperty("Truyens")]
    public virtual TheLoai MatheloaiNavigation { get; set; } = null!;

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<TheoDoi> TheoDois { get; set; } = new List<TheoDoi>();
}
