using project_music.DTOs.Subscriptions;

namespace project_music.Services.Subscriptions
{
    public interface ISubscriptionService
    {
        Task<List<PlanResponse>> GetAllPlansAsync();

        // Tạo hóa đơn chờ thanh toán
        Task<TransactionResponse> CreateTransactionAsync(string userId, CreateTransactionRequest request);

        // (Giả lập) Hệ thống thanh toán gọi về báo thành công để nâng cấp tài khoản
        Task<bool> ConfirmPaymentAsync(string transactionId);
    }
}