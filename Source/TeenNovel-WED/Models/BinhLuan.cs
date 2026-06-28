using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("BinhLuan")]
[Index("Matruyen", Name = "IX_BinhLuan_Matruyen")]
public partial class BinhLuan
{
    [Key]
    public int Mabinhluan { get; set; }

    public int MaDocGia { get; set; }

    public int Matruyen { get; set; }

    [StringLength(1000)]
    public string Noidung { get; set; } = null!;

    public byte TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaybinhluan { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("BinhLuans")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Matruyen")]
    [InverseProperty("BinhLuans")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
