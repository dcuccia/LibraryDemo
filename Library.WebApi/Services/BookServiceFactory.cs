using Library.WebApi.Models;
using Microsoft.Extensions.Options;
using SQLite;

namespace Library.WebApi.Services;

public class BookServiceFactory
{
    private readonly IServiceProvider _provider;
    private readonly IOptions<DatabaseOptions>? _options;

    public BookServiceFactory(IServiceProvider provider, IOptions<DatabaseOptions>? options = null)
        => (_provider, _options) = (provider, options);

    public IBookService GetBookService()
        => (_options?.Value.ServiceType ?? "InMemory") switch
        {
            "CosmosDb"      => _provider.GetRequiredService<CosmosDbBookService>(),
            "SQLite"        => _provider.GetRequiredService<SQLiteBookService>(),
            "InMemory" or _ => _provider.GetRequiredService<InMemoryBookService>(),
        };
}