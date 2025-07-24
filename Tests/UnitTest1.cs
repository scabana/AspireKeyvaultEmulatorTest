using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        using var testingBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireKeyvaultEmulatorTest_AppHost>();
        using var app = await testingBuilder.BuildAsync();

        await app.StartAsync();

        DistributedApplicationModel applicationModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        ResourceNotificationService resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();

        await resourceNotificationService.WaitForResourceAsync("api");

        Uri uri = app.GetEndpoint("api", "https");

        HttpClient httpClient = new HttpClient();

        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(uri + "weatherforecast");

        Assert.Equal(System.Net.HttpStatusCode.OK, httpResponseMessage.StatusCode);

        string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

        Assert.Contains("Hello from aspire", responseBody);

    }
}
