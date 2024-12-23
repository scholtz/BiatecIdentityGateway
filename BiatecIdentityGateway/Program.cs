using AlgorandAuthentication;
using BiatecIdentityGateway.BusinessController;
using Grpc.Net.Client;
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

builder.Services.Configure<BiatecIdentityGateway.Model.Config>(
    builder.Configuration.GetSection("GatewayConfig"));

var algorandAuthenticationOptions = new AlgorandAuthenticationOptions();
builder.Configuration.GetSection("AlgorandAuthentication").Bind(algorandAuthenticationOptions);
builder.Services
 .AddAuthentication(AlgorandAuthenticationHandler.ID)
 .AddAlgorand(o =>
 {
     o.CheckExpiration = algorandAuthenticationOptions.CheckExpiration;
     o.Debug = algorandAuthenticationOptions.Debug;
     o.AlgodServer = algorandAuthenticationOptions.AlgodServer;
     o.AlgodServerToken = algorandAuthenticationOptions.AlgodServerToken;
     o.AlgodServerHeader = algorandAuthenticationOptions.AlgodServerHeader;
     o.Realm = algorandAuthenticationOptions.Realm;
     o.NetworkGenesisHash = algorandAuthenticationOptions.NetworkGenesisHash;
     o.MsPerBlock = algorandAuthenticationOptions.MsPerBlock;
     o.EmptySuccessOnFailure = algorandAuthenticationOptions.EmptySuccessOnFailure;
     o.EmptySuccessOnFailure = algorandAuthenticationOptions.EmptySuccessOnFailure;
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
app.Run();
