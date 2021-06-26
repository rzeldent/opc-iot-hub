using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace OpcIotHub
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IConfigurationAzureIotHub, ConfigurationOpcIotHub>();
                    services.AddTransient<IConfigurationMqtt, ConfigurationOpcIotHub>();
                    services.AddTransient<IConfigurationOpc, ConfigurationOpcIotHub>();
                    //services.AddTransient<ISampleSink, AzureIotHub.AzureSampleSink>();
                    services.AddTransient<ISampleSink, Mqtt.MqttSampleSink>();
                    //services.AddTransient<ISampleSink, Mocks.MockSampleSink>();
                    //services.AddTransient<ISampleSource, Opc.OpcSampleSource>();
                    services.AddTransient<ISampleSource, Mocks.MockSampleSource>();
                    services.AddHostedService<Worker>();
                })
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration));
           }
}
