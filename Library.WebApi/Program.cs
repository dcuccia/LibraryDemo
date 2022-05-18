using FluentValidation;
using Library.WebApi;
using Library.WebApi.Endpoints.Internal;
using Library.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddBookServicesForAssemblyContaining<IApiMarker>(builder.Configuration);
builder.Services.AddEndpointsForAssemblyContaining<IApiMarker>(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<IApiMarker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseEndpointsForAssemblyContaining<Program>();

app.Run();