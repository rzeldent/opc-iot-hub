using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpcIotHub;
using OpcIotHub.Interfaces;
using OpcIotHub.Services.Mqtt;
using OpcIotHub.Settings;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) => services
        .Configure<ConfigurationOpc>(context.Configuration.GetSection(ConfigurationOpc.Section))
        .Configure<ConfigurationAzureIotHub>(context.Configuration.GetSection(ConfigurationAzureIotHub.Section))
        .Configure<ConfigurationMqtt>(context.Configuration.GetSection(ConfigurationMqtt.Section))
        //.AddTransient<ISampleSink, AzureIotHub.AzureSampleSink>()
        .AddTransient<ISampleSink, MqttSampleSink>()
        //.AddTransient<ISampleSink, Mocks.MockSampleSink>()
        //.AddTransient<ISampleSource, Opc.OpcSampleSource>()
        .AddTransient<ISampleSource, OpcIotHub.Mocks.MockSampleSource>()
        .AddHostedService<Worker>()
        )
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
    )
    .Build()
    .Run();
