using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApiClient _client;

    public InvoiceService(ApiClient client) => _client = client;

    public Task<IEnumerable<InvoiceDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<InvoiceDto>>("api/invoices")!;

    public Task<InvoiceDto?> GetByIdAsync(int id) =>
        _client.GetAsync<InvoiceDto>($"api/invoices/{id}");

    public Task<InvoiceDto?> CreateAsync(CreateInvoiceDto dto) =>
        _client.PostAsync<InvoiceDto>("api/invoices", dto);

    public Task UpdateAsync(int id, InvoiceDto dto) =>
        _client.PutAsync($"api/invoices/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/invoices/{id}");
}
