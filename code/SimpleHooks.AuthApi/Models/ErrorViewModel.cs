namespace SimpleTools.SimpleHooks.AuthApi.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; init; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
