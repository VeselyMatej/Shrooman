using System.Collections.Generic;

namespace Shrooman;

public class Player
{
    public int Money { get; set; } = 100;
    public int AuraPoints { get; set; } = 0;
    public int PrestigePoints { get; set; } = 0;
    public int Day { get; set; } = 1;
    
    public bool HasKitchen { get; set; } = false;
    public bool HasLaboratory { get; set; } = false;

    public List<Seed> InventorySeeds { get; set; } = new List<Seed>();
    public List<Plant> InventoryHarvested { get; set; } = new List<Plant>();
    public List<ProcessedProduct> InventoryProducts { get; set; } = new List<ProcessedProduct>();
}