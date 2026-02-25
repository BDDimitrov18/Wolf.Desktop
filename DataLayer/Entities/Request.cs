using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Request
{
    public int Requestid { get; set; }

    public string Requestname { get; set; } = null!;

    public double Price { get; set; }

    public string Paymentstatus { get; set; } = null!;

    public double Advance { get; set; }

    public string? Comments { get; set; }

    public string? Path { get; set; }

    public int? Requestcreatorid { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<ClientRequestrelashionship> ClientRequestrelashionships { get; set; } = new List<ClientRequestrelashionship>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Employee? Requestcreator { get; set; }

    public virtual ICollection<StarrequestEmployeerelashionship> StarrequestEmployeerelashionships { get; set; } = new List<StarrequestEmployeerelashionship>();
}
