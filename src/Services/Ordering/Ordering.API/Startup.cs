using EventBus.Messages.Common;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Ordering.API.EventBusConsumer;
using Ordering.Application;
using Ordering.Infrastructure;

namespace Ordering.API
{
    public class Startup
    {
        public static IConfiguration StaticConfig { get; private set; }
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            StaticConfig = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationServices();
            services.AddInfrastructureServices(Configuration);

            services.AddControllers();

            //Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Catalog.API", Version = "v1" });
            });

            // register general services
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<BasketCheckoutConsumer>();

            // MassTransit-RabbitMQ configuration for consumer
            services.AddMassTransit(config =>
            {
                config.AddConsumer<BasketCheckoutConsumer>();

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(Configuration["EventBusSettings:HostAddress"]);
                    cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
                    {
                        c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
                    });
                });
            });
            services.AddMassTransitHostedService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            //app.MapControllers();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
