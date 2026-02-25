using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class ClientRequestrelashionship
{
    public int Requestid { get; set; }

    public int Clientid { get; set; }

    public string? Ownershiptype { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}
