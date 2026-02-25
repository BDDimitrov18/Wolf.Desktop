namespace Wolf.Dtos;

public class TasktypeDto
{
    public int Tasktypeid { get; set; }
    public string Tasktypename { get; set; } = null!;
    public int Activitytypeid { get; set; }
}

public class CreateTasktypeDto
{
    public string Tasktypename { get; set; } = null!;
    public int Activitytypeid { get; set; }
}
