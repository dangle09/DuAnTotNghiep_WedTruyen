using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("TheoDoi")]
[Index("MaDocGia", Name = "IX_TheoDoi_MaDocGia")]
[Index("MaDocGia", "Matruyen", Name = "UQ__TheoDoi__B8E2FF32F5055A3D", IsUnique = true)]
public partial class TheoDoi
{
    [Key]
    public int Matheodoi { get; set; }

    public int MaDocGia { get; set; }

    public int Matruyen { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaytheodoi { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("TheoDois")]
    public virtual DocGium MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Matruyen")]
    [InverseProperty("TheoDois")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
