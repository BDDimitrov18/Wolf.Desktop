namespace Wolf.Dtos;

public class PlotDto
{
    public int Plotid { get; set; }
    public string Plotnumber { get; set; } = null!;
    public string? Regulatedplotnumber { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? Municipality { get; set; }
    public string? Street { get; set; }
    public int? Streetnumber { get; set; }
    public string Designation { get; set; } = null!;
    public string? Locality { get; set; }
}

public class CreatePlotDto
{
    public string Plotnumber { get; set; } = null!;
    public string? Regulatedplotnumber { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? Municipality { get; set; }
    public string? Street { get; set; }
    public int? Streetnumber { get; set; }
    public string Designation { get; set; } = null!;
    public string? Locality { get; set; }
}
