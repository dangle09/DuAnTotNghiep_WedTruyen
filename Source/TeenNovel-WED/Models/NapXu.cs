using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("NapXu")]
[Index("MaDocGia", Name = "IX_NapXu_MaDocGia")]
public partial class NapXu
{
    [Key]
    public int Manap { get; set; }

    public int MaDocGia { get; set; }

    public int Soxunhan { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Sotien { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Phuongthuc { get; set; } = null!;

    [StringLength(20)]
    public string Trangthai { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Ngaynap { get; set; }

    [ForeignKey("MaDocGia")]
    [InverseProperty("NapXus")]
    public virtual DocGia MaDocGiaNavigation { get; set; } = null!;
}
