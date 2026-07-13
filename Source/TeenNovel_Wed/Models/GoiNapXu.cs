using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("GoiNapXu")]
public partial class GoiNapXu
{
    [Key]
    public int MaGoiNap { get; set; }

    [StringLength(100)]
    public string TenGoi { get; set; } = null!;

    [Column(TypeName = "decimal(12,2)")]
    public decimal SoTien { get; set; }

    public int SoXuNhan { get; set; }

    [StringLength(500)]
    public string? Anh { get; set; }

    [StringLength(100)]
    public string? KhuyenMai { get; set; }

    public bool HienThi { get; set; }
}