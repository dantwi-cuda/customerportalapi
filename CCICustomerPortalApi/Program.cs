using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using AutoMapper;
using System.Reflection;
using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Services;
using CCICustomerPortalApi.Middleware;
using CCICustomerPortalApi.Authorization;
using CCICustomerPortalApi.Validators;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure OpenAPI/Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CCI Customer Portal API",
        Version = "v1",
        Description = "API for managing customer workspaces, shops, and Power BI reports",
        Contact = new OpenApiContact
        {
            Name = "CCI Support",
            Email = "support@cci.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configure JWT Authentication and Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.Audience = builder.Configuration["Authentication:Audience"];
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

    // Configure token validation parameters to include tenant claims
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Authority"],
        ValidAudience = builder.Configuration["Authentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Authentication:JwtKey"] ?? throw new InvalidOperationException("JWT key not configured")))
    };

    // Add event handler to map tenant claims to the current user principal
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            // Add tenant claims to the user's identity if they exist in the token
            var tenantIdClaim = context.Principal?.FindFirst("TenantId");
            var tenantSubdomainClaim = context.Principal?.FindFirst("TenantSubdomain");

            if (tenantIdClaim != null && tenantSubdomainClaim != null)
            {
                // Tenant claims are already present in the token, no additional action needed
                return;
            }

            // Optionally, you could add fallback code here to extract tenant info
            // from other sources if not in the token, but that would be less secure
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireMultiTenant", policy =>
        policy.Requirements.Add(new MultiTenantRequirement()));

    options.AddPolicy("RequireCCIUser", policy =>
        policy.RequireClaim("IsCCIUser", "true"));

    options.AddPolicy("RequireCustomerAdmin", policy =>
        policy.RequireRole("CustomerAdmin")
             .AddRequirements(new MultiTenantRequirement()));
});

// Register authorization handler
builder.Services.AddScoped<IAuthorizationHandler, MultiTenantAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();

// Add Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity Framework with custom user model
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Configure PowerBI Authentication
builder.Services.Configure<PowerBiAuthenticationOptions>(
    builder.Configuration.GetSection("PowerBI"));

// Configure HttpClient for PowerBI service
builder.Services.AddHttpClient<IPowerBiService, PowerBiService>();

// Add validators
builder.Services.AddScoped<IValidator<PowerBiAuthenticationOptions>, PowerBiAuthenticationOptionsValidator>();
builder.Services.AddScoped<IValidator<ReportDto>, ReportDtoValidator>();

// Add custom services
builder.Services.AddSingleton<ITenantService, TenantService>();  // CHANGED: Now a singleton for development
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IPowerBiService, PowerBiService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CCI Customer Portal API V1");
        c.RoutePrefix = "api-docs";
    });
    app.UseDeveloperExceptionPage();
}

// Essential middleware for establishing request context
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Register basic middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ConfigurationValidationMiddleware>();

// Add AdminPortalMiddleware to handle admin portal access from the main domain
app.UseMiddleware<AdminPortalMiddleware>();

// Use different tenant middleware based on environment
if (app.Environment.IsDevelopment())
{
    // In development, use the simplified DevTenantMiddleware that doesn't try to lookup tenants by subdomain
    app.UseMiddleware<DevTenantMiddleware>();
}
else
{
    // In production, use the full TenantMiddleware that looks up tenants by subdomain
    app.UseMiddleware<TenantMiddleware>();
}

// Add audit logging after tenant is resolved
app.UseMiddleware<AuditLoggingMiddleware>();

// Map controllers at the end of the pipeline
app.MapControllers();

app.Run();
