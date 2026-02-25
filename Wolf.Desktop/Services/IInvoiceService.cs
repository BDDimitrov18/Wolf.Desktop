using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllAsync();
    Task<InvoiceDto?> GetByIdAsync(int id);
    Task<InvoiceDto?> CreateAsync(CreateInvoiceDto dto);
    Task UpdateAsync(int id, InvoiceDto dto);
    Task DeleteAsync(int id);
}
