using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Index("Matk", Name = "IX_DocGia_Matk")]
[Index("Matk", Name = "UQ__DocGia__27240439AB5C9569", IsUnique = true)]
public partial class DocGium
{
    [Key]
    public int MaDocGia { get; set; }

    public int Matk { get; set; }

    [StringLength(100)]
    public string Ten { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? Sodienthoai { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    public int Soxu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Ngaytao { get; set; }

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<HoTro> HoTros { get; set; } = new List<HoTro>();

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<LichSuDoc> LichSuDocs { get; set; } = new List<LichSuDoc>();

    [ForeignKey("Matk")]
    [InverseProperty("DocGium")]
    public virtual TaiKhoan MatkNavigation { get; set; } = null!;

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<NapXu> NapXus { get; set; } = new List<NapXu>();

    [InverseProperty("MaDocGiaNavigation")]
    public virtual ICollection<TheoDoi> TheoDois { get; set; } = new List<TheoDoi>();
}
