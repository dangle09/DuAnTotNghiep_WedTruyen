using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("NhanVien")]
[Index("Matk", Name = "IX_NhanVien_Matk")]
[Index("Matk", Name = "UQ__NhanVien__27240439F8382A8C", IsUnique = true)]
public partial class NhanVien
{
    [Key]
    public int Manv { get; set; }

    public int Matk { get; set; }

    [StringLength(100)]
    public string Ten { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? Sodienthoai { get; set; }

    [StringLength(10)]
    public string? Gioitinh { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Ngaysinh { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngayvaolamm { get; set; }

    [StringLength(20)]
    public string Trangthai { get; set; } = null!;

    [InverseProperty("ManvNavigation")]
    public virtual ICollection<HoTro> HoTros { get; set; } = new List<HoTro>();

    [ForeignKey("Matk")]
    [InverseProperty("NhanVien")]
    public virtual TaiKhoan MatkNavigation { get; set; } = null!;

    [InverseProperty("ManvNavigation")]
    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
