using Microsoft.Azure.CosmosRepository;
using SQLite;

namespace Library.WebApi.Models;

public static class CosmosDbBookMapper
{
    public static Book ToBook(this CosmosDbBook cosmosDbBook)
        => new ()
        {
            Isbn = cosmosDbBook.Id,
            Title = cosmosDbBook.Title,
            Author = cosmosDbBook.Author,
            ShortDescription = cosmosDbBook.ShortDescription,
            PageCount = cosmosDbBook.PageCount,
            ReleaseDate = cosmosDbBook.ReleaseDate
        };
    
    public static CosmosDbBook ToCosmosDbBook(this Book book)
        => new ()
        {
            Id = book.Isbn,
            Title = book.Title,
            Author = book.Author,
            ShortDescription = book.ShortDescription,
            PageCount = book.PageCount,
            ReleaseDate = book.ReleaseDate
        };
}