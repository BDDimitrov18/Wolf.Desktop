namespace Wolf.Dtos;

public class ActivitytypeDto
{
    public int Activitytypeid { get; set; }
    public string Activitytypename { get; set; } = null!;
}

public class CreateActivitytypeDto
{
    public string Activitytypename { get; set; } = null!;
}
