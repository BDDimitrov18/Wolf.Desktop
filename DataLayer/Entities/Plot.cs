using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Plot
{
    public int Plotid { get; set; }

    public string Plotnumber { get; set; } = null!;

    public string? Regulatedplotnumber { get; set; }

    public string? Neighborhood { get; set; }

    public string? City { get; set; }

    public string? Municipality { get; set; }

    public string? Street { get; set; }

    public int? Streetnumber { get; set; }

    public string Designation { get; set; } = null!;

    public string? Locality { get; set; }

    public virtual ICollection<ActivityPlotrelashionship> ActivityPlotrelashionships { get; set; } = new List<ActivityPlotrelashionship>();

    public virtual ICollection<PlotDocumentofownership> PlotDocumentofownerships { get; set; } = new List<PlotDocumentofownership>();
}
