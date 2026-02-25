using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Employee
{
    public int Employeeid { get; set; }

    public string Firstname { get; set; } = null!;

    public string? Secondname { get; set; }

    public string? Lastname { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool? Outsider { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ApplicationUser? Aspnetuser { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual ICollection<StarrequestEmployeerelashionship> StarrequestEmployeerelashionships { get; set; } = new List<StarrequestEmployeerelashionship>();

    public virtual ICollection<Task> TaskControls { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskExecutants { get; set; } = new List<Task>();
}
