using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Subscriptions;
using project_music.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace project_music.Services.Subscriptions
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly MusicDbContext _context;

        public SubscriptionService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanResponse>> GetAllPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Select(p => new PlanResponse
                {
                    PlanId = p.PlanId,
                    Name = p.Name,
                    Price = p.Price,
                    DurationDays = p.DurationDays,
                    Features = p.Features
                })
                .ToListAsync();
        }

        public async Task<TransactionResponse> CreateTransactionAsync(string userId, CreateTransactionRequest request)
        {
            // 1. Kiểm tra gói cước có tồn tại không
            var plan = await _context.SubscriptionPlans.FindAsync(request.PlanId);
            if (plan == null) throw new Exception("Gói cước không tồn tại.");

            // 2. Tạo hóa đơn mới (PENDING)
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                UserId = userId,
                PlanId = request.PlanId,
                Amount = plan.Price,
                PaymentGateway = "MOMO", // Ép luôn thanh toán qua MoMo
                Status = "PENDING",
                PaidAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // 3. GỌI API MOMO LẤY LINK MỞ MÃ QR
            string payUrl = await GenerateMoMoPaymentUrl(transaction.TransactionId, plan.Price, plan.Name);

            return new TransactionResponse
            {
                TransactionId = transaction.TransactionId,
                PlanName = plan.Name,
                Amount = transaction.Amount,
                PaymentGateway = transaction.PaymentGateway,
                Status = transaction.Status,
                PaidAt = transaction.PaidAt,
                PayUrl = payUrl // 👉 Trả link này về cho React
            };
        }

        public async Task<bool> ConfirmPaymentAsync(string transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Plan)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) throw new Exception("Không tìm thấy giao dịch.");
            if (transaction.Status == "SUCCESS") throw new Exception("Giao dịch này đã được xử lý rồi.");

            var user = await _context.Users.FindAsync(transaction.UserId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

            // XỬ LÝ LOGIC NÂNG CẤP VIP
            transaction.Status = "SUCCESS";
            user.IsPremium = true;

            if (user.PremiumExpiryDate != null && user.PremiumExpiryDate > DateTime.UtcNow)
            {
                user.PremiumExpiryDate = user.PremiumExpiryDate.Value.AddDays(transaction.Plan.DurationDays);
            }
            else
            {
                user.PremiumExpiryDate = DateTime.UtcNow.AddDays(transaction.Plan.DurationDays);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================================
        // 🚀 HÀM PHỤ TRỢ: GỌI API MOMO ĐỂ LẤY LINK QUÉT MÃ QR
        // ==========================================================
        private async Task<string> GenerateMoMoPaymentUrl(string orderId, decimal amount, string planName)
        {
            // THÔNG TIN TEST TỪ TÀI LIỆU CỦA MOMO (Boss thay bằng key thật của Boss sau này nếu lên thật)
            string partnerCode = "MOMO5RGX20191128";
            string accessKey = "M8brj9K6E22vXoDB";
            string secretKey = "nqQiVSgDMy809JoPF6OzP5OdBUB550Y4";
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";

            string orderInfo = $"Thanh toan VIP Am Vang: {planName}";
            string redirectUrl = "http://localhost:5173/payment-success"; // Web React của Boss khi nạp xong nó nhảy về đây
            string ipnUrl = "https://localhost:7020/api/Subscriptions/momo-ipn"; // Webhook cho MoMo gọi về Backend C#
            string requestType = "captureWallet"; // Bắt buộc là captureWallet để hiện mã QR
            string amountStr = ((long)amount).ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // 1. Tạo chuỗi ký tự chuẩn theo thứ tự A-Z MoMo yêu cầu
            string rawHash = $"accessKey={accessKey}&amount={amountStr}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

            // 2. Dùng thuật toán HMAC SHA256 để ký bảo mật
            string signature = ComputeHmacSha256(rawHash, secretKey);

            // 3. Đóng gói JSON gửi sang Google
            var message = new
            {
                partnerCode = partnerCode,
                partnerName = "Am Vang Premium",
                storeId = "AmVangStore",
                requestId = requestId,
                amount = amountStr,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = redirectUrl,
                ipnUrl = ipnUrl,
                lang = "vi",
                extraData = extraData,
                requestType = requestType,
                signature = signature
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var document = JsonDocument.Parse(responseString);
                // Bóc cái payUrl từ kết quả MoMo trả về
                if (document.RootElement.TryGetProperty("payUrl", out JsonElement payUrlElement))
                {
                    return payUrlElement.GetString() ?? "";
                }
            }

            // Nếu mạng lỗi hoặc config sai, trả về link mặc định hoặc rỗng
            return "";
        }

        // ==========================================================
        // 🚀 HÀM PHỤ TRỢ: THUẬT TOÁN BĂM CHỮ KÝ HMAC SHA256
        // ==========================================================
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}