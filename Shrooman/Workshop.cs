namespace Shrooman;

public class Workshop
{
    public ProcessedProduct Cook(Plant plant)
    {
        int newPrice = (int)(plant.ParentSeed.HarvestValue * 1.8);
        return new ProcessedProduct(plant.ParentSeed.Name + " Cooked", newPrice);
    }

    public ProcessedProduct Extract(Plant plant)
    {
        int newPrice = (int)(plant.ParentSeed.HarvestValue * 3.5);
        return new ProcessedProduct(plant.ParentSeed.Name + " Extract", newPrice);
    }
}