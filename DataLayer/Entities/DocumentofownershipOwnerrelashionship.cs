using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class DocumentofownershipOwnerrelashionship
{
    public int Documentownerid { get; set; }

    public int Documentid { get; set; }

    public int Ownerid { get; set; }

    public virtual Documentsofownership Document { get; set; } = null!;

    public virtual ICollection<DocumentplotDocumentowenerrelashionship> DocumentplotDocumentowenerrelashionships { get; set; } = new List<DocumentplotDocumentowenerrelashionship>();

    public virtual Owner Owner { get; set; } = null!;
}
