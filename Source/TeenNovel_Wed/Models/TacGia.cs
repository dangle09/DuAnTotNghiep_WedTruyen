using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("TacGia")]
public partial class TacGia
{
    [Key]
    public int MaTacGia { get; set; }

    [StringLength(100)]
    public string TenTacGia { get; set; } = null!;

    [StringLength(100)]
    public string? ButDanh { get; set; }

    [StringLength(100)]
    public string? QuocGia { get; set; }

    [StringLength(1000)]
    public string? TieuSu { get; set; }

    [StringLength(500)]
    public string? AnhDaiDien { get; set; }

    [InverseProperty("MaTacGiaNavigation")]
    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
