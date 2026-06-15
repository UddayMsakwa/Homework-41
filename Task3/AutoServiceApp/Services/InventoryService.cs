using AutoServiceApp.Models;

namespace AutoServiceApp.Services;

public class InventoryService
{
    private readonly AutoServiceManager _manager;

    public InventoryService(AutoServiceManager manager)
    {
        _manager = manager;
    }

    public Part AddPart(string name, string article, decimal price, int stock)
    {
        var p = new Part { Name = name, Article = article, Price = price, Stock = stock };
        _manager.Parts.Add(p);
        _manager.SaveAll();
        return p;
    }

    public void UpdatePart(Part part, string name, string article, decimal price, int stock)
    {
        part.Name = name;
        part.Article = article;
        part.Price = price;
        part.Stock = stock;
        _manager.SaveAll();
    }

    public void DeletePart(Part p)
    {
        _manager.Parts.Remove(p);
        _manager.SaveAll();
    }
}