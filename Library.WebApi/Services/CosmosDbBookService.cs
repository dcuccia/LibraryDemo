using Library.WebApi.Models;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Specification;

namespace Library.WebApi.Services;

public class CosmosDbBookService : IBookService
{
    private readonly IRepository<CosmosDbBook> _repository;

    public CosmosDbBookService(IRepository<CosmosDbBook> repository) 
        => _repository = repository;
    public async ValueTask<bool> CreateAsync(Book book)
    {
        if(await _repository.ExistsAsync(book.Isbn))
            return false;

        return await _repository.CreateAsync(book.ToCosmosDbBook()) is { };
    }

    public async ValueTask<bool> UpdateAsync(Book book)
    {
        if(await _repository.ExistsAsync(book.Isbn) is false)
            return false;

        return await _repository.UpdateAsync(book.ToCosmosDbBook()) is { };
    }

    public async ValueTask<bool> DeleteAsync(string isbn)
    {
        if(await _repository.ExistsAsync(isbn))
            return false;

        await _repository.DeleteAsync(isbn);
        return true;
    }

    public async ValueTask<Book?> GetByIsbnAsync(string isbn)
        => (await _repository.GetAsync(isbn)).ToBook();

    public async ValueTask<List<Book>> SearchAsync(string? searchTerm)
    {
        return (searchTerm?.ToLower() ?? "") switch
        {
            {Length: > 0} searchTermLower
                => (await _repository
                    .QueryAsync(new BookMatchingSearchTermSpecification(searchTermLower)))
                    .Items
                    .Select(CosmosDbBookMapper.ToBook)
                    .ToList(),
            _   => (await _repository
                    .GetAsync(_ => true))
                    .Select(CosmosDbBookMapper.ToBook)
                    .ToList()
        };
    }

    public async ValueTask<List<Book>> GetAllAsync() => await SearchAsync("");
    
    private class BookMatchingSearchTermSpecification: DefaultSpecification<CosmosDbBook>
    {
        public BookMatchingSearchTermSpecification(string searchTerm)
        {
            Query.Where(cosmosDbBook =>
                cosmosDbBook.Id.ToLower().Contains(searchTerm) ||
                cosmosDbBook.Author.ToLower().Contains(searchTerm) ||
                cosmosDbBook.Title.ToLower().Contains(searchTerm));
        }
    }
}