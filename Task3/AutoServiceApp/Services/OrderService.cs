using System.Text;
using AutoServiceApp.Models;

namespace AutoServiceApp.Services;

public class OrderService
{
    private readonly AutoServiceManager _manager;
    private readonly SmsNotifier _sms;
    private readonly EmailSender _email;
    private readonly OrderStatusService _statusService;

    public OrderService(AutoServiceManager manager, SmsNotifier sms, EmailSender email, OrderStatusService statusService)
    {
        _manager = manager;
        _sms = sms;
        _email = email;
        _statusService = statusService;
    }

    public RepairOrder CreateOrder(Customer? customer, Car? car, string description, Mechanic? mechanic, OrderStatus status, string paymentMethod)
    {
        var order = new RepairOrder
        {
            OrderNumber = "RO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
            CustomerId = customer?.Id ?? "",
            CarId = car?.Id ?? "",
            Customer = customer,
            Car = car,
            ProblemDescription = description,
            AssignedMechanicId = mechanic?.Id ?? "",
            AssignedMechanic = mechanic,
            Status = status,
            PaymentMethod = paymentMethod,
            Cost = 0
        };
        order.StatusHistory.Add($"{DateTime.Now:g}: order created with status {status}");
        _manager.Orders.Add(order);
        if (mechanic != null)
            mechanic.AssignedOrderIds.Add(order.Id);
        _manager.SaveAll();
        return order;
    }

    public void UpdateOrder(RepairOrder order, Customer? customer, Car? car, string description, Mechanic? mechanic, OrderStatus status, decimal cost, string paymentMethod)
    {
        order.CustomerId = customer?.Id ?? "";
        order.CarId = car?.Id ?? "";
        order.Customer = customer;
        order.Car = car;
        order.ProblemDescription = description;
        order.AssignedMechanicId = mechanic?.Id ?? "";
        order.AssignedMechanic = mechanic;
        order.PaymentMethod = paymentMethod;
        order.Cost = cost;
        if (order.Status != status)
            ChangeOrderStatus(order, status, "both");
        _manager.RelinkEverything();
        _manager.SaveAll();
    }

    public void ChangeOrderStatus(RepairOrder order, OrderStatus newStatus, string notificationType)
    {
        _manager.Context.SelectedOrder = order;

        _statusService.UpdateStatus(order, newStatus,
            calculateCost: () => CalculateOrderCost(order, true, order.PaymentMethod));

        NotifyAboutStatus(order, notificationType);
        _manager.SaveAll();
    }

    public void AddWorkToOrder(RepairOrder order, string name, double hours, decimal cost)
    {
        var work = new RepairWork { Name = name, Hours = hours, Cost = cost };
        order.Works.Add(work);
        order.Cost = CalculateOrderCost(order, false, order.PaymentMethod);
        _manager.SaveAll();
    }

    public bool UsePartForOrder(RepairOrder order, Part part, int qty)
    {
        _manager.Context.SelectedPart = part;
        if (part.Stock < qty)
            return false;

        part.Stock -= qty;
        for (var i = 0; i < qty; i++)
            order.UsedPartIds.Add(part.Id);
        order.Cost += part.Price * qty * 1.50m;
        order.StatusHistory.Add($"{DateTime.Now:g}: part used {part.Name} x{qty}");
        _manager.SaveAll();
        return true;
    }

    public decimal CalculateOrderCost(RepairOrder order, bool final, string paymentMethod)
    {
        var works = order.Works.Sum(x => x.Cost + (decimal)x.Hours * (order.AssignedMechanic?.HourRate ?? 0));
        var parts = order.UsedPartIds.Select(id => _manager.Parts.FirstOrDefault(p => p.Id == id))
                                   .Where(p => p != null)
                                   .Sum(p => p!.Price * 1.20m);
        var result = works + parts;
        if (paymentMethod == "card")
            result += result * 0.05m;
        if (order.Customer != null && order.Customer.Cars.Count > 2)
            result -= result * 0.10m;
        if (final && order.Status == OrderStatus.Ready)
            result += 500;
        if (result > 10000)
            _manager.Context.TempDiscount = result * 0.15m;
        else
            _manager.Context.TempDiscount = 0;
        return result - _manager.Context.TempDiscount;
    }

    public string BuildOrderDetails(RepairOrder order)
    {
        var sb = new StringBuilder();
        sb.AppendLine(order.ToString());
        sb.AppendLine(order.ProblemDescription);
        sb.AppendLine("Works:");
        foreach (var work in order.Works)
            sb.AppendLine(" - " + work);
        sb.AppendLine("History:");
        foreach (var h in order.StatusHistory)
            sb.AppendLine(" - " + h);

        var firstCar = order.Customer?.Cars.FirstOrDefault();
        if (firstCar != null)
            sb.AppendLine("First car owner phone: " + firstCar.Owner?.Phone);
        return sb.ToString();
    }

    public List<RepairOrder> GetOrdersForMechanic(Mechanic m)
    {
        var result = new List<RepairOrder>();
        foreach (var id in m.AssignedOrderIds)
        {
            var o = _manager.Orders.FirstOrDefault(x => x.Id == id);
            if (o != null)
                result.Add(o);
        }
        return result;
    }

    private void NotifyAboutStatus(RepairOrder order, string type)
    {
        var phone = order.Customer?.Phone ?? "";
        var email = order.Customer?.Email ?? "";
        var text = $"Order {order.OrderNumber}: new status {order.Status}";

        if (type == "sms")
            _sms.Send(phone, text);
        else if (type == "email")
            _email.Send(email, text);
        else
        {
            _sms.Send(phone, text);
            _email.Send(email, text);
        }
        _manager.Notifications.Add($"{DateTime.Now:g}: {type} {text}");
    }
}