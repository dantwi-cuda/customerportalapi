using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CCICustomerPortalApi>("CustomerPoralApi");

builder.Build().Run();
