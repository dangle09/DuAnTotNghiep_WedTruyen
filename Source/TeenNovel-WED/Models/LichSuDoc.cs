using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("LichSuDoc")]
[Index("MaDocGia", Name = "IX_LichSuDoc_MaDocGia")]
public partial class LichSuDoc
{
    [Key]
    public int Malichsu { get; set; }

    public int MaDocGia { get; set; }

    public int Machuong { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaydoc { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("LichSuDocs")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Machuong")]
    [InverseProperty("LichSuDocs")]
    public virtual Chuong MachuongNavigation { get; set; } = null!;
}
