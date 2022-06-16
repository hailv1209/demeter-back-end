var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentEmail(builder.Configuration.GetValue<string>("Mail:DefaultFrom"))
                .AddRazorRenderer()
                .AddSmtpSender(builder.Configuration.GetValue<string>("Mail:Server"), 587, builder.Configuration.GetValue<string>("Mail:User"), builder.Configuration.GetValue<string>("Mail:HashPassword"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x => x.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
