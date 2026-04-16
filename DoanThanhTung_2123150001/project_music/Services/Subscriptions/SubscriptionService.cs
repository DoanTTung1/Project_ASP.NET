using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Subscriptions;
using project_music.Models;

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
                PaymentGateway = request.PaymentGateway,
                Status = "PENDING",
                PaidAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new TransactionResponse
            {
                TransactionId = transaction.TransactionId,
                PlanName = plan.Name,
                Amount = transaction.Amount,
                PaymentGateway = transaction.PaymentGateway,
                Status = transaction.Status,
                PaidAt = transaction.PaidAt
            };
        }

        public async Task<bool> ConfirmPaymentAsync(string transactionId)
        {
            // 1. Lấy hóa đơn kèm thông tin gói cước
            var transaction = await _context.Transactions
                .Include(t => t.Plan) // Phải Join bảng để lấy số ngày (DurationDays)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) throw new Exception("Không tìm thấy giao dịch.");
            if (transaction.Status == "SUCCESS") throw new Exception("Giao dịch này đã được xử lý rồi.");

            // 2. Lấy thông tin User
            var user = await _context.Users.FindAsync(transaction.UserId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

            // 3. XỬ LÝ LOGIC NÂNG CẤP VIP
            transaction.Status = "SUCCESS"; // Đổi trạng thái hóa đơn
            user.IsPremium = true; // Lên đời VIP

            // Cộng dồn ngày sử dụng nếu họ đang là VIP sẵn, nếu không thì tính từ hôm nay
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
    }
}