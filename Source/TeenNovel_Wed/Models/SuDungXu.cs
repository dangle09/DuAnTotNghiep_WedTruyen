using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("SuDungXu")]
[Index("MaDocGia", "Machuong", Name = "UQ_SuDungXu", IsUnique = true)]
public partial class SuDungXu
{
    [Key]
    public int MaSuDungXu { get; set; }

    public int MaDocGia { get; set; }

    public int Machuong { get; set; }

    public int SoXu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime NgaySuDung { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("SuDungXus")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Machuong")]
    [InverseProperty("SuDungXus")]
    public virtual Chuong MachuongNavigation { get; set; } = null!;
}
