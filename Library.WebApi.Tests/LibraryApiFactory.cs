using Library.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Library.WebApi.Tests;

public class LibraryApiFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(collection =>
        {
            collection.RemoveAll(typeof(BookServiceFactory));
            collection.AddSingleton<BookServiceFactory>(provider =>
                new BookServiceFactory(provider, Options.Create(new DatabaseOptions
                {
                    ServiceType = "InMemory"
                })));
        });
    }
}