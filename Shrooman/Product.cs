namespace Shrooman;

public class Product
{
    public string Name { get; set; }
    public int Price { get; set; }

    public Product() { }

    public Product(string name, int price)
    {
        Name = name;
        Price = price;
    }
}