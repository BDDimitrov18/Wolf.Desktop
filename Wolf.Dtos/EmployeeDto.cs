namespace Wolf.Dtos;

public class EmployeeDto
{
    public int Employeeid { get; set; }
    public string Firstname { get; set; } = null!;
    public string? Secondname { get; set; }
    public string? Lastname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool? Outsider { get; set; }
}

public class CreateEmployeeDto
{
    public string Firstname { get; set; } = null!;
    public string? Secondname { get; set; }
    public string? Lastname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool? Outsider { get; set; }
}
