using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WED_DOCTRUYEN.Models;

[Table("Binhluan")]
[Index("Matruyen", Name = "IX_Binhluan_Matruyen")]
public partial class Binhluan
{
    [Key]
    public int Mabinhluan { get; set; }

    public int Matk { get; set; }

    public int Matruyen { get; set; }

    [StringLength(1000)]
    public string Noidung { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Ngaybinhluan { get; set; }

    [ForeignKey("Matk")]
    [InverseProperty("Binhluans")]
    public virtual Taikhoan MatkNavigation { get; set; } = null!;

    [ForeignKey("Matruyen")]
    [InverseProperty("Binhluans")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
