namespace Wolf.Dtos;

public class DocumentplotDocumentowenerrelashionshipDto
{
    public int Id { get; set; }
    public int Documentplotid { get; set; }
    public int Documentownerid { get; set; }
    public double Idealparts { get; set; }
    public string Wayofacquiring { get; set; } = null!;
    public bool? Isdrob { get; set; }
    public int Powerofattorneyid { get; set; }
}

public class CreateDocumentplotDocumentowenerrelashionshipDto
{
    public int Documentplotid { get; set; }
    public int Documentownerid { get; set; }
    public double Idealparts { get; set; }
    public string Wayofacquiring { get; set; } = null!;
    public bool? Isdrob { get; set; }
    public int Powerofattorneyid { get; set; }
}
