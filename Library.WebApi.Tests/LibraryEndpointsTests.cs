using Library.WebApi.Models;
using Library.WebApi.Models.FailureResponses;

namespace Library.WebApi.Tests;

public class LibraryEndpointsTests : IClassFixture<LibraryApiFactory>, IAsyncLifetime
{
    private readonly LibraryApiFactory _factory;
    private readonly List<string> _createdIsbns = new();

    public LibraryEndpointsTests(LibraryApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"http://localhost/books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook() with { Isbn = "INVALID" };
        
        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var validationFailureResponse = await result.Content.ReadFromJsonAsync<ValidationFailureResponse>();
        var error = validationFailureResponse!.Errors.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.ProperyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var validationFailureResponse = await result.Content.ReadFromJsonAsync<CreationFailureResponse>();
        var error = validationFailureResponse!.Errors.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Should().Be("Book not added, because it already exists in the library");
    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBook.Should().BeEquivalentTo(book);
    }
    
    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();
        
        // Act
        var result = await httpClient.GetAsync(isbn);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsAllBooks_WhenBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };
        
        // Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);
    }
    
    [Fact]
    public async Task GetAllBooks_ReturnsNoBooks_WhenNoBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        
        // Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleMatches()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };
        
        // Act
        var result = await httpClient.GetAsync($"/books?searchTerm=eye");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks!.First().Should().BeEquivalentTo(book);
    }
    
    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var updatedBook = book with { Title = "The Great Hunt" };
        var result = await httpClient.PutAsJsonAsync($"/books/{updatedBook.Isbn}", updatedBook);
        var returnedBook = await result.Content.ReadFromJsonAsync<Book>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBook.Should().BeEquivalentTo(updatedBook);
    }
    
    [Fact]
    public async Task UpdateBook_DoesNotUpdateBook_WhenDataIsIncorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        var updatedBook = book with { Title = string.Empty };
        var result = await httpClient.PutAsJsonAsync($"/books/{updatedBook.Isbn}", updatedBook);
        var validationFailureResponse = await result.Content.ReadFromJsonAsync<ValidationFailureResponse>();
        var error = validationFailureResponse!.Errors.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.ProperyName.Should().Be("Title");
        error.ErrorMessage.Should().Be("'Title' must not be empty.");
    }

    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        
        // Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var updateFailureResponse = await result.Content.ReadFromJsonAsync<UpdateFailureResponse>();
        var error = updateFailureResponse!.Errors.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Should().Be("Book not updated, because it does not exist in the library");
    }
    
    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();
        
        // Act
        var result = await httpClient.DeleteAsync($"/books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenBookDoesExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    private Book GenerateBook(string title = "The Eye of the World")
        => new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Robert Jordan",
            PageCount = 1337,
            ShortDescription = "Great first-book world-building with somewhat shaky canon.",
            ReleaseDate = new DateTime(1990, 1, 15)
        };

    private string GenerateIsbn()
        => $"{Random.Shared.Next(100, 999)}-" + $"{Random.Shared.Next(1000000000, 2100999999)}";

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach (var createdIsbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{createdIsbn}");
        }
    }
}