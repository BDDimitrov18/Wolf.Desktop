namespace Wolf.Dtos;

public class DocumentofownershipOwnerrelashionshipDto
{
    public int Documentownerid { get; set; }
    public int Documentid { get; set; }
    public int Ownerid { get; set; }
}

public class CreateDocumentofownershipOwnerrelashionshipDto
{
    public int Documentid { get; set; }
    public int Ownerid { get; set; }
}
