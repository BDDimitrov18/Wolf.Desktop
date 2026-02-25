using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Efmigrationshistory
{
    public string Migrationid { get; set; } = null!;

    public string Productversion { get; set; } = null!;
}
