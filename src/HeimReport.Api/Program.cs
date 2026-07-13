using FluentValidation;
using FluentValidation.AspNetCore;
using HeimReport.Api.Data;
using HeimReport.Api.Email;
using HeimReport.Api.ExceptionHandlers;
using HeimReport.Api.Extensions;
using HeimReport.Api.Security;
using HeimReport.Api.Validators.Auth;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRepositories();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSecurityServices();
builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEmailSender(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(CorsServiceExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
