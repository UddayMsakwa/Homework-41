namespace AutoServiceApp.Models;

public class OrderProcessingContext
{
    public RepairOrder? SelectedOrder { get; set; }
    public Part? SelectedPart { get; set; }
    public decimal TempDiscount { get; set; }
    public BaseReport? CurrentReport { get; set; }
}