using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("Chuong")]
[Index("Matruyen", Name = "IX_Chuong_Matruyen")]
public partial class Chuong
{
    [Key]
    public int Machuong { get; set; }

    public int Matruyen { get; set; }

    [StringLength(200)]
    public string Tenchuong { get; set; } = null!;

    public string Noidung { get; set; } = null!;

    public int Giaxu { get; set; }

    public int Thutu { get; set; }

    public int LuotDoc { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaydang { get; set; }

    [InverseProperty("MachuongNavigation")]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    [InverseProperty("MachuongNavigation")]
    public virtual ICollection<LichSuDoc> LichSuDocs { get; set; } = new List<LichSuDoc>();

    [ForeignKey("Matruyen")]
    [InverseProperty("Chuongs")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
