using AutoServiceApp.Models;

namespace AutoServiceApp.Services;

public class CustomerService
{
    private readonly AutoServiceManager _manager;

    public CustomerService(AutoServiceManager manager)
    {
        _manager = manager;
    }

    public Customer AddCustomer(CustomerInfo info)
    {
        var c = new Customer
        {
            Name = info.Name,
            Phone = info.Phone,
            Email = info.Email,
            Address = info.Address
        };
        _manager.Customers.Add(c);
        _manager.SaveAll();
        return c;
    }

    public void UpdateCustomer(Customer customer, string name, string phone, string email, string address)
    {
        customer.Name = name;
        customer.Phone = phone;
        customer.Email = email;
        customer.Address = address;
        foreach (var order in _manager.Orders.Where(x => x.CustomerId == customer.Id))
            order.Customer = customer;
        _manager.SaveAll();
    }

    public void DeleteCustomer(Customer customer)
    {
        _manager.Customers.Remove(customer);
        foreach (var car in _manager.Cars.Where(x => x.CustomerId == customer.Id).ToList())
            _manager.Cars.Remove(car);
        foreach (var order in _manager.Orders.Where(x => x.CustomerId == customer.Id).ToList())
            _manager.Orders.Remove(order);
        _manager.SaveAll();
    }
}