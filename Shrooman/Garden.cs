using System.Collections.Generic;

namespace Shrooman;

public class Garden
{
    public List<Plant> GrowingPlants { get; set; } = new List<Plant>();
    public int MaxCapacity { get; set; } = 5;

    public void GrowAll()
    {
        foreach (var plant in GrowingPlants)
        {
            if (plant.CurrentGrowth < plant.ParentSeed.GrowDays)
            {
                plant.CurrentGrowth = plant.CurrentGrowth + 1;
            }
        }
    }
}