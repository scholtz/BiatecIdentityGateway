using AlgorandAuthentication;
using AlgorandAuthenticationV2;
using BiatecIdentityGateway.BusinessController;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Biatec Identity Gateway",
        Version = "v1",
        Description = File.ReadAllText("doc/readme.md")
    });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "ARC-0014 Algorand authentication transaction",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });
    c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

});
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<Gateway>();
builder.Services.AddSingleton<SecurityController>();
builder.Services.AddSingleton<DocumentVerification>();

builder.Services.Configure<BiatecIdentityGateway.Model.Config>(
    builder.Configuration.GetSection("GatewayConfig"));

var authOptions = builder.Configuration.GetSection("AlgorandAuthentication").Get<AlgorandAuthenticationOptionsV2>();
if (authOptions == null) throw new Exception("Config for the authentication is missing");
builder.Services.AddAuthentication(AlgorandAuthenticationHandlerV2.ID).AddAlgorand(a =>
{
    a.Realm = authOptions.Realm;
    a.CheckExpiration = authOptions.CheckExpiration;
    a.EmptySuccessOnFailure = authOptions.EmptySuccessOnFailure;
    a.AllowedNetworks = authOptions.AllowedNetworks;
    a.Debug = authOptions.Debug;
});

// Add CORS policy
var corsConfig = builder.Configuration.GetSection("Cors").AsEnumerable().Select(k => k.Value).Where(k => !string.IsNullOrEmpty(k)).ToArray();
if (!(corsConfig?.Length > 0)) throw new Exception("Cors not defined");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    builder =>
    {
        builder.WithOrigins(corsConfig)
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders("rowcount", "rowstate");
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
// if debugging locally, run "docker run -p 50051:50051 scholtz2/derec-crypto-core-grpc" to run the cryptography primitives
using (var channel = GrpcChannel.ForAddress("http://localhost:50051"))
{
    // this tests the connection to the derec crypto service and fails to start the app if the connection does not exists. it also allows to create new app configuration if not defined
    var client = new DerecCrypto.DeRecCryptographyService.DeRecCryptographyServiceClient(channel);
    var encKeys = client.EncryptGenerateEncryptionKey(new DerecCrypto.EncryptGenerateEncryptionKeyRequest());

    app.Logger.LogInformation($"New enc keys: PK: {encKeys.PublicKey.ToBase64()} SK: {encKeys.PrivateKey.ToBase64()} ");
    var signKeys = client.SignGenerateSigningKey(new DerecCrypto.SignGenerateSigningKeyRequest());
    app.Logger.LogInformation($"New sign keys: PK: {signKeys.PublicKey.ToBase64()} SK: {signKeys.PrivateKey.ToBase64()} ");
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// preload the services
_ = app.Services.GetService<Gateway>();
_ = app.Services.GetService<SecurityController>();
_ = app.Services.GetService<DocumentVerification>();

app.Run();
