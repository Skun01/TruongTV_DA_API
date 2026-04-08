using API.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddCorsConfigurationExtension(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddApiBehaviorExtension();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOptionSettingsExtension(builder.Configuration);
builder.Services.AddAuthConfigurationExtension(builder.Configuration);
builder.Services.AddSwaggerExtension();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AppDbSeeder.SeedAsync(dbContext);
}

app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(CorsConfigurationExtension.FrontendPolicyName);

app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tài liệu API");
        c.RoutePrefix = string.Empty;
    });    
}
app.MapControllers();

app.Run();
