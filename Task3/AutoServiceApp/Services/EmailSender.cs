using AutoServiceApp.Services;

namespace AutoServiceApp.Services;

public class EmailSender : INotifier
{
    public List<string> Log { get; set; } = new();

    
    public void Send(string email, string subject, string body)
    {
        Log.Add($"EMAIL {DateTime.Now:g} -> {email}: {subject} {body}");
    }

    
    public void Send(string recipient, string message)
    {
        Send(recipient, "Order status", message);
    }
}