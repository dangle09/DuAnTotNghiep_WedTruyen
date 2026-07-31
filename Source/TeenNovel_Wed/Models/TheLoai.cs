using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("TheLoai")]
[Index("Tentheloai", Name = "UQ__TheLoai__1FA8FFDB62BD986C", IsUnique = true)]
public partial class TheLoai
{
    [Key]
    public int Matheloai { get; set; }

    [StringLength(100)]
    public string Tentheloai { get; set; } = null!;

    [StringLength(500)]
    public string? Mota { get; set; }

    [ForeignKey("MaTheLoai")]
    [InverseProperty("MaTheLoais")]
    public virtual ICollection<Truyen> MaTruyens { get; set; } = new List<Truyen>();
}
