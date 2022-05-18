using Library.WebApi.Models;

namespace Library.WebApi.Services;

public class InMemoryBookService : IBookService
{
    private readonly Dictionary<string, Book> _books = new();

    public async ValueTask<bool> CreateAsync(Book book) => await Task.FromResult(_books.TryAdd(book.Isbn, book));
    public async ValueTask<bool> UpdateAsync(Book book)
    {
        static bool Update(Dictionary<string, Book> books, Book b)
        {
            if(!books.ContainsKey(b.Isbn))
                return false;
            books[b.Isbn] = b;
            return true;
        }

        return await Task.FromResult(Update(_books, book));
    }
    public async ValueTask<bool> DeleteAsync(string isbn) => await Task.FromResult(_books.Remove(isbn));
    public async ValueTask<Book?> GetByIsbnAsync(string isbn) => await Task.FromResult(
        _books.TryGetValue(isbn, out var book) switch
        {
            true  => book,
            false => null
        });
    public async ValueTask<List<Book>> SearchAsync(string? searchTerm) => await Task.FromResult(
        searchTerm switch
        {
            {Length: > 0} 
                => _books.Values.Where(book =>
                    book.Isbn.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                    book.Author.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                    book.Title.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList(),
            _   => _books.Values.ToList(),
        });
}