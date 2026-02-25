namespace Wolf.Dtos;

public class PlotDocumentofownershipDto
{
    public int Documentplotid { get; set; }
    public int Plotid { get; set; }
    public int Documentofownershipid { get; set; }
}

public class CreatePlotDocumentofownershipDto
{
    public int Plotid { get; set; }
    public int Documentofownershipid { get; set; }
}
