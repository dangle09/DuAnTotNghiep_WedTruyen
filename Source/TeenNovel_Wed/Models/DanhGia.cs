using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Index("Matruyen", Name = "IX_DanhGia_Matruyen")]
[Index("MaDocGia", "Matruyen", Name = "UQ__DanhGia__B8E2FF32742050B3", IsUnique = true)]
[Table("DanhGia")]
public partial class DanhGia
{
    [Key]
    public int Madanhgia { get; set; }

    public int MaDocGia { get; set; }

    public int Matruyen { get; set; }

    public byte Sosao { get; set; }

    [StringLength(1000)]
    public string? Nhanxet { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaydanhgia { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("DanhGia")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Matruyen")]
    [InverseProperty("DanhGias")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
