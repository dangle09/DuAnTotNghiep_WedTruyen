using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("NapXu")]
[Index("MaDocGia", Name = "IX_NapXu_MaDocGia")]
[Index("MaGoiNap", Name = "IX_NapXu_MaGoiNap")]
public partial class NapXu
{
    [Key]
    public int Manap { get; set; }

    public int MaDocGia { get; set; }

    public int? MaGoiNap { get; set; }

    public int Soxunhan { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Sotien { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Phuongthuc { get; set; } = "VietQR";

    [StringLength(100)]
    public string? NoiDungChuyenKhoan { get; set; }

    [StringLength(100)]
    public string? MaGiaoDich { get; set; }

    [StringLength(20)]
    public string Trangthai { get; set; } = "ChoThanhToan";

    [Column(TypeName = "datetime")]
    public DateTime? Ngaynap { get; set; }

    [ForeignKey(nameof(MaDocGia))]
    [InverseProperty(nameof(DocGia.NapXus))]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey(nameof(MaGoiNap))]
    [InverseProperty(nameof(GoiNapXu.NapXus))]
    public virtual GoiNapXu? MaGoiNapNavigation { get; set; }
}