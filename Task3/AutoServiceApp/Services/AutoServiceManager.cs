using System.Text;
using AutoServiceApp.Helpers;
using AutoServiceApp.Models;
using AutoServiceApp.Storage;

namespace AutoServiceApp.Services;

public class AutoServiceManager
{
    public List<Customer> Customers { get; set; } = new();
    public List<Car> Cars { get; set; } = new();
    public List<RepairOrder> Orders { get; set; } = new();
    public List<Part> Parts { get; set; } = new();
    public List<Mechanic> Mechanics { get; set; } = new();
    public List<string> Notifications { get; set; } = new();

    public OrderProcessingContext Context { get; set; } = new();

    public JsonFileStore<Customer> CustomerStore { get; set; } = new();
    public JsonFileStore<Car> CarStore { get; set; } = new();
    public JsonFileStore<RepairOrder> OrderStore { get; set; } = new();
    public JsonFileStore<Part> PartStore { get; set; } = new();
    public JsonFileStore<Mechanic> MechanicStore { get; set; } = new();
    public SmsNotifier SmsNotifier { get; set; } = new();
    public EmailSender EmailSender { get; set; } = new();
    public ReportService ReportService { get; set; } = new();
    public OrderStatusHelper StatusHelper { get; set; } = new();
    public OrderStatusService StatusService { get; set; }

    
    public CustomerService CustomerService { get; set; }
    public InventoryService InventoryService { get; set; }
    public OrderService OrderService { get; set; }

    public AutoServiceManager()
    {
        StatusService = new OrderStatusService(StatusHelper);
        CustomerService = new CustomerService(this);
        InventoryService = new InventoryService(this);
        OrderService = new OrderService(this, SmsNotifier, EmailSender, StatusService);
    }

    public void Load()
    {
        Customers = CustomerStore.Load("customers.json");
        Cars = CarStore.Load("cars.json");
        Orders = OrderStore.Load("orders.json");
        Parts = PartStore.Load("parts.json");
        Mechanics = MechanicStore.Load("mechanics.json");
        RelinkEverything();
        if (Customers.Count == 0 && Cars.Count == 0 && Mechanics.Count == 0)
            Seed();
    }

    public void SaveAll()
    {
        CustomerStore.Save("customers.json", Customers);
        CarStore.Save("cars.json", Cars);
        OrderStore.Save("orders.json", Orders);
        PartStore.Save("parts.json", Parts);
        MechanicStore.Save("mechanics.json", Mechanics);
    }

    public void RelinkEverything()
    {
        foreach (var c in Customers)
        {
            c.ClearCars();
            foreach (var car in Cars.Where(x => x.CustomerId == c.Id))
                c.AddCar(car);
        }

        foreach (var car in Cars)
            car.Owner = Customers.FirstOrDefault(x => x.Id == car.CustomerId);

        foreach (var order in Orders)
        {
            order.Customer = Customers.FirstOrDefault(x => x.Id == order.CustomerId);
            order.Car = Cars.FirstOrDefault(x => x.Id == order.CarId);
            order.AssignedMechanic = Mechanics.FirstOrDefault(x => x.Id == order.AssignedMechanicId);
        }

        foreach (var m in Mechanics)
            m.AssignedOrderIds = Orders.Where(x => x.AssignedMechanicId == m.Id).Select(x => x.Id).ToList();
    }

    
    public Customer AddCustomer(CustomerInfo info) => CustomerService.AddCustomer(info);
    public void UpdateCustomer(Customer customer, string name, string phone, string email, string address)
        => CustomerService.UpdateCustomer(customer, name, phone, email, address);
    public void DeleteCustomer(Customer customer) => CustomerService.DeleteCustomer(customer);

    public Part AddPart(string name, string article, decimal price, int stock)
        => InventoryService.AddPart(name, article, price, stock);
    public void UpdatePart(Part part, string name, string article, decimal price, int stock)
        => InventoryService.UpdatePart(part, name, article, price, stock);
    public void DeletePart(Part p) => InventoryService.DeletePart(p);

