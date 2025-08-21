using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Modules.Security.Application.Authorization;

// Persistencia (EF)
using Persistence; // AppDbContext

// Users (Dominio / App / Infra)
using Modules.Users.Domain;
using Modules.Users.Application;
using Modules.Users.Infrastructure.Repositories;

// Security (Dominio / App / Infra)
using Modules.Security.Domain;                      
using Modules.Security.Application;                
using Modules.Security.Application.Auth;            
using Modules.Security.Infrastructure;              
using Modules.Security.Infrastructure.Repositories; 

// Shared (UoW)
using Shared.Application;    
using Shared.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Modules.Events.Domain;
using Modules.Events.Infrastructure;
using Modules.Tickets.Application;
using Modules.Tickets.Domain;
using Modules.Tickets.Infrastructure;
using Modules.Auditing.Domain;
using Modules.Auditing.Infrastructure;
using Modules.Auditing.Application; // EfUnitOfWork

var builder = WebApplication.CreateBuilder(args);

// ===== Controllers (Presentación) =====
builder.Services.AddControllers();

// ===== Swagger/OpenAPI =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Eventia API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token como: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));



// ===== Repositorios EF + Unit of Work =====
builder.Services.AddScoped<IUserRepository, UserRepositoryEF>();          // Users
builder.Services.AddScoped<IRoleRepository, RoleRepositoryEF>();          // Security
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepositoryEF>();  // Security
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();                  // UoW EF
builder.Services.AddScoped<IPermissionRepository, PermissionRepositoryEF>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddHttpContextAccessor();


// ===== Application Services =====
builder.Services.AddScoped<IUsersAppService, UsersAppService>();
builder.Services.AddScoped<IAuthAppService, AuthAppService>();
builder.Services.AddScoped<ITicketAppService, TicketAppService>();


// ===== JWT =====
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-super-secret-1234567890";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddSingleton<IJwtTokenService>(new JwtTokenService(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// ===== Autorización (Policies) =====
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddAuthorization(options =>
{
    // Ahora defines policies en base a permisos
    options.AddPolicy("MANAGE_USERS", p =>
        p.Requirements.Add(new PermissionRequirement("MANAGE_USERS")));

    options.AddPolicy("MANAGE_ROLES", p =>
        p.Requirements.Add(new PermissionRequirement("MANAGE_ROLES")));

    options.AddPolicy("MANAGE_TICKETS", p =>
        p.Requirements.Add(new PermissionRequirement("MANAGE_TICKETS")));

    options.AddPolicy("MANAGE_PERMISSIONS", p =>
       p.Requirements.Add(new PermissionRequirement("MANAGE_PERMISSIONS")));

    options.AddPolicy("MANAGE_EVENTS", p =>
        p.Requirements.Add(new PermissionRequirement("MANAGE_EVENTS")));
        
    options.AddPolicy("MANAGE_AUDIT", p => 
        p.Requirements.Add(new PermissionRequirement("MANAGE_AUDIT")));
});

var app = builder.Build();

// ===== Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


var hash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
Console.WriteLine("este es "+hash);

app.Run();
