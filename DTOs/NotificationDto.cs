using System.ComponentModel.DataAnnotations;

namespace TrafficViolationsAPI.DTOs
{
    public class NotificationDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        public Dictionary<string, string>? Data { get; set; }
    }

    public class BulkNotificationDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public List<string> Tokens { get; set; } = new List<string>();

        public Dictionary<string, string>? Data { get; set; }
    }
}
