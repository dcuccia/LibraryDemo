using System.Reflection;

namespace Library.WebApi.Endpoints.Internal;

public static class EndpointExtensions
{
    public static void AddEndpointsForAssemblyContaining<TMarker>(this IServiceCollection services, IConfiguration configuration)
        => AddEndpointsForAssemblyContaining(services, typeof(TMarker), configuration);
    
    public static void AddEndpointsForAssemblyContaining(this IServiceCollection services,
        Type typeMarker, IConfiguration configuration)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!
                .Invoke(null, new object[] {services, configuration});
        }
    }

    public static void UseEndpointsForAssemblyContaining<TMarker>(this IApplicationBuilder app)
        => UseEndpointsForAssemblyContaining(app, typeof(TMarker));
    public static void UseEndpointsForAssemblyContaining(this IApplicationBuilder app, Type typeMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);
        
        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                .Invoke(null, new object[] {app});
        }
    }
    
    private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
        => typeMarker.Assembly.DefinedTypes
            .Where(x => !x.IsAbstract && !x.IsInterface &&
                        typeof(IEndpoints).IsAssignableFrom(x));
}