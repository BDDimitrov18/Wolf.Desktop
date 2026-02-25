using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class ActivityPlotrelashionship
{
    public int Activityid { get; set; }

    public int Plotid { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Plot Plot { get; set; } = null!;
}
