using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_music.Models;
using project_music.Services.Admin;
using project_music.Services.AI;
using project_music.Services.Artists;
using project_music.Services.AudioFiles;
using project_music.Services.Auth;
using project_music.Services.Downloads;
using project_music.Services.Genres;
using project_music.Services.History;
using project_music.Services.Home;
using project_music.Services.Lyrics;
using project_music.Services.Playlists;
using project_music.Services.Search;
using project_music.Services.Social;
using project_music.Services.Songs;
using project_music.Services.Subscriptions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== CẤU HÌNH DATABASE ==================
var connectionString = builder.Configuration.GetConnectionString("MusicAppDb");
builder.Services.AddDbContext<MusicDbContext>(options =>
   options.UseMySql(connectionString,
    new MySqlServerVersion(new Version(8, 0, 32))));

// ================== CẤU HÌNH JWT ==================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev localhost
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// --- 1. MỚI THÊM: CẤU HÌNH CORS CHO WEB FRONTEND ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        // Cho phép các port phổ biến của React (3000) và Vite/Vue (5173) gọi API
        policy.AllowAnyOrigin()
  .AllowAnyHeader()
  .AllowAnyMethod();
    });
});
// ----------------------------------------------------

// ================== ĐĂNG KÝ SERVICES ==================
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IAudioFileService, AudioFileService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ILyricService, LyricService>();
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddHttpClient<IAiService, AiService>();

// ================== CONTROLLER + SWAGGER ==================
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================== PIPELINE ==================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thay thế app.UseStaticFiles(); bằng đoạn này:
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    }
});

// --- 2. MỚI THÊM: MỞ CỬA CORS ---
// Lưu ý: Phải đặt UseCors TRƯỚC UseAuthentication và UseAuthorization
app.UseCors("AllowWebFrontend");
// --------------------------------

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();