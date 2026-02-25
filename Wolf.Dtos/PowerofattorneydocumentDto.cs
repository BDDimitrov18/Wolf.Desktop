namespace Wolf.Dtos;

public class PowerofattorneydocumentDto
{
    public int Powerofattorneyid { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Dateofissuing { get; set; }
    public string Issuer { get; set; } = null!;
}

public class CreatePowerofattorneydocumentDto
{
    public string Number { get; set; } = null!;
    public DateTime Dateofissuing { get; set; }
    public string Issuer { get; set; } = null!;
}
