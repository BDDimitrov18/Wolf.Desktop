using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Tasktype
{
    public int Tasktypeid { get; set; }

    public string Tasktypename { get; set; } = null!;

    public int Activitytypeid { get; set; }

    public virtual Activitytype Activitytype { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
