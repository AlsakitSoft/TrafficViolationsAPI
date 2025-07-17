//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using TrafficViolationsAPI.DTOs;
//using TrafficViolationsAPI.Services;

//namespace TrafficViolationsAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class NotificationsController : ControllerBase
//    {
//        private readonly IFirebaseService _firebaseService;
//        private readonly ILogger<NotificationsController> _logger;

//        public NotificationsController(IFirebaseService firebaseService, ILogger<NotificationsController> logger)
//        {
//            _firebaseService = firebaseService;
//            _logger = logger;
//        }

//        // POST: api/notifications/send - إرسال إشعار واحد
//        [HttpPost("send")]
//        [Authorize(Policy = "TrafficOfficerOrAdmin")]
//        public async Task<IActionResult> SendNotification(NotificationDto notificationDto)
//        {
//            try
//            {
//                var response = await _firebaseService.SendNotificationAsync(notificationDto);
//                return Ok(new { message = "تم إرسال الإشعار بنجاح", messageId = response });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error sending notification");
//                return StatusCode(500, new { message = "حدث خطأ أثناء إرسال الإشعار", error = ex.Message });
//            }
//        }

//        // POST: api/notifications/send-bulk - إرسال إشعارات متعددة
//        [HttpPost("send-bulk")]
//        [Authorize(Policy = "TrafficOfficerOrAdmin")]
//        public async Task<IActionResult> SendBulkNotification(BulkNotificationDto bulkNotificationDto)
//        {
//            try
//            {
//                var response = await _firebaseService.SendBulkNotificationAsync(bulkNotificationDto);

//                return Ok(new
//                {
//                    message = "تم إرسال الإشعارات",
//                    successCount = response.SuccessCount,
//                    failureCount = response.FailureCount,
//                    totalCount = bulkNotificationDto.Tokens.Count
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error sending bulk notifications");
//                return StatusCode(500, new { message = "حدث خطأ أثناء إرسال الإشعارات", error = ex.Message });
//            }
//        }

//        // POST: api/notifications/violation-created - إشعار بمخالفة جديدة
//        [HttpPost("violation-created")]
//        [Authorize(Policy = "TrafficOfficerOrAdmin")]
//        public async Task<IActionResult> SendViolationCreatedNotification([FromBody] ViolationNotificationRequest request)
//        {
//            try
//            {
//                var notificationDto = new NotificationDto
//                {
//                    Title = "مخالفة مرورية جديدة",
//                    Body = $"تم تسجيل مخالفة جديدة للمركبة {request.PlateNumber} - {request.ViolationType}",
//                    Token = request.UserToken,
//                    Data = new Dictionary<string, string>
//                    {
//                        { "type", "violation_created" },
//                        { "violationId", request.ViolationId.ToString() },
//                        { "plateNumber", request.PlateNumber },
//                        { "violationType", request.ViolationType }
//                    }
//                };

//                var response = await _firebaseService.SendNotificationAsync(notificationDto);
//                return Ok(new { message = "تم إرسال إشعار المخالفة بنجاح", messageId = response });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error sending violation notification");
//                return StatusCode(500, new { message = "حدث خطأ أثناء إرسال إشعار المخالفة", error = ex.Message });
//            }
//        }

//        // POST: api/notifications/payment-reminder - تذكير بدفع المخالفة
//        [HttpPost("payment-reminder")]
//        [Authorize(Policy = "TrafficOfficerOrAdmin")]
//        public async Task<IActionResult> SendPaymentReminderNotification([FromBody] PaymentReminderRequest request)
//        {
//            try
//            {
//                var notificationDto = new NotificationDto
//                {
//                    Title = "تذكير بدفع المخالفة",
//                    Body = $"لديك مخالفة غير مدفوعة للمركبة {request.PlateNumber} بقيمة {request.FineAmount} ريال",
//                    Token = request.UserToken,
//                    Data = new Dictionary<string, string>
//                    {
//                        { "type", "payment_reminder" },
//                        { "violationId", request.ViolationId.ToString() },
//                        { "plateNumber", request.PlateNumber },
//                        { "fineAmount", request.FineAmount.ToString() }
//                    }
//                };

//                var response = await _firebaseService.SendNotificationAsync(notificationDto);
//                return Ok(new { message = "تم إرسال تذكير الدفع بنجاح", messageId = response });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error sending payment reminder");
//                return StatusCode(500, new { message = "حدث خطأ أثناء إرسال تذكير الدفع", error = ex.Message });
//            }
//        }
//    }

//    public class ViolationNotificationRequest
//    {
//        public Guid ViolationId { get; set; }
//        public string PlateNumber { get; set; } = string.Empty;
//        public string ViolationType { get; set; } = string.Empty;
//        public string UserToken { get; set; } = string.Empty;
//    }

//    public class PaymentReminderRequest
//    {
//        public Guid ViolationId { get; set; }
//        public string PlateNumber { get; set; } = string.Empty;
//        public decimal FineAmount { get; set; }
//        public string UserToken { get; set; } = string.Empty;
//    }
//}