namespace Wolf.Dtos;

public class ClientRequestrelashionshipDto
{
    public int Requestid { get; set; }
    public int Clientid { get; set; }
    public string? Ownershiptype { get; set; }
}

public class CreateClientRequestrelashionshipDto
{
    public int Requestid { get; set; }
    public int Clientid { get; set; }
    public string? Ownershiptype { get; set; }
}
