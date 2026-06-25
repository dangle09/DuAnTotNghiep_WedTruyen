using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WED_DOCTRUYEN.Models;

[Table("Truyen")]
[Index("Matheloai", Name = "IX_Truyen_Matheloai")]
[Index("Matk", Name = "IX_Truyen_Matk")]
public partial class Truyen
{
    [Key]
    public int Matruyen { get; set; }

    public int Matk { get; set; }

    public int Matheloai { get; set; }

    [StringLength(200)]
    public string Tentruyen { get; set; } = null!;

    [StringLength(2000)]
    [Unicode(false)]
    public string? Mota { get; set; }

    [StringLength(100)]
    public string? Tacgia { get; set; }

    [StringLength(30)]
    public string Trangthai { get; set; } = null!;

    [StringLength(200)]
    public string? Theloai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaydang { get; set; }

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<Binhluan> Binhluans { get; set; } = new List<Binhluan>();

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<Chuong> Chuongs { get; set; } = new List<Chuong>();

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<Danhgium> Danhgia { get; set; } = new List<Danhgium>();

    [ForeignKey("Matheloai")]
    [InverseProperty("Truyens")]
    public virtual Theoloai MatheloaiNavigation { get; set; } = null!;

    [ForeignKey("Matk")]
    [InverseProperty("Truyens")]
    public virtual Taikhoan MatkNavigation { get; set; } = null!;

    [InverseProperty("MatruyenNavigation")]
    public virtual ICollection<Theodoi> Theodois { get; set; } = new List<Theodoi>();
}
