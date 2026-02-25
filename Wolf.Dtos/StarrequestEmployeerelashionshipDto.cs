namespace Wolf.Dtos;

public class StarrequestEmployeerelashionshipDto
{
    public int Requestid { get; set; }
    public int Employeeid { get; set; }
    public string Starcolor { get; set; } = null!;
}

public class CreateStarrequestEmployeerelashionshipDto
{
    public int Requestid { get; set; }
    public int Employeeid { get; set; }
    public string Starcolor { get; set; } = null!;
}
