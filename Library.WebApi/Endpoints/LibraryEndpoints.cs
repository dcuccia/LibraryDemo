using FluentValidation;
using Library.WebApi.Endpoints.Internal;
using Library.WebApi.Filters;
using Library.WebApi.Models;
using Library.WebApi.Services;

namespace Library.WebApi.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/books", async (string? searchTerm, IBookService bookService)
            => Results.Ok(await bookService.SearchAsync(searchTerm)))
            .WithName("GetBooks")
            .WithTags("Books")
            .Produces<List<Book>>(StatusCodes.Status200OK);

        app.MapGet("/books/{isbn}", async (string isbn, IBookService bookService)
            => await bookService.GetByIsbnAsync(isbn) switch
                {
                    {} book => Results.Ok(book),
                    _       => Results.NotFound()
                })
            .WithName("GetBook")
            .WithTags("Books")
            .Produces<Book>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/books", async (Book book, IBookService bookService) //, LinkGenerator linker, HttpContext context)
            => await bookService.CreateAsync(book) switch
                {
                    // true  => Results.Created($"/books/{book.Isbn}", book),
                    true  => Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book),
                    // true  => Results.Created(linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn })!, book),
                    false => Results.BadRequest(new CreationFailureResponse("Book not added, because it already exists in the library"))
                })
            .AddFilter<ValidationFilter<Book>>()
            .WithName("CreateBook")
            .WithTags("Books")
            .Accepts<Book>("application/json")
            .Produces<Book>(StatusCodes.Status201Created)
            .Produces<CreationFailureResponse>(StatusCodes.Status400BadRequest)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest);

        app.MapPut("/books/{isbn}", async (string isbn, Book book, IBookService bookService)
            => await bookService.UpdateAsync(book with { Isbn = isbn }) switch
                {
                    true  => Results.Ok("Book updated!"),
                    false => Results.NotFound(new UpdateFailureResponse("Book not updated, because it does not exist in the library"))
                })
            .AddFilter<ValidationFilter<Book>>()
            .WithName("UpdateBook")
            .WithTags("Books")
            .Accepts<Book>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<UpdateFailureResponse>(StatusCodes.Status404NotFound)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest);

        app.MapDelete("/books/{isbn}", async (string isbn, IBookService bookService)
            => await bookService.DeleteAsync(isbn) switch
                {
                    true  => Results.NoContent(),
                    false => Results.NotFound("Book not deleted, because it does not exist in the library")
                })
            .WithName("DeleteBook")
            .WithTags("Books")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));
        services.AddSingleton<InMemoryBookService>();
        services.AddSingleton<SQLiteBookService>();
        services.AddCosmosRepository(
            options =>
            {
                options.CosmosConnectionString = configuration.GetValue<string>("DatabaseOptions:ConnectionString");
                options.ContainerId = "book-store";
                options.DatabaseId = "library";
                options.ContainerPerItemType = true;
                options.ContainerBuilder.Configure<CosmosDbBook>(containerOptions => containerOptions
                    .WithContainer("books")
                    .WithPartitionKey("/isbn")
                    // .WithContainerDefaultTimeToLive(TimeSpan.FromMinutes(1))
                    // .WithManualThroughput(500)
                    .WithSyncableContainerProperties()
                );
            });
        services.AddSingleton<CosmosDbBookService>();
        services.AddSingleton<BookServiceFactory>();
        services.AddSingleton<IBookService>(provider => provider.GetRequiredService<BookServiceFactory>().GetBookService());
        
        services.AddValidatorsFromAssemblyContaining<Program>();
    }
}