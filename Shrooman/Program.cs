using Spectre.Console;
using System;
using System.Collections.Generic;

// ----- SEED -----
class Seed
{
    public string Name;
    public int Price;
    public int GrowTime;
    public int SellPrice;

    public Seed(string name, int price, int growTime, int sellPrice)
    {
        Name = name;
        Price = price;
        GrowTime = growTime;
        SellPrice = sellPrice;
    }
}

// ----- PLANT -----
class Plant
{
    public string Name;
    public int Growth;
    public int MaxGrowth;
    public int SellPrice;

    public Plant(Seed seed)
    {
        Name = seed.Name;
        Growth = 0;
        MaxGrowth = seed.GrowTime;
        SellPrice = seed.SellPrice;
    }

    public void Grow()
    {
        if (Growth < MaxGrowth)
            Growth++;
    }

    public bool IsReady()
    {
        return Growth >= MaxGrowth;
    }
}

// ----- PLAYER -----
class Player
{
    public int Money = 100;
    public List<Seed> Inventory = new List<Seed>();
}

// ----- GARDEN -----
class Garden
{
    public List<Plant> Plants = new List<Plant>();

    public void PlantSeed(Seed seed)
    {
        Plants.Add(new Plant(seed));
    }

    public void NextDay()
    {
        foreach (var plant in Plants)
        {
            plant.Grow();
        }
    }

    public void SellReadyPlants(Player player)
    {
        List<Plant> toRemove = new List<Plant>();

        foreach (var plant in Plants)
        {
            if (plant.IsReady())
            {
                player.Money += plant.SellPrice;
                toRemove.Add(plant);
            }
        }

        foreach (var p in toRemove)
        {
            Plants.Remove(p);
        }
    }
}

// ----- GAME -----
class Game
{
    Player player = new Player();
    Garden garden = new Garden();
    List<Seed> shop = new List<Seed>();

    public Game()
    {
        shop.Add(new Seed("Basic Weed", 10, 2, 25));
        shop.Add(new Seed("Purple Weed", 20, 3, 50));
        shop.Add(new Seed("Exotic Weed", 40, 4, 100));
    }

    public void Start()
    {
        while (true)
        {
            DrawUI();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[#a8ffb0]Choose action[/]")
                .HighlightStyle(new Style(foreground: Color.Lime))
                .AddChoices("Buy seeds", "Plant seed", "Next day", "Sell ready plants", "Exit")
            );

            if (choice == "Buy seeds") Buy();
            else if (choice == "Plant seed") Plant();
            else if (choice == "Next day") garden.NextDay();
            else if (choice == "Sell ready plants") garden.SellReadyPlants(player);
            else if (choice == "Exit") break;
        }
    }

    void DrawUI()
    {
        AnsiConsole.Clear();

        var panel = new Panel(
            $"[#a8ffb0]Money:[/] {player.Money}$\n" +
            $"[#a8ffb0]Inventory:[/] {player.Inventory.Count}\n" +
            $"[#a8ffb0]Plants:[/] {garden.Plants.Count}"
        );

        panel.Header("[lime]Shrooman Tycoon[/]", Justify.Center);
        panel.Border = BoxBorder.Double;

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        foreach (var plant in garden.Plants)
        {
            string status = plant.IsReady()
                ? "[lime]READY[/]"
                : $"[#a8ffb0]{plant.Growth}/{plant.MaxGrowth}[/]";

            AnsiConsole.MarkupLine($"[#a8ffb0]{plant.Name}[/] - {status}");
        }

        AnsiConsole.WriteLine();
    }

    void Buy()
    {
        var options = new List<string>();

        foreach (var s in shop)
        {
            options.Add($"[#a8ffb0]{s.Name}[/] | {s.Price}$ | Grow:{s.GrowTime}d | Sell:{s.SellPrice}$");
        }

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[#a8ffb0]Choose seed[/]")
            .HighlightStyle(new Style(foreground: Color.Lime))
            .AddChoices(options)
        );

        Seed selected = shop.Find(s => choice.Contains(s.Name));

        if (player.Money >= selected.Price)
        {
            player.Money -= selected.Price;
            player.Inventory.Add(selected);
            AnsiConsole.MarkupLine("[lime]Bought![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Not enough money[/]");
        }

        Console.ReadKey();
    }

    void Plant()
    {
        if (player.Inventory.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No seeds[/]");
            Console.ReadKey();
            return;
        }

        var options = new List<string>();
        foreach (var s in player.Inventory)
        {
            options.Add($"[#a8ffb0]{s.Name}[/]");
        }

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[#a8ffb0]Choose seed to plant[/]")
            .HighlightStyle(new Style(foreground: Color.Lime))
            .AddChoices(options)
        );

        Seed selected = player.Inventory.Find(s => choice.Contains(s.Name));

        player.Inventory.Remove(selected);
        garden.PlantSeed(selected);

        AnsiConsole.MarkupLine("[lime]Planted![/]");
        Console.ReadKey();
    }
}

// ----- MAIN -----
class Program
{
    static void Main()
    {
        Game game = new Game();
        game.Start();
    }
}