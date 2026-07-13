using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("TaiKhoan")]
[Index("MaVaiTro", Name = "IX_TaiKhoan_MaVaiTro")]
[Index("Tendangnhap", Name = "UQ__TaiKhoan__68789B1D083255E4", IsUnique = true)]
[Index("Email", Name = "UQ__TaiKhoan__A9D10534D7CC4B99", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    public int Matk { get; set; }

    public int MaVaiTro { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Matkhau { get; set; } = null!;

    [StringLength(50)]
    public string Tendangnhap { get; set; } = null!;

    public bool Trangthai { get; set; }

    [InverseProperty("MatkNavigation")]
    public virtual DocGia? DocGia { get; set; }

    [ForeignKey("MaVaiTro")]
    [InverseProperty("TaiKhoans")]
    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    [InverseProperty("MatkNavigation")]
    public virtual NhanVien? NhanVien { get; set; }
}
