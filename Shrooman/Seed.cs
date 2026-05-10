namespace Shrooman;

public class Seed : Product
{
    public int GrowDays { get; set; }
    public bool IsMushroom { get; set; }
    public int HarvestValue { get; set; }

    public Seed() : base() { }

    public Seed(string name, int buyPrice, int harvestValue, int growDays, bool isMushroom = false) 
        : base(name, buyPrice)
    {
        HarvestValue = harvestValue;
        GrowDays = growDays;
        IsMushroom = isMushroom;
    }
}