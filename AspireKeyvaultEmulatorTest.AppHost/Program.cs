using Aspire.Hosting;
using AzureKeyVaultEmulator.Aspire.Hosting;
using WireMock.Server;
using WireMock.Settings;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Api>("api");

if (Environment.GetEnvironmentVariable("NOKV") != "true")
{
  var kv = builder.AddAzureKeyVault("kv").RunAsEmulator(new KeyVaultEmulatorOptions()
  {
    Persist = false,
    Lifetime = ContainerLifetime.Session,
    ShouldGenerateCertificates = true,
  });

  api.WithReference(kv).WaitFor(kv);
}

WireMockServer wireMockServer = WireMockServer.Start(new WireMockServerSettings
{
  UseSSL = true
});

wireMockServer.Given(WireMock.RequestBuilders.Request.Create()
  .WithPath("/test")
  .UsingGet())
  .RespondWith(WireMock.ResponseBuilders.Response.Create()
  .WithStatusCode(200)
  .WithBody("Hello from aspire"));

api.WithEnvironment("wiremock", wireMockServer.Url);

builder.Build().Run();
