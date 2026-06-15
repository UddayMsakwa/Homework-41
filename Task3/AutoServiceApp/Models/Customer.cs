namespace AutoServiceApp.Models;

public class Customer : BaseEntity, IExportable
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";

    [System.Text.Json.Serialization.JsonIgnore]
    private readonly List<Car> _cars = new();
    public IReadOnlyCollection<Car> Cars => _cars;

    public void AddCar(Car car)
    {
        _cars.Add(car);
    }

    
    public void RemoveCar(Car car)
    {
        _cars.Remove(car);
    }

    public void ClearCars()
    {
        _cars.Clear();
    }

    public string DisplayText()
    {
        return $"{Name} / {Phone}";
    }

    public string LastPaymentMethod { get; set; } = "cash";

    public string Export() => $"{Name};{Phone};{Email};{Address}";
    public override string ToString() => string.IsNullOrWhiteSpace(Phone) ? Name : $"{Name} ({Phone})";
}