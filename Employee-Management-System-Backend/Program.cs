

using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register RoleRepository as a singleton or scoped (recommended: scoped)
builder.Services.AddScoped<RoleRepository>();


// ✅ Register EmployeeRepository for DI
builder.Services.AddScoped<EmployeeRepository>();


builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<DepartmentEmployeeRepository>();


builder.Services.AddControllers();


// Add LeaveRepo

builder.Services.AddScoped<LeaveRepository>();

// Register PayslipRepository for dependency injection
builder.Services.AddScoped<PayslipRepository>();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure uploads directory path
builder.Services.Configure<PayslipUploadSettings>(options =>
{
    options.UploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Payslips");
});

var app = builder.Build();

// Ensure uploads directory exists
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Payslips");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable serving static files
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
