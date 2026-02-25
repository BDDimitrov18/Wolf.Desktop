namespace Wolf.Dtos;

public class OwnerDto
{
    public int Ownerid { get; set; }
    public string? Egn { get; set; }
    public string? Address { get; set; }
    public string Fullname { get; set; } = null!;
}

public class CreateOwnerDto
{
    public string? Egn { get; set; }
    public string? Address { get; set; }
    public string Fullname { get; set; } = null!;
}
