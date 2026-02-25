using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class PlotDocumentofownership
{
    public int Documentplotid { get; set; }

    public int Plotid { get; set; }

    public int Documentofownershipid { get; set; }

    public virtual Documentsofownership Documentofownership { get; set; } = null!;

    public virtual ICollection<DocumentplotDocumentowenerrelashionship> DocumentplotDocumentowenerrelashionships { get; set; } = new List<DocumentplotDocumentowenerrelashionship>();

    public virtual Plot Plot { get; set; } = null!;
}
