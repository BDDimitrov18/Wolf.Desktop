using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class DocumentplotDocumentowenerrelashionship
{
    public int Documentplotid { get; set; }

    public int Documentownerid { get; set; }

    public double Idealparts { get; set; }

    public string Wayofacquiring { get; set; } = null!;

    public bool? Isdrob { get; set; }

    public int Powerofattorneyid { get; set; }

    public int Id { get; set; }

    public virtual DocumentofownershipOwnerrelashionship Documentowner { get; set; } = null!;

    public virtual PlotDocumentofownership Documentplot { get; set; } = null!;

    public virtual Powerofattorneydocument Powerofattorney { get; set; } = null!;
}
