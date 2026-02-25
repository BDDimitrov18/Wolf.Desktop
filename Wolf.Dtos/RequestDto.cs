namespace Wolf.Dtos;

public class RequestDto
{
    public int Requestid { get; set; }
    public string Requestname { get; set; } = null!;
    public double Price { get; set; }
    public string Paymentstatus { get; set; } = null!;
    public double Advance { get; set; }
    public string? Comments { get; set; }
    public string? Path { get; set; }
    public int? Requestcreatorid { get; set; }
    public string Status { get; set; } = null!;
}

public class CreateRequestDto
{
    public string Requestname { get; set; } = null!;
    public double Price { get; set; }
    public string Paymentstatus { get; set; } = null!;
    public double Advance { get; set; }
    public string? Comments { get; set; }
    public string? Path { get; set; }
    public int? Requestcreatorid { get; set; }
    public string Status { get; set; } = null!;
}
