namespace Shrooman;

public class Plant
{
    public Seed ParentSeed { get; set; }
    public int CurrentGrowth { get; set; }

    public Plant() { }

    public Plant(Seed seed)
    {
        ParentSeed = seed;
        CurrentGrowth = 0;
    }

    public bool IsReady()
    {
        if (CurrentGrowth >= ParentSeed.GrowDays)
        {
            return true;
        }
        return false;
    }
}