
using blogpessoal.Configuration;
using blogpessoal.Data;
using blogpessoal.Model;
using blogpessoal.NovaPasta3;
using blogpessoal.Security.Implements;
using blogpessoal.Service;
using blogpessoal.Service.Implements;
using blogpessoal.Validator;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace blogpessoal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            //Conex�o com o banco de dados
            if (builder.Configuration["Enviroment:Start"] == "PROD")
            {
                builder.Configuration
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("secrets.json");

                var connectionString = builder.Configuration
                .GetConnectionString("ProdConnection");

                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString)
                );
            }
            else
            {
                var connectionString = builder.Configuration
                    .GetConnectionString("DefaultConnection");

                builder.Services.AddDbContext<AppDbContext> (options =>
                    options.UseSqlServer(connectionString)
                );
            }


            // Registrar a valida��o das Entidades
            builder.Services.AddTransient<IValidator<Postagem>, PostagemValidator>();
            builder.Services.AddTransient<IValidator<Tema>, TemaValidator>();
            builder.Services.AddTransient<IValidator<User>, UserValidator>();


            // Regsitrar as classes de servi�o (Service)
            builder.Services.AddScoped<IPostagemService, PostagemService>();
            builder.Services.AddScoped<ITemaService, TemaService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(Settings.Secret);
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            //Configura��o do Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                //Informa��es do projeto e do dev que desenvolveu
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Projeto Blog Pessoal",
                    Description = "Projeto blog pessoal - ASP.NET CORE 7.0",
                    Contact = new OpenApiContact
                    {
                        Name = "Jo�o Pedro da Maia Moura",
                        Email = "joaopedrodamaia04@gmail.com",
                        Url = new Uri("https://github.com/JP328"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "GitHub",
                        Url = new Uri("https://github.com/JP328")
                    }
                });
                
                //Configura��o de seguran�a no Swagger
                options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Digite um token JWT v�lido",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                //Adicionar a indica��o de endponit protegido
                options.OperationFilter<AuthResponsesOperationFilter>();
            });

            //Adicionar o FluentValidation no Swagger
            builder.Services.AddFluentValidationRulesToSwagger();

            //configura��o do CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolice",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            //Criar o banco de dados e as tabelas automaticamente
            using(var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }


            app.UseDeveloperExceptionPage();

            // Habilitar o Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            //Abrir o Swagger como p�gina inicial na nuvem, quando em produ��o
            if (app.Environment.IsProduction())
            {
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog Pessoal - V1");
                    options.RoutePrefix = string.Empty;
                });
            }
            
            //Inicializa o CORT
            app.UseCors("MyPolicy");

            //Habilitar a autentica��o e autoriza��o
            app.UseAuthentication();

            app.UseAuthorization();

            //Habilitar Controller
            app.MapControllers();

            app.Run();
        }
    }
}