using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class StarrequestEmployeerelashionship
{
    public int Requestid { get; set; }

    public int Employeeid { get; set; }

    public string Starcolor { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}
