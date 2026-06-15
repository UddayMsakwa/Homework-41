namespace AutoServiceApp.Models;

public enum OrderStatus
{
    New,
    Diagnostics,
    InProgress,
    WaitingForParts,
    Ready,
    Released,
    Completed,
    Cancelled
}