using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using TrafficViolationsAPI.DTOs;

namespace TrafficViolationsAPI.Services
{
    public interface IFirebaseService
    {
        Task<string> SendNotificationAsync(NotificationDto notificationDto);
        Task<BatchResponse> SendBulkNotificationAsync(BulkNotificationDto bulkNotificationDto);
    }

    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;

            // Initialize Firebase Admin SDK if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("path/to/your/firebase-service-account-key.json"),
                });
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<string> SendNotificationAsync(NotificationDto notificationDto)
        {
            try
            {
                var message = new Message()
                {
                    Token = notificationDto.Token,
                    Notification = new Notification()
                    {
                        Title = notificationDto.Title,
                        Body = notificationDto.Body
                    },
                    Data = notificationDto.Data,
                    Android = new AndroidConfig()
                    {
                        Notification = new AndroidNotification()
                        {
                            Icon = "ic_notification",
                            Color = "#FF6B35",
                            Sound = "default"
                        }
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Alert = new ApsAlert()
                            {
                                Title = notificationDto.Title,
                                Body = notificationDto.Body
                            },
                            Sound = "default"
                        }
                    }
                };

                var response = await _messaging.SendAsync(message);
                _logger.LogInformation($"Successfully sent message: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                throw;
            }
        }

        public async Task<BatchResponse> SendBulkNotificationAsync(BulkNotificationDto bulkNotificationDto)
        {
            try
            {
                var messages = bulkNotificationDto.Tokens.Select(token => new Message()
                {
                    Token = token,
                    Notification = new Notification()
                    {
                        Title = bulkNotificationDto.Title,
                        Body = bulkNotificationDto.Body
                    },
                    Data = bulkNotificationDto.Data,
                    Android = new AndroidConfig()
                    {
                        Notification = new AndroidNotification()
                        {
                            Icon = "ic_notification",
                            Color = "#FF6B35",
                            Sound = "default"
                        }
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Alert = new ApsAlert()
                            {
                                Title = bulkNotificationDto.Title,
                                Body = bulkNotificationDto.Body
                            },
                            Sound = "default"
                        }
                    }
                }).ToList();

                var response = await _messaging.SendAllAsync(messages);
                _logger.LogInformation($"Successfully sent {response.SuccessCount} messages out of {bulkNotificationDto.Tokens.Count}");
                
                if (response.FailureCount > 0)
                {
                    _logger.LogWarning($"Failed to send {response.FailureCount} messages");
                    foreach (var error in response.Responses.Where(r => !r.IsSuccess))
                    {
                        _logger.LogError($"Error: {error.Exception?.Message}");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notifications");
                throw;
            }
        }
    }
}

