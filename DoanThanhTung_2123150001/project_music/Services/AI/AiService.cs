using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using project_music.Models;

namespace project_music.Services.AI
{
    public interface IAiService
    {
        Task<string> GetChatbotResponseAsync(string userMessage);
    }

    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly MusicDbContext _context; // 👉 GỌI DATABASE VÀO ĐÂY

        private const string API_KEY = "AIzaSyCwowDX_ge6M4eDPBr2OPQ6gJYIt0HNDSo";
        private const string GEMINI_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key=" + API_KEY;

        // 👉 Nhớ Inject thêm DB Context vào Constructor
        public AiService(HttpClient httpClient, MusicDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<string> GetChatbotResponseAsync(string userMessage)
        {
            // 1. MÓC DỮ LIỆU TỪ DATABASE (Lấy top 50 nghệ sĩ và bài hát làm vốn liếng cho AI)
            // 👉 CODE MỚI ĐÃ SỬA LỖI:
            var artists = await _context.Artists.Where(a => a.IsDeleted == false).Select(a => new { a.ArtistId, a.Name }).Take(50).ToListAsync();
            var songs = await _context.Songs.Where(s => s.IsDeleted == false).Select(s => new { s.Title }).Take(50).ToListAsync();

            var artistList = string.Join(", ", artists.Select(a => $"{a.Name} (Link: /artist/{a.ArtistId})"));
            var songList = string.Join(", ", songs.Select(s => s.Title));

            // 2. DẠN DÒ AI TRƯỚC KHI NÓ TRẢ LỜI
            string systemPrompt = $@"Bạn là 'Trợ Lý Âm Vang' - một chuyên gia âm nhạc thông minh của ứng dụng Âm Vang.
            Hãy tư vấn, gợi ý nhạc dựa MỘT CÁCH CHÍNH XÁC vào dữ liệu hệ thống đang có dưới đây:
            - NGHỆ SĨ HIỆN CÓ: {artistList}
            - BÀI HÁT HIỆN CÓ: {songList}
            
            QUY TẮC TẠO LINK (BẮT BUỘC):
            1. Khi nhắc đến tên Nghệ sĩ có trong danh sách trên, PHẢI chèn link theo cú pháp Markdown: [Tên Nghệ Sĩ](/artist/ID_Nghệ_Sĩ)
            2. Khi nhắc đến Bài hát, hãy gắn link Tìm kiếm: [Tên Bài Hát](/search?q=Tên_Bài_Hát)
            3. Nếu người dùng hỏi bài hát/nghệ sĩ KHÔNG CÓ trong danh sách trên, hãy xin lỗi và nói hệ thống chưa cập nhật, sau đó gợi ý bài khác trong danh sách.
            4. Phản hồi ngắn gọn, thân thiện, sành điệu.";

            // 3. ĐÓNG GÓI JSON GỬI CHO GOOGLE
            var payload = new
            {
                // Dùng system_instruction để định hình tính cách và kiến thức cho AI
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(GEMINI_URL, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Gemini từ chối phản hồi: {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseString);

            var textResponse = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textResponse ?? "Trợ lý đang bận, Boss quay lại sau nhé!";
        }
    }
}