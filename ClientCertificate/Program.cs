using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(s =>
    {
        s.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.RequireCertificate;
        s.ClientCertificateValidation = (certificate, chain, errors) =>
        {
            if (errors.HasFlag(SslPolicyErrors.None))
            {
                X509Certificate2 caCertificate = GetCACertificate();
                if (certificate.Issuer == caCertificate.Subject)
                {
                    // Verify chain of trust and other validation checks
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Optional: Customize revocation check
                    if (chain.Build(certificate))
                    {
                        // Certificate is valid
                        return true;
                    }
                }
            }

            return false;
        };
    });
});

X509Certificate2 GetCACertificate()
{
    // Load your CA certificate from a file or other source
    // Replace this with actual code to load your CA certificate
    byte[] certData = File.ReadAllBytes("ca.crt");
    return new X509Certificate2(certData);
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}