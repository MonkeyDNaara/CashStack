var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();

// builder.Services.AddControllers();
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();