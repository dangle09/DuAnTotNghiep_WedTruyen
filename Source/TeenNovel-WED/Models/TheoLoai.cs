using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("TheoLoai")]
[Index("Tentheloa", Name = "UQ__TheoLoai__1D69C5C26DCB177E", IsUnique = true)]
public partial class TheoLoai
{
    [Key]
    public int Matheloai { get; set; }

    [StringLength(100)]
    public string Tentheloa { get; set; } = null!;

    [StringLength(500)]
    public string? Mota { get; set; }

    [InverseProperty("MatheloaiNavigation")]
    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
