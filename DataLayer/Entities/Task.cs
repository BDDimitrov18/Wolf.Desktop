using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Task
{
    public int Taskid { get; set; }

    public int Activityid { get; set; }

    public TimeOnly Duration { get; set; }

    public DateTime Startdate { get; set; }

    public int Executantid { get; set; }

    public int? Controlid { get; set; }

    public string? Comments { get; set; }

    public int Tasktypeid { get; set; }

    public string Commenttax { get; set; } = null!;

    public double Executantpayment { get; set; }

    public double Tax { get; set; }

    public DateTime Finishdate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Activity Activity { get; set; } = null!;

    public virtual Employee? Control { get; set; }

    public virtual Employee Executant { get; set; } = null!;

    public virtual Tasktype Tasktype { get; set; } = null!;
}
