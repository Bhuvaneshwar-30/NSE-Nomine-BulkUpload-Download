using WebApplication1.Model;
using WebApplication1.Services;
var builder = WebApplication.CreateBuilder(args);




builder.Services.AddScoped<Achfileservice>();

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


builder.Services.AddControllers();
builder.Services.Configure<filePath>(builder.Configuration.GetSection("AppSettings:filePath"));
     

builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalhost4200", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
   });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowLocalhost4200");

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
