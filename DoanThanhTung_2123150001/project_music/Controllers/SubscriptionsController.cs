using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Subscriptions;
using project_music.Services.Subscriptions;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        // Bất kỳ ai cũng có thể xem danh sách gói cước
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var result = await _subscriptionService.GetAllPlansAsync();
            return Ok(result);
        }

        // Tạo giao dịch (Phải đăng nhập)
        [HttpPost("transactions")]
        [Authorize]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _subscriptionService.CreateTransactionAsync(userId!, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Giả lập Webhook từ Momo/ZaloPay gọi về để báo thanh toán thành công
        // Trong thực tế API này không cần [Authorize] nhưng cần Verify Chữ ký (Signature)
        [HttpPost("transactions/{transactionId}/confirm")]
        public async Task<IActionResult> ConfirmPayment(string transactionId)
        {
            try
            {
                await _subscriptionService.ConfirmPaymentAsync(transactionId);
                return Ok(new { message = "Thanh toán thành công! Tài khoản của bạn đã được nâng cấp lên Premium. 👑" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}