using Library.WebApi.Models;
using Microsoft.Extensions.Options;
using SQLite;

namespace Library.WebApi.Services;

public class SQLiteBookService : IBookService
{
    private readonly SQLiteAsyncConnection _db;
    private readonly Lazy<Task> _init;

    public SQLiteBookService(IOptions<DatabaseOptions>? options = null)
    {
        var connectionString = options?.Value.ConnectionString ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "default.db");

        _db = new SQLiteAsyncConnection(connectionString);
        _init = new Lazy<Task>(() => _db.CreateTableAsync<Book>());
    }

    public async ValueTask<bool> CreateAsync(Book book)
    {
        await _init.Value;
        return await _db.Table<Book>().Where(b => b.Isbn == book.Isbn).CountAsync() switch
        {
            0 => await _db.InsertAsync(book) == 1,
            _ => await Task.FromResult(false)
        };
    }

    public async ValueTask<bool> UpdateAsync(Book book)
    {
        await _init.Value;
        return await _db.UpdateAsync(book) == 1;
    }

    public async ValueTask<bool> DeleteAsync(string isbn)
    {
        await _init.Value;
        return await _db.DeleteAsync<Book>(isbn) == 1;
    }

    public async ValueTask<Book?> GetByIsbnAsync(string isbn)
    {
        await _init.Value;
        return await _db.FindAsync<Book>(book => book.Isbn == isbn);
    }

    public async ValueTask<List<Book>> SearchAsync(string? searchTerm)
    {
        await _init.Value;
        return (searchTerm?.ToLower() ?? "") switch
        {
            {Length: > 0} searchTermLower
                => await _db.Table<Book>()
                    .Where(book =>
                        book.Isbn.ToLower().Contains(searchTermLower) ||
                        book.Author.ToLower().Contains(searchTermLower) ||
                        book.Title.ToLower().Contains(searchTermLower))
                    .ToListAsync(),
            _   => await _db.Table<Book>().ToListAsync()
        };
    }
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));
        services.AddSingleton<SQLiteBookService>();
    }
}