namespace Wolf.Dtos;

public class ActivityDto
{
    public int Activityid { get; set; }
    public int Requestid { get; set; }
    public int Activitytypeid { get; set; }
    public DateTime Expectedduration { get; set; }
    public int? Parentactivityid { get; set; }
    public int Executantid { get; set; }
    public double Employeepayment { get; set; }
    public DateTime Startdate { get; set; }
}

public class CreateActivityDto
{
    public int Requestid { get; set; }
    public int Activitytypeid { get; set; }
    public DateTime Expectedduration { get; set; }
    public int? Parentactivityid { get; set; }
    public int Executantid { get; set; }
    public double Employeepayment { get; set; }
    public DateTime Startdate { get; set; }
}
