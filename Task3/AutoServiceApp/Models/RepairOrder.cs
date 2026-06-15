namespace AutoServiceApp.Models;

public class RepairOrder : BaseEntity
{
    public string OrderNumber { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string CarId { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public Customer? Customer { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Car? Car { get; set; }
    public string ProblemDescription { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public string AssignedMechanicId { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public Mechanic? AssignedMechanic { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public decimal Cost { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public List<RepairWork> Works { get; set; } = new();
    public List<string> UsedPartIds { get; set; } = new();
    public List<string> StatusHistory { get; set; } = new();

    
    public RepairOrderType Type { get; set; } = RepairOrderType.Standard;

    
    public bool NeedTaxi { get; set; }
    public decimal UrgentFee { get; set; } = BusinessRules.UrgentRepairFee;
    public string WarrantyNumber { get; set; } = "";
    public bool ApprovedByDealer { get; set; }

    public void Complete()
    {
        Status = OrderStatus.Completed;
        CompletedAt = DateTime.Now;
        StatusHistory.Add($"{DateTime.Now:g}: order completed");
    }

    public string GetCustomerName()
    {
        return Customer?.Name ?? "";
    }

    public string GetOwnerPhone()
    {
        return Car?.Owner?.Phone ?? "";
    }

    public override string ToString()
    {
        var client = Customer?.Name ?? CustomerId;
        var car = Car == null ? CarId : $"{Car.Make} {Car.Model}";
        return $"{OrderNumber}: {client}, {car}, {Status}, {Cost:C}";
    }
}