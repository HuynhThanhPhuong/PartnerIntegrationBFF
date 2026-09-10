var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/partnerverification/{partnerId}", (string partnerId) =>
{
    if (Random.Shared.Next(100) < 30)
    {
        throw new TimeoutException($"Partner verification service timed out for partner '{partnerId}'.");
    }

    return Results.Ok(new { partnerId, isValid = true });
});

app.Run();