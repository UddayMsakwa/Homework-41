namespace AutoServiceApp.Services;   

public interface INotifier
{
    void Send(string recipient, string message);
}