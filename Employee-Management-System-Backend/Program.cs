using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Employee_Management_System_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== Register Repositories =====
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<DepartmentEmployeeRepository>();
builder.Services.AddScoped<LeaveRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<IPayslipRepository, PayslipRepository>();

// ===== Register Services =====
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// ===== Configure Upload Paths =====
var rootDir = Directory.GetCurrentDirectory();
var wwwRoot = Path.Combine(rootDir, "wwwroot");
var employeesPath = Path.Combine(wwwRoot, "uploads", "employees");
var payslipsPath = Path.Combine(rootDir, "Uploads", "Payslips");

// Ensure directories exist
Directory.CreateDirectory(employeesPath);
Directory.CreateDirectory(payslipsPath);

builder.Services.Configure<EmployeeUploadSettings>(options =>
{
    options.UploadsPath = employeesPath;
});

builder.Services.Configure<PayslipUploadSettings>(options =>
{
    options.UploadsPath = payslipsPath;
});

// ===== Configure CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== JWT Authentication =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourDefaultSecretKeyHere")),
        ClockSkew = TimeSpan.Zero
    };
});

// ===== Authorization Policies =====
// FIXED: Use AddAuthorizationBuilder() to resolve ASP0025 warning
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("HROrAdmin", policy => policy.RequireRole("HR", "Admin"))
    .AddPolicy("AllEmployees", policy => policy.RequireRole("Employee", "HR", "Admin"));

// ===== Controllers with Global Authorization =====
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Management API",
        Version = "v1",
        Description = "API for Employee Management System with JWT Authentication"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== HTTP Request Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAngularApp");
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
