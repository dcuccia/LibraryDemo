using Microsoft.Azure.CosmosRepository;
using SQLite;

namespace Library.WebApi.Models;

public record Book
{
    [PrimaryKey]
    public string Isbn { get; set; } = "";
    public string Title { get; init; } = "";
    public string Author { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public int PageCount { get; init; } = 0;
    public DateTime ReleaseDate { get; init; } = DateTime.Today;
}