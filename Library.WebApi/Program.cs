using FluentValidation;
using Library.WebApi;
// using Library.WebApi.Filters;
using Library.WebApi.Models;
using Library.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
var dbConfig = builder.Configuration.GetSection("DatabaseOptions");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOptions();
builder.Services.Configure<DatabaseOptions>(dbConfig);
builder.Services.AddSingleton<InMemoryBookService>();
builder.Services.AddSingleton<SQLiteBookService>();
builder.Services.AddCosmosRepository(
    options =>
    {
        options.CosmosConnectionString = builder.Configuration.GetValue<string>("DatabaseOptions:ConnectionString");
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
builder.Services.AddSingleton<CosmosDbBookService>();
builder.Services.AddSingleton<BookServiceFactory>();
builder.Services.AddSingleton<IBookService>(provider => provider.GetRequiredService<BookServiceFactory>().GetBookService());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/books", async (string? searchTerm, IBookService bookService)
    => Results.Ok(await bookService.SearchAsync(searchTerm))).WithName("GetBooks");

app.MapGet("/books/{isbn}", async (string isbn, IBookService bookService)
    => await bookService.GetByIsbnAsync(isbn) switch
        {
            {} book => Results.Ok(book),
            _       => Results.NotFound()
        }).WithName("GetBook");

app.MapPost("/books", async (Book book, IBookService bookService, IValidator<Book> validator, LinkGenerator linker, HttpContext context)
    => await validator.ValidateAsync(book) switch
        {
            { IsValid: true } => await bookService.CreateAsync(book) switch
                {
                    // true  => Results.Created($"/books/{book.Isbn}", book),
                    // true  => Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book),
                    true  => Results.Created(linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn })!, book),
                    false => Results.BadRequest("Book not added, because it already exists in the library")
                },
            var result        => Results.BadRequest(result.Errors.ToResponse())
        }).WithName("CreateBook");

app.MapPut("/books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator)
    => await validator.ValidateAsync(book with { Isbn = isbn }) switch
        {
            { IsValid: true } => await bookService.UpdateAsync(book with { Isbn = isbn }) switch
                {
                    true  => Results.Ok("Book updated!"),
                    false => Results.NotFound("Book not updated, because it does not exist in the library")
                },
            var result        => Results.BadRequest(result.Errors.ToResponse())
        }).WithName("UpdateBook");

app.MapDelete("/books/{isbn}", async (string isbn, IBookService bookService)
    => await bookService.DeleteAsync(isbn) switch
        {
            true  => Results.NoContent(),
            false => Results.NotFound("Book not deleted, because it does not exist in the library")
        }).WithName("DeleteBook");

app.Run();

// // Future .Net 7 inline filters, once delegate serialization is properly working 
// app.MapPost("/books", async (Book book, IBookService bookService)
//     => await bookService.CreateAsync(book) switch
//     {
//         true  => Results.Created($"/books/{book.Isbn}", book),
//         false => Results.BadRequest("Book not added, because it already exists in the library")
//     })
//    .AddFilter<ValidationFilter<Book>>();

// app.MapPut("/books/{isbn}", async (Book book, IBookService bookService)
//     => await bookService.UpdateAsync(book) switch
//     {
//         true  => Results.Ok("Book updated!"),
//         false => Results.BadRequest("Book not updated, because it does not exist in the library")
//     })
//    .AddFilter<ValidationFilter<Book>>();