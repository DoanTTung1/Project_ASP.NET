using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Subscriptions
{
    // 1. Trả về thông tin gói cước
    public class PlanResponse
    {
        public string PlanId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string? Features { get; set; } // Trả về chuỗi JSON chứa đặc quyền
    }

    // 2. Nhận dữ liệu khi User bấm "Mua gói"
    public class CreateTransactionRequest
    {
        [Required]
        public string PlanId { get; set; } = null!;

        [Required]
        public string PaymentGateway { get; set; } = null!; // MOMO, ZALOPAY, STRIPE, PAYPAL
    }

    // 3. Trả về thông tin hóa đơn sau khi tạo
    public class TransactionResponse
    {
        public string TransactionId { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PaymentGateway { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? PaidAt { get; set; }
        public string? PayUrl { get; set; }
    }
}