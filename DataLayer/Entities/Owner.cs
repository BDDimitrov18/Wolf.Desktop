using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Owner
{
    public int Ownerid { get; set; }

    public string? Egn { get; set; }

    public string? Address { get; set; }

    public string Fullname { get; set; } = null!;

    public virtual ICollection<DocumentofownershipOwnerrelashionship> DocumentofownershipOwnerrelashionships { get; set; } = new List<DocumentofownershipOwnerrelashionship>();
}
