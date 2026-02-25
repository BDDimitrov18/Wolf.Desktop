using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Activity
{
    public int Activityid { get; set; }

    public int Requestid { get; set; }

    public int Activitytypeid { get; set; }

    public DateTime Expectedduration { get; set; }

    public int? Parentactivityid { get; set; }

    public int Executantid { get; set; }

    public double Employeepayment { get; set; }

    public DateTime Startdate { get; set; }

    public virtual ICollection<ActivityPlotrelashionship> ActivityPlotrelashionships { get; set; } = new List<ActivityPlotrelashionship>();

    public virtual Activitytype Activitytype { get; set; } = null!;

    public virtual Employee Executant { get; set; } = null!;

    public virtual ICollection<Activity> InverseParentactivity { get; set; } = new List<Activity>();

    public virtual Activity? Parentactivity { get; set; }

    public virtual Request Request { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
