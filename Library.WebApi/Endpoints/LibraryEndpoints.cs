using Library.WebApi.Endpoints.Internal;
using Library.WebApi.Filters;
using Library.WebApi.Models;
using Library.WebApi.Models.FailureResponses;
using Library.WebApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Library.WebApi.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Books";
    private const string BaseRoute = "books";

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute, GetAllBooksAsync)
            .WithName("GetBooks")
            .WithTags(Tag)
            .Produces<List<Book>>(StatusCodes.Status200OK);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookAsync)
            .WithName("GetBook")
            .WithTags(Tag)
            .Produces<Book>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound);

        app.MapPost(BaseRoute, CreateBookAsync)
            .AddFilter<ValidationFilter<Book>>()
            .WithName("CreateBook")
            .WithTags(Tag)
            .Accepts<Book>(ContentType)
            .Produces<Book>(StatusCodes.Status201Created)
            .Produces<CreationFailureResponse>(StatusCodes.Status400BadRequest)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest);

        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
            .AddFilter<ValidationFilter<Book>>()
            .WithName("UpdateBook")
            .WithTags(Tag)
            .Accepts<Book>(ContentType)
            .Produces(StatusCodes.Status200OK)
            .Produces<UpdateFailureResponse>(StatusCodes.Status404NotFound)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
            .WithName("DeleteBook")
            .WithTags(Tag)
            .Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    internal static async ValueTask<Ok<List<Book>>> GetAllBooksAsync(string? searchTerm, IBookService bookService)
    {
        return TypedResults.Ok(await bookService.SearchAsync(searchTerm));
    }

    internal static async ValueTask<Results<Ok<Book>, NotFound>> GetBookAsync(string isbn, IBookService bookService)
    {
        return await bookService.GetByIsbnAsync(isbn) switch
        {
            { } book => TypedResults.Ok(book),
            _        => TypedResults.NotFound()
        };
    }

    internal static async ValueTask<Results<CreatedAtRoute<Book>, BadRequest<CreationFailureResponse>, NotFound>>
        CreateBookAsync(Book book, IBookService bookService) //, LinkGenerator linker, HttpContext context)
    {
        return await bookService.CreateAsync(book) switch
        {
            // true  => TypedResults.Created($"{BaseRoute}/{book.Isbn}", book),
            true  => TypedResults.CreatedAtRoute(book, "GetBook", new {isbn = book.Isbn}),
            // true  => TypedResults.Created(linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn })!, book),
            false => TypedResults.BadRequest(
                new CreationFailureResponse("Book not added, because it already exists in the library"))
        };
    }

    internal static async ValueTask<Results<Ok<Book>, NotFound<UpdateFailureResponse>>>
        UpdateBookAsync(string isbn, Book book, IBookService bookService)
    {
        var updatedBook = book with {Isbn = isbn};
        return await bookService.UpdateAsync(updatedBook)switch
        {
            true  => TypedResults.Ok(book),
            false => TypedResults.NotFound(
                new UpdateFailureResponse("Book not updated, because it does not exist in the library"))
        };
    }

    internal static async ValueTask<Results<NoContent, NotFound<DeletionFailureResponse>>>
        DeleteBookAsync(string isbn, IBookService bookService)
    {
        return await bookService.DeleteAsync(isbn) switch
        {
            true  => TypedResults.NoContent(),
            false => TypedResults.NotFound(
                new DeletionFailureResponse("Book not deleted, because it does not exist in the library"))
        };
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
    }
}