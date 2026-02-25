namespace Wolf.Dtos;

public class FileDto
{
    public int Fileid { get; set; }
    public string Filename { get; set; } = null!;
    public string Filepath { get; set; } = null!;
    public DateTime Uploadedat { get; set; }
}

public class CreateFileDto
{
    public string Filename { get; set; } = null!;
    public string Filepath { get; set; } = null!;
    public DateTime Uploadedat { get; set; }
}
