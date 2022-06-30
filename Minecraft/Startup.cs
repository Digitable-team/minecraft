using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.Swagger;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using Minecraft.Data.Interfaces;
using Minecraft.Data.Services;
using System.Text;
using Minecraft.Data;

namespace Minecraft
{
    public class Startup
    {
        private IConfigurationRoot _confString;
        public Startup(IHostingEnvironment hostEnv)
        {
            _confString = new ConfigurationBuilder().SetBasePath(hostEnv.ContentRootPath).AddJsonFile("db_settings.json").Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                {
                    Version = "v1",
                    Title = "SuperMinecraft",
                    Description = "<h1>�������� Endpoint'��:</h1>  " +
                    "<h2><b>ThingApi</b></h2>" +
                    "<h3><b>Get: /api/things</b></h3>" +
                    "������ ��� ������ �� ������� Things �� 6 ���� �� ��������.\n" +
                    "��������� ������ ������� �� ����� ��������� ���:\n" +
                    "�������� \n" +
                    "����������\n" +
                    "����\n" +
                    "����\n" +
                    "<h3><b>Get: /api/things/getQr/id</b></h3>" +
                    "���������� Qr - ���, ������� �������� � ���� ������ � ���������� ��������� ������ �� ������� Things\n\n" +
                    "<h3><b>Put: /api/things/update/id </b></h3>" +
                    "��������� ��������� ������ � ������� Things" +
                    "<h3><b>Post: /api/things/new</b></h3>" +
                    "������ ����� ������ � ������� Things" +
                    "<h3><b>Delete: /api/things/delete/id</b></h3> " +
                    "������� ��������� ������ � ������� Things" +
                    "<h3><b>Delete: /api/things/DeleteQr</b></h3>" +
                    "������� Qr - ���" +
                    "<h3><b>Delete: /api/things/DeleteAll</b></h3>" +
                    "������� ��� ������ � ������� Things" +
                    "<h2><b>UserApi</b></h2>" +
                    "<h3><b>Get: /api/users</b></h3>" +
                    "���������� ��� ������ � ������� Users" +
                    "<h3><b>Get: /api/users/id</b></h3>" +
                    "���������� ��������� ������ �� ������� Users" +
                    "<h3><b>Put: /api/users/update/id</b></h3>" +
                    "��������� ��������� ������ � ������� Users" +
                    "<h3><b>Post: api/users/new</b></h3>" +
                    "��������� ����� ������ � ������� Users" +
                    "<h3><b>Delete: api/users/delete/id</b></h3>" +
                    "������� ��������� ������ �� ������� Users � ��� ������, ��������� � ���� ������� �� ������� Things",
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            services.AddSingleton <IUriService>(p =>
            {
                var accessor = p.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(uri);
            });

            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                }));

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddMvc();
            services.AddDbContext<AppDBContent>(options => options.UseSqlServer(_confString.GetConnectionString("DefaultConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseCors("CorsPolicy");
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });
            if (env.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.SerializeAsV2 = true;
                });
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API V1");
                });
            }

            using (var scope = app.ApplicationServices.CreateScope())
            {
                AppDBContent content = scope.ServiceProvider.GetRequiredService<AppDBContent>();
            }
        }
    }
}
