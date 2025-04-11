using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; 
using SidoAgung.ApiSaga.Infrastruktur.Persistences;
using SidoAgung.ApiSaga.Infrastruktur.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SidoAgung.ApiSaga.Infrastruktur.Middleware;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

// Konfigurasi Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tambahkan Repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024; // Ukuran maksimum cache dalam bytes
    options.UseCaseSensitivePaths = true; 
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Tambahkan Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SidoAgung API SAGA Mobile", Version = "v1" });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseGlobalExceptionHandler();
app.UseCustomHttpsRedirection();
app.UseCustomCompression(); //Gzip Compresion
app.UseSecurityHeaders();
app.UseRequestLogging();
app.UseApiPerformanceTracking();

app.UseRateLimiter();
app.UseCustomRouting();
app.UseCors("AllowSpecificOrigin"); // Buat policy CORS
//app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
//app.UseJwtBlacklist();
app.UseExceptionHandler("/error"); // Endpoint error custom
app.UseWebSockets();
app.MapControllers();
//app.UseHsts();
app.Run();
