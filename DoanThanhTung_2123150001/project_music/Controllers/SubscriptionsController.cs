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

        // 1. LẤY DANH SÁCH GÓI CƯỚC (Ai cũng xem được)
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            try
            {
                var result = await _subscriptionService.GetAllPlansAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. TẠO GIAO DỊCH VÀ LẤY LINK MOMO (Phải đăng nhập)
        [HttpPost("transactions")]
        [Authorize]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized(new { message = "Vui lòng đăng nhập." });

                var result = await _subscriptionService.CreateTransactionAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 3. REACT GỌI ĐỂ XÁC NHẬN SAU KHI QUÉT MÃ XONG (Phải đăng nhập)
        [HttpPost("transactions/{transactionId}/confirm")]
        [Authorize]
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

        // 4. (TÙY CHỌN BỔ SUNG) WEBHOOK CHO MOMO GỌI VỀ SAU LƯNG
        // Khi lên môi trường thật, MoMo sẽ gọi ngầm vào API này để báo kết quả (IPN)
        [HttpPost("momo-ipn")]
        public IActionResult MoMoIpn([FromBody] object requestData)
        {
            // Trong thực tế: 
            // 1. Boss sẽ lấy requestData MoMo gửi về.
            // 2. Kiểm tra chữ ký (Signature) xem có đúng MoMo thật không.
            // 3. Nếu resultCode == 0 thì gọi _subscriptionService.ConfirmPaymentAsync(orderId).

            // Hiện tại ở môi trường Dev localhost, trả về OK (204) để MoMo biết mình đã nhận được tin báo.
            return NoContent();
        }
    }
}