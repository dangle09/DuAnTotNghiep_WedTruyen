using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("HoTro")]
[Index("MaDocGia", Name = "IX_HoTro_MaDocGia")]
public partial class HoTro
{
    [Key]
    public int MaHoTro { get; set; }

    public int MaDocGia { get; set; }

    public int? Manv { get; set; }

    [StringLength(200)]
    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string? PhanHoi { get; set; }

    public byte TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime NgayGui { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayXuLy { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("HoTros")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Manv")]
    [InverseProperty("HoTros")]
    public virtual NhanVien? ManvNavigation { get; set; }
}
