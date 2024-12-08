﻿using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudEvents.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureServiceBusClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAzureClients(
            clientBuilder =>
            {
                clientBuilder.AddServiceBusClientWithNamespace(configuration["AzureServiceBus:Namespace"])
                    .WithCredential(
                        new ClientSecretCredential(
                            configuration["AzureAD:TenantId"],
                            configuration["AzureAD:ClientId"],
                            configuration["AzureAD:ClientSecret"]));
            });

        return services;
    }
}