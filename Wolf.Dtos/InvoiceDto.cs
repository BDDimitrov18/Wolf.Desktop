namespace Wolf.Dtos;

public class InvoiceDto
{
    public int Invoiceid { get; set; }
    public int Requestid { get; set; }
    public double Sum { get; set; }
    public string Number { get; set; } = null!;
}

public class CreateInvoiceDto
{
    public int Requestid { get; set; }
    public double Sum { get; set; }
    public string Number { get; set; } = null!;
}
