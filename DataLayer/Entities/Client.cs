using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Client
{
    public int Clientid { get; set; }

    public string Firstname { get; set; } = null!;

    public string? Middlename { get; set; }

    public string? Lastname { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Clientlegaltype { get; set; }

    public virtual ICollection<ClientRequestrelashionship> ClientRequestrelashionships { get; set; } = new List<ClientRequestrelashionship>();
}
