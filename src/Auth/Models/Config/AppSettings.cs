namespace Auth.Models.Config
{
    public class AppSettings
    {
        public string ApplicationName { get; set; }
        public Urls Urls { get; set; }
        public string AdminEmail { get; set; }
        public string ReCaptchaVerificationUrl { get; set; }
        public string ReCaptchaSecret { get; set; }
    }
}
