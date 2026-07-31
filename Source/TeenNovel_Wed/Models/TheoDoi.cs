using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("TheoDoi")]
[Index("MaDocGia", Name = "IX_TheoDoi_MaDocGia")]
[Index("MaDocGia", "Matruyen", Name = "UQ__TheoDoi__B8E2FF322F50DECF", IsUnique = true)]
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
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Matruyen")]
    [InverseProperty("TheoDois")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
