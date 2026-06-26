using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("VaiTro")]
[Index("TenVaiTro", Name = "UQ__VaiTro__1DA55814F19BA851", IsUnique = true)]
public partial class VaiTro
{
    [Key]
    public int MaVaiTro { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string TenVaiTro { get; set; } = null!;

    [InverseProperty("MaVaiTroNavigation")]
    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
