using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_Wed.Models;

[Table("Bookmark")]
[Index("MaDocGia", Name = "IX_Bookmark_MaDocGia")]
[Index("Machuong", Name = "IX_Bookmark_Machuong")]
public partial class Bookmark
{
    [Key]
    public int MaBookmark { get; set; }

    public int MaDocGia { get; set; }

    public int Matruyen { get; set; }

    public int Machuong { get; set; }

    public int SoDong { get; set; }

    [StringLength(200)]
    public string? GhiChu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaytao { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("Bookmarks")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;

    [ForeignKey("Machuong")]
    [InverseProperty("Bookmarks")]
    public virtual Chuong MachuongNavigation { get; set; } = null!;

    [ForeignKey("Matruyen")]
    [InverseProperty("Bookmarks")]
    public virtual Truyen MatruyenNavigation { get; set; } = null!;
}
