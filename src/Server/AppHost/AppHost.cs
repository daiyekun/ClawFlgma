var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ClawFlgma_AuthService>("clawflgma-authservice");

builder.AddProject<Projects.ClawFlgma_AuthService>("authservice");

builder.AddProject<Projects.ClawFlgma_ApiGateway>("apigateway");

builder.AddProject<Projects.ClawFlgma_UserService>("userservice");

builder.AddProject<Projects.ClawFlgma_ProjectService>("projectservice");

builder.AddProject<Projects.ClawFlgma_NotificationService>("notificationservice");

builder.AddProject<Projects.ClawFlgma_DesignService>("designservice");

builder.AddProject<Projects.ClawFlgma_CollaborationService>("collaborationservice");

builder.AddProject<Projects.ClawFlgma_AssetService>("assetservice");

builder.AddProject<Projects.ClawFlgma_AIService>("aiservice");

builder.Build().Run();
