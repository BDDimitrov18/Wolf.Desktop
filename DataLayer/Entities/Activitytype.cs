using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Activitytype
{
    public int Activitytypeid { get; set; }

    public string Activitytypename { get; set; } = null!;

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<Tasktype> Tasktypes { get; set; } = new List<Tasktype>();
}
