using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Models;

public partial class Staff
{
    [StringLength(8)]
    public string Id { get; set; } = null!;

    [StringLength(100)]
    public string Fullname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public string Gender { get; set; } = null!;
}