    public RepairOrder CreateOrder(Customer? customer, Car? car, string description, Mechanic? mechanic, OrderStatus status, string paymentMethod)
        => OrderService.CreateOrder(customer, car, description, mechanic, status, paymentMethod);
    public void UpdateOrder(RepairOrder order, Customer? customer, Car? car, string description, Mechanic? mechanic, OrderStatus status, decimal cost, string paymentMethod)
        => OrderService.UpdateOrder(order, customer, car, description, mechanic, status, cost, paymentMethod);
    public void ChangeOrderStatus(RepairOrder order, OrderStatus newStatus, string notificationType)
        => OrderService.ChangeOrderStatus(order, newStatus, notificationType);
    public void AddWorkToOrder(RepairOrder order, string name, double hours, decimal cost)
        => OrderService.AddWorkToOrder(order, name, hours, cost);
    public bool UsePartForOrder(RepairOrder order, Part part, int qty)
        => OrderService.UsePartForOrder(order, part, qty);
    public decimal CalculateOrderCost(RepairOrder order, bool final, string paymentMethod)
        => OrderService.CalculateOrderCost(order, final, paymentMethod);
    public string BuildOrderDetails(RepairOrder order) => OrderService.BuildOrderDetails(order);
    public List<RepairOrder> GetOrdersForMechanic(Mechanic m) => OrderService.GetOrdersForMechanic(m);

    
    public void NotifyAboutStatus(RepairOrder order, string type) { }

    
    public Mechanic AddMechanic(string name, string specialization, decimal hourRate)
    {
        var m = new Mechanic { Name = name, Specialization = specialization, HourRate = hourRate };
        Mechanics.Add(m);
        SaveAll();
        return m;
    }

    public void UpdateMechanic(Mechanic m, string name, string specialization, decimal hourRate)
    {
        m.Name = name;
        m.Specialization = specialization;
        m.HourRate = hourRate;
        SaveAll();
    }

    public void DeleteMechanic(Mechanic m)
    {
        Mechanics.Remove(m);
        foreach (var order in Orders.Where(o => o.AssignedMechanicId == m.Id))
        {
            order.AssignedMechanicId = "";
            order.AssignedMechanic = null;
        }
        SaveAll();
    }

    
    public Car AddCar(Customer? owner, string make, string model, int year, string vin, int mileage, string licensePlate)
    {
        var car = new Car
        {
            CustomerId = owner?.Id ?? "",
            Owner = owner,
            Make = make,
            Model = model,
            Year = year,
            Vin = vin,
            Mileage = mileage,
            LicensePlate = licensePlate
        };
        Cars.Add(car);
        if (owner != null)
            owner.AddCar(car);
        SaveAll();
        return car;
    }

    public void UpdateCar(Car car, Customer? owner, string make, string model, int year, string vin, int mileage, string licensePlate)
    {
        car.CustomerId = owner?.Id ?? "";
        car.Owner = owner;
        car.Make = make;
        car.Model = model;
        car.Year = year;
        car.Vin = vin;
        car.Mileage = mileage;
        car.LicensePlate = licensePlate;
        RelinkEverything();
        SaveAll();
    }

    public void DeleteCar(Car car)
    {
        Cars.Remove(car);
        foreach (var c in Customers)
            c.RemoveCar(car);
        foreach (var order in Orders.Where(x => x.CarId == car.Id).ToList())
            Orders.Remove(order);
        SaveAll();
    }

    public string BuildReports(DateTime from, DateTime to)
    {
        Context.CurrentReport = new RepairReport { Title = "General report", From = from, To = to, Orders = Orders };
        return ReportService.BuildRevenueReport(Orders, from, to) + "\n"
            + ReportService.BuildPopularWorks(Orders) + "\n\n"
            + ReportService.BuildMechanicsLoad(Mechanics, Orders) + "\n"
            + ReportService.BuildPartsStock(Parts);
    }

    private void Seed()
    {
        var c1 = AddCustomer(new CustomerInfo { Name = "John Parker", Phone = "+1 555 100-20-30", Email = "john@example.com", Address = "12 Market Street" });
        var c2 = AddCustomer(new CustomerInfo { Name = "Anna Stone", Phone = "+1 555 555-44-33", Email = "anna@example.com", Address = "45 Lake Avenue" });
        var car1 = AddCar(c1, "Toyota", "Camry", 2018, "JTNB11HK303000001", 87000, "ABC123");
        AddCar(c2, "Kia", "Rio", 2021, "Z94CB41ABMR000002", 43000, "MOR777");
        var m1 = AddMechanic("Sam Miller", "engine", 1200);
        AddMechanic("Owen Lane", "electrical", 1500);
        AddPart("Oil filter", "OF-100", 650, 12);
        AddPart("Brake pads", "BR-500", 3200, 5);
        var order = CreateOrder(c1, car1, "Knock on startup, diagnostics required", m1, OrderStatus.Diagnostics, "card");
        AddWorkToOrder(order, "Computer diagnostics", 1.5, 2500);
        SaveAll();
    }
}