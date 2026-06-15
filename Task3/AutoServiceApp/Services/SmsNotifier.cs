using AutoServiceApp.Services;

namespace AutoServiceApp.Services;

public class SmsNotifier : INotifier
{
    public List<string> SentMessages { get; set; } = new();

    public void Send(string recipient, string message)
    {
        SentMessages.Add($"SMS {DateTime.Now:g} -> {recipient}: {message}");
    }
}