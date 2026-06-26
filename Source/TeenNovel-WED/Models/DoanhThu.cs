using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TeenNovel_WED.Models;

[Table("DoanhThu")]
public partial class DoanhThu
{
    [Key]
    public int Madoanhthu { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Tongtien { get; set; }

    public int Tongxunap { get; set; }

    public int LuotNap { get; set; }
}
