using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Invoice
{
    public int Invoiceid { get; set; }

    public int Requestid { get; set; }

    public double Sum { get; set; }

    public string Number { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}
