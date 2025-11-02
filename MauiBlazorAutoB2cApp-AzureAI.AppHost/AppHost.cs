var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MauiBlazorAutoB2cApp>("mauiblazorautob2capp");

builder.Build().Run();
