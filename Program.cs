using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add Services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<art_gallery_api.Persistence.IArtefactDataAccess, art_gallery_api.Persistence.ArtefactDataAccess>();
builder.Services.AddScoped<art_gallery_api.Persistence.IArtistDataAccess, art_gallery_api.Persistence.ArtistDataAccess>();
builder.Services.AddScoped<art_gallery_api.Persistence.IArtTypeDataAccess, art_gallery_api.Persistence.ArtTypeDataAccess>();
builder.Services.AddScoped<art_gallery_api.Persistence.ISourceLocationDataAccess, art_gallery_api.Persistence.SourceLocationDataAccess>();
builder.Services.AddScoped<art_gallery_api.Persistence.IUserDataAccess, art_gallery_api.Persistence.UserDataAccess>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    // authorship
    Title = "Art Gallery API",
    Description = "Backend service to provide resources for aboriginal art gallery.",
    Contact = new OpenApiContact
    {
      Name = "Betty Yuliani",
      Email = "betty_yuliani@hotmail.com"
    },
  });
  // generates xml comments file
  var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  // pass the file into open API generator
  // open API generator gets the comments out and use them in the documentation
  options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Name = "Authorization",
    Description = "Bearer Authentication with JWT Token",
    Type = SecuritySchemeType.Http
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Id = "Bearer",
          Type = ReferenceType.SecurityScheme
        }
      },
      new List<String>()
    }
  });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters()
  {
    ValidateActor = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
  };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger();
// app.UseSwaggerUI();
app.UseSwaggerUI(setup => setup.InjectStylesheet("/styles/theme-betty.css"));

app.UseHttpsRedirection();

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();
app.Run();
