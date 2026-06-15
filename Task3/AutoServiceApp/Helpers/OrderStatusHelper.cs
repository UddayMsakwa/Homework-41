using AutoServiceApp.Models;

namespace AutoServiceApp.Helpers;

public class OrderStatusHelper
{
    
    public List<OrderStatus> CommonStatuses { get; set; } = new()
    {
        OrderStatus.New,
        OrderStatus.Diagnostics,
        OrderStatus.InProgress,
        OrderStatus.WaitingForParts,
        OrderStatus.Ready,
        OrderStatus.Released
    };

    
    public void MarkStatus(RepairOrder order, OrderStatus status)
    {
        order.Status = status;
        order.StatusHistory.Add($"{DateTime.Now:g}: status changed to {status}");
        if (status == OrderStatus.Ready)
            order.CompletedAt = DateTime.Now;
    }
}