using AutoServiceApp.Helpers;
using AutoServiceApp.Models;

namespace AutoServiceApp.Services;

public class OrderStatusService
{
    private readonly OrderStatusHelper _statusHelper;

    public OrderStatusService(OrderStatusHelper statusHelper)
    {
        _statusHelper = statusHelper;
    }

    public void UpdateStatus(RepairOrder order, OrderStatus newStatus, Func<decimal>? calculateCost = null)
    {
        if (newStatus == OrderStatus.Completed)
        {
            order.Complete();
        }
        else
        {
            _statusHelper.MarkStatus(order, newStatus);
            if (newStatus == OrderStatus.Ready && calculateCost != null)
                order.Cost = calculateCost();
        }

        if (order.AssignedMechanic != null && !order.AssignedMechanic.AssignedOrderIds.Contains(order.Id))
            order.AssignedMechanic.AssignedOrderIds.Add(order.Id);
    }
}