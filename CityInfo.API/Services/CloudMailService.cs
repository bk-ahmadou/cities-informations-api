namespace CityInfo.API.Services
{
    public class CloudMailService : IMailService
    {
        private readonly string _mailTo = String.Empty;
        private readonly string _mailFrom = String.Empty;

        public CloudMailService(IConfiguration configuration)
        {
            _mailFrom = configuration["mailSetting:mailToAddress"];
            _mailTo = configuration["mailSetting:mailFromAddress"];
        }

        public void Send(string subject, string message)
        {
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, " +
                $"with {nameof(CloudMailService)}.");
            Console.WriteLine($"Subject : {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
