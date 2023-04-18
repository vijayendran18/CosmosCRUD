using CRUD.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Configuration;
using System.Net.Http;


IConfiguration _config =null;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostContext, config) =>
    {

        config.AddJsonFile("local.settings.json", optional: true);
        //config.AddEnvironmentVariables(prefix: "PREFIX_");
        config.AddCommandLine(args);
        _config = config.Build();

    })
    .ConfigureServices(Services =>
    {

        //Source DB 
        SourceCosmosConfiguration sourceCosmosConfiguration = _config.GetSection("SourceCosmosConfiguration").Get<SourceCosmosConfiguration>();
        Services.AddSingleton<SourceCosmosConfiguration>(sourceCosmosConfiguration);
        Services.AddSingleton(new SourceCosmosClient(sourceCosmosConfiguration));

        //Dest DB
        DestinationCosmosConfiguration destCosmosConfiguration = _config.GetSection("DestCosmosConfiguration").Get<DestinationCosmosConfiguration>();
        Services.AddSingleton<DestinationCosmosConfiguration>(destCosmosConfiguration);
        Services.AddSingleton(new DestinationCosmosClient(destCosmosConfiguration));

    })
    .Build();


host.Run();
