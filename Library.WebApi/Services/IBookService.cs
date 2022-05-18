using Library.WebApi.Models;

namespace Library.WebApi.Services;

public interface IBookService
{
    public static abstract void AddServices(IServiceCollection services, IConfiguration configuration);
    
    ValueTask<bool> CreateAsync(Book book);
    ValueTask<bool> UpdateAsync(Book book);
    
    ValueTask<bool> DeleteAsync(string isbn);
    ValueTask<Book?> GetByIsbnAsync(string isbn);
    ValueTask<List<Book>> SearchAsync(string? searchTerm);
}