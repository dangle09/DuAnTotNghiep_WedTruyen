using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WED_DOCTRUYEN.Models;

[Table("Theoloai")]
[Index("Tentheloa", Name = "UQ__Theoloai__1D69C5C219284D19", IsUnique = true)]
public partial class Theoloai
{
    [Key]
    public int Matheloai { get; set; }

    [StringLength(500)]
    public string? Mota { get; set; }

    [StringLength(100)]
    public string Tentheloa { get; set; } = null!;

    [InverseProperty("MatheloaiNavigation")]
    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
