using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Powerofattorneydocument
{
    public int Powerofattorneyid { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Dateofissuing { get; set; }

    public string Issuer { get; set; } = null!;

    public virtual ICollection<DocumentplotDocumentowenerrelashionship> DocumentplotDocumentowenerrelashionships { get; set; } = new List<DocumentplotDocumentowenerrelashionship>();
}
