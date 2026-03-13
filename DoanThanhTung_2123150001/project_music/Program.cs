using Microsoft.EntityFrameworkCore;
using project_music.Models;
using project_music.Services.Artists;
using project_music.Services.Genres;
using project_music.Services.AudioFiles;
using project_music.Services.Songs;
var builder = WebApplication.CreateBuilder(args);

// ---CẤU HÌNH KẾT NỐI DATABASE ---
var connectionString = builder.Configuration.GetConnectionString("MusicAppDb");
builder.Services.AddDbContext<MusicDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// --------------------------------------------------

// Đăng ký Service vào DI Container
builder.Services.AddScoped<IArtistService, ArtistService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IAudioFileService, AudioFileService>();
builder.Services.AddScoped<ISongService, SongService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();   

app.UseAuthorization();

app.MapControllers();

app.Run();