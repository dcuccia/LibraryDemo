using System.Reflection;

namespace Library.WebApi.Services;

public static class BookServiceExtensions
{
    public static void AddBookServicesForAssemblyContaining<TMarker>(this IServiceCollection services, IConfiguration configuration)
        => AddBookServicesForAssemblyContaining(services, typeof(TMarker), configuration);
    
    public static void AddBookServicesForAssemblyContaining(this IServiceCollection services,
        Type typeMarker, IConfiguration configuration)
    {
        var endpointTypes = GetBookServiceTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IBookService.AddServices))!
                .Invoke(null, new object[] {services, configuration});
        }
    }
    
    private static IEnumerable<TypeInfo> GetBookServiceTypesFromAssemblyContaining(Type typeMarker)
        => typeMarker.Assembly.DefinedTypes
            .Where(x => !x.IsAbstract && !x.IsInterface &&
                        typeof(IBookService).IsAssignableFrom(x));
}