using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Documentsofownership
{
    public int Documentid { get; set; }

    public string Typeofdocument { get; set; } = null!;

    public string Numberofdocument { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public int Tom { get; set; }

    public string Register { get; set; } = null!;

    public string Doccase { get; set; } = null!;

    public DateTime Dateofissuing { get; set; }

    public DateTime Dateofregistering { get; set; }

    public string Typeofownership { get; set; } = null!;

    public virtual ICollection<DocumentofownershipOwnerrelashionship> DocumentofownershipOwnerrelashionships { get; set; } = new List<DocumentofownershipOwnerrelashionship>();

    public virtual ICollection<PlotDocumentofownership> PlotDocumentofownerships { get; set; } = new List<PlotDocumentofownership>();
}
