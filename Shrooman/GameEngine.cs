using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Shrooman;

public class SaveData
{
    public Player SavedPlayer { get; set; }
    public Garden SavedGarden { get; set; }
}

public class GameEngine
{
    private Player _player = new Player();
    private Garden _garden = new Garden();
    private Workshop _workshop = new Workshop();
    private List<Seed> _shopStock = new List<Seed>();
    private bool _isRunning = true;
    //snad ted
    private string _saveFile;
    public void InitializeSavePath()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "Shrooman"
        );
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        _saveFile = Path.Combine(folder, "savegame.json");
    }

    public GameEngine()
    {
        InitializeSavePath();
        _shopStock.Add(new Seed("White Widow", 10, 20, 2));
        _shopStock.Add(new Seed("Bubba Kush", 30, 80, 3));
        _shopStock.Add(new Seed("Purple Kush", 60, 200, 3));
        _shopStock.Add(new Seed("Super Silver Haze", 100, 300, 4));
        _shopStock.Add(new Seed("Blue Dream", 250, 700, 5));
        _shopStock.Add(new Seed("OG Kush", 400, 950, 3));

        _shopStock.Add(new Seed("Albino Penis Envy", 4000, 13000, 6, true));
        _shopStock.Add(new Seed("Golden Teacher", 8900, 20000, 9, true));
        _shopStock.Add(new Seed("Psilocybe cubensis", 21000, 100000, 8, true));
    }

    public void Start()
    {
        while (_isRunning)
        {
            AnsiConsole.Clear();
            DrawUI();
            HandleMenu();
        }
    }

    private void DrawUI()
    {
        string stats = $"[yellow]Money:[/] [green]{_player.Money}$[/] | [bold fuchsia]Aura: {_player.AuraPoints}[/] | Prestige: [purple]{_player.PrestigePoints}[/] | Day: [cyan]{_player.Day}[/]";
        string upgrades = $"Capacity: {_garden.GrowingPlants.Count}/{_garden.MaxCapacity} | Kitchen: {(_player.HasKitchen ? "[lime]YES[/]" : "[red]NO[/]")} | Lab: {(_player.HasLaboratory ? "[lime]YES[/]" : "[red]NO[/]")}";
        AnsiConsole.Write(new Panel(stats + "\n" + upgrades).Header(" SHROOMAN TYCOON ").BorderColor(Color.Fuchsia));

        if (_player.InventorySeeds.Count > 0 || _player.InventoryHarvested.Count > 0)
        {
            var invTable = new Table().BorderColor(Color.Blue).Title("YOUR STORAGE");
            invTable.AddColumn("Item");
            invTable.AddColumn("Amount");

            Dictionary<string, int> sCounts = new Dictionary<string, int>();
            foreach (var s in _player.InventorySeeds) {
                if (sCounts.ContainsKey(s.Name)) sCounts[s.Name]++;
                else sCounts[s.Name] = 1;
            }
            foreach (var pair in sCounts) invTable.AddRow(pair.Key + " (Seeds)", pair.Value + "x");

            Dictionary<string, int> hCounts = new Dictionary<string, int>();
            foreach (var h in _player.InventoryHarvested) {
                if (hCounts.ContainsKey(h.ParentSeed.Name)) hCounts[h.ParentSeed.Name]++;
                else hCounts[h.ParentSeed.Name] = 1;
            }
            foreach (var pair in hCounts) invTable.AddRow("[green]" + pair.Key + " (Harvested)[/]", pair.Value + "x");

            AnsiConsole.Write(invTable);
        }

        if (_garden.GrowingPlants.Count > 0)
        {
            var table = new Table().BorderColor(Color.DarkGreen).Title("GARDEN");
            table.AddColumn("Plant");
            table.AddColumn("Status");
            foreach (var p in _garden.GrowingPlants) {
                string status = p.IsReady() ? "[bold lime]READY[/]" : p.CurrentGrowth + "/" + p.ParentSeed.GrowDays + " d";
                table.AddRow(p.ParentSeed.Name, status);
            }
            AnsiConsole.Write(table);
        }
    }

    private void HandleMenu()
    {
        List<string> choices = new List<string> { "Shop", "Plant", "Harvest" };
        if (_player.HasKitchen) choices.Add("Workshop");
        choices.AddRange(new[] { "Sell", "Upgrades", "Next Day", "Save/Load", "Tutorial", "Prestige", "Exit" });

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What to do?")
                .HighlightStyle(new Style(foreground: Color.Green)) 
                .AddChoices(choices)
        );

        if (choice == "Shop") Buy();
        else if (choice == "Plant") PlantSeeds();
        else if (choice == "Harvest") Harvest();
        else if (choice == "Workshop") Work();
        else if (choice == "Sell") SellMenu();
        else if (choice == "Upgrades") UpgradesMenu();
        else if (choice == "Next Day") NextDay();
        else if (choice == "Save/Load") SaveLoadMenu();
        else if (choice == "Tutorial") ShowTutorial();
        else if (choice == "Prestige") TryPrestige();
        else if (choice == "Exit") _isRunning = false;
    }

    private void SellMenu()
    {
        var type = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Sell:").HighlightStyle(new Style(foreground: Color.Green)).AddChoices("Harvested Plants", "Processed Products", "Back"));
        if (type == "Back") return;

        double bonus = 1.0 + (_player.AuraPoints / 100.0);
        
        if (type == "Harvested Plants")
        {
            if (_player.InventoryHarvested.Count == 0) return;
            List<string> options = new List<string> { "SELL ALL" };
            HashSet<string> uniqueNames = new HashSet<string>();
            foreach (var p in _player.InventoryHarvested) uniqueNames.Add(p.ParentSeed.Name);
            foreach (var name in uniqueNames)
            {
                int singlePrice = 0;
                foreach(var p in _player.InventoryHarvested) if(p.ParentSeed.Name == name) singlePrice = (int)(p.ParentSeed.HarvestValue * bonus);
                options.Add($"{name} - [yellow]{singlePrice}$[/]");
            }
            options.Add("Back");

            var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices(options));
            if (sel == "Back") return;

            if (sel == "SELL ALL") {
                int total = 0;
                foreach (var p in _player.InventoryHarvested) { total += (int)(p.ParentSeed.HarvestValue * bonus); _player.AuraPoints += 2; }
                _player.Money += total; _player.InventoryHarvested.Clear();
            } else {
                string pureName = sel.Split(" - ")[0];
                int has = 0; foreach (var p in _player.InventoryHarvested) if (p.ParentSeed.Name == pureName) has++;
                int count = AnsiConsole.Ask<int>("How many? (0 for all)");
                if (count <= 0 || count > has) count = has;
                int earned = 0; int soldCount = 0;
                for (int i = _player.InventoryHarvested.Count - 1; i >= 0 && soldCount < count; i--) {
                    if (_player.InventoryHarvested[i].ParentSeed.Name == pureName) {
                        earned += (int)(_player.InventoryHarvested[i].ParentSeed.HarvestValue * bonus);
                        _player.AuraPoints += 2; _player.InventoryHarvested.RemoveAt(i); soldCount++;
                    }
                }
                _player.Money += earned;
            }
        } else {
            if (_player.InventoryProducts.Count == 0) return;
            List<string> options = new List<string> { "SELL ALL" };
            HashSet<string> uniqueProds = new HashSet<string>();
            foreach (var pr in _player.InventoryProducts) uniqueProds.Add(pr.Name);
            foreach (var name in uniqueProds)
            {
                int singlePrice = 0;
                foreach(var pr in _player.InventoryProducts) if(pr.Name == name) singlePrice = (int)(pr.Price * bonus);
                options.Add($"{name} - [yellow]{singlePrice}$[/]");
            }
            options.Add("Back");

            var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices(options));
            if (sel == "Back") return;

            if (sel == "SELL ALL") {
                int total = 0;
                foreach (var pr in _player.InventoryProducts) { total += (int)(pr.Price * bonus); _player.AuraPoints += 5; }
                _player.Money += total; _player.InventoryProducts.Clear();
            } else {
                string pureName = sel.Split(" - ")[0];
                int has = 0; foreach (var pr in _player.InventoryProducts) if (pr.Name == pureName) has++;
                int count = AnsiConsole.Ask<int>("How many?");
                int earned = 0; int soldCount = 0;
                for (int i = _player.InventoryProducts.Count - 1; i >= 0 && soldCount < count; i--) {
                    if (_player.InventoryProducts[i].Name == pureName) {
                        earned += (int)(_player.InventoryProducts[i].Price * bonus);
                        _player.AuraPoints += 5; _player.InventoryProducts.RemoveAt(i); soldCount++;
                    }
                }
                _player.Money += earned;
            }
        }
        Thread.Sleep(1000);
    }

    private void Buy()
    {
        List<string> menu = new List<string>();
        foreach (var s in _shopStock) {
            if (s.IsMushroom && _player.PrestigePoints < 2) menu.Add($"[red]{s.Name} (LOCKED)[/]");
            else menu.Add($"{s.Name} ({s.Price}$)");
        }
        menu.Add("Back");

        var selection = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices(menu));
        if (selection == "Back" || selection.Contains("LOCKED")) return;

        Seed selected = null;
        foreach (var s in _shopStock) if (selection.Contains(s.Name)) selected = s;

        int count = AnsiConsole.Ask<int>("How many?");
        if (count > 0 && _player.Money >= selected.Price * count) {
            _player.Money -= (selected.Price * count);
            for (int i = 0; i < count; i++) _player.InventorySeeds.Add(selected);
        }
        Thread.Sleep(600);
    }

    private void PlantSeeds()
    {
        if (_player.InventorySeeds.Count == 0) return;
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var s in _player.InventorySeeds) {
            if (counts.ContainsKey(s.Name)) counts[s.Name]++;
            else counts[s.Name] = 1;
        }

        List<string> menu = new List<string>();
        foreach (var pair in counts) menu.Add(pair.Key + " (" + pair.Value + "x)");
        menu.Add("Back");

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices(menu));
        if (choice == "Back") return;

        string pureName = choice.Split(" (")[0];

        int toPlant = AnsiConsole.Ask<int>("How many?");
        int available = _garden.MaxCapacity - _garden.GrowingPlants.Count;
        int has = counts[pureName];
        int final = Math.Min(toPlant, Math.Min(available, has));

        int done = 0;
        for (int i = _player.InventorySeeds.Count - 1; i >= 0 && done < final; i--) {
            if (_player.InventorySeeds[i].Name == pureName) {
                _garden.GrowingPlants.Add(new Plant(_player.InventorySeeds[i]));
                _player.InventorySeeds.RemoveAt(i);
                done++;
            }
        }
    }

    private void Harvest()
    {
        int count = 0;
        for (int i = _garden.GrowingPlants.Count - 1; i >= 0; i--) {
            if (_garden.GrowingPlants[i].IsReady()) {
                _player.InventoryHarvested.Add(_garden.GrowingPlants[i]);
                _garden.GrowingPlants.RemoveAt(i);
                count++;
            }
        }
        AnsiConsole.MarkupLine($"[green]Harvested {count} items![/]");
        Thread.Sleep(800);
    }

    private void Work()
    {
        List<string> options = new List<string> { "Back" };
        if (_player.HasKitchen) options.Insert(0, "Kitchen (Cook)");
        if (_player.HasLaboratory) options.Insert(1, "Laboratory (Extract)");

        var mode = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices(options));
        if (mode == "Back" || _player.InventoryHarvested.Count == 0) return;

        int count = AnsiConsole.Ask<int>("How many?");
        for (int i = 0; i < count && _player.InventoryHarvested.Count > 0; i++) {
            var plant = _player.InventoryHarvested[0];
            _player.InventoryHarvested.RemoveAt(0);
            if (mode.Contains("Kitchen")) _player.InventoryProducts.Add(_workshop.Cook(plant));
            else _player.InventoryProducts.Add(_workshop.Extract(plant));
        }
    }

    private void UpgradesMenu()
    {
        List<string> menu = new List<string> { "Bigger Garden (+5) - 200$" };
        if (!_player.HasKitchen) menu.Add("Buy Kitchen - 500$");
        if (_player.PrestigePoints < 2) menu.Add("[red]Laboratory (LOCKED)[/]");
        else if (!_player.HasLaboratory) menu.Add("Buy Laboratory - 1500$");
        menu.Add("Back");

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices(menu));
        if (choice.Contains("Garden") && _player.Money >= 200) { _player.Money -= 200; _garden.MaxCapacity += 5; }
        else if (choice.Contains("Kitchen") && _player.Money >= 500) { _player.Money -= 500; _player.HasKitchen = true; }
        else if (choice.Contains("Laboratory") && !choice.Contains("LOCKED") && _player.Money >= 1500) { _player.Money -= 1500; _player.HasLaboratory = true; }
    }

    private void SaveLoadMenu()
    {
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().HighlightStyle(new Style(foreground: Color.Green)).AddChoices("Save", "Load", "Back"));
        if (choice == "Save") {
            SaveData data = new SaveData { SavedPlayer = _player, SavedGarden = _garden };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveFile, json);
            AnsiConsole.MarkupLine("[green]Saved![/]");
        } else if (choice == "Load") {
            if (File.Exists(_saveFile)) {
                string json = File.ReadAllText(_saveFile);
                SaveData loaded = JsonSerializer.Deserialize<SaveData>(json);
                _player = loaded.SavedPlayer;
                _garden = loaded.SavedGarden;
                AnsiConsole.MarkupLine("[yellow]Loaded![/]");
            }
        }
        Thread.Sleep(1200);
    }

    private void ShowTutorial()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Panel("1. Buy seeds.\n2. Plant and wait.\n3. Harvest and sell.\n4. Upgrades unlock workshops.\n5. Aura increases prices permanently.").Header(" TUTORIAL ").BorderColor(Color.Cyan));
        AnsiConsole.MarkupLine("\nPress any key...");
        Console.ReadKey();
    }

    private void NextDay() { _player.Day++; _garden.GrowAll(); Thread.Sleep(600); }

    private void TryPrestige()
    {
        if (_player.Money >= 10000)
        {
            _player.PrestigePoints = _player.PrestigePoints + 1;
            _player.Money = 100;
            _player.Day = 1;
            _player.HasKitchen = false;
            _player.HasLaboratory = false;
            _garden.MaxCapacity = 5;
            _garden.GrowingPlants.Clear();
            _player.InventorySeeds.Clear();
            _player.InventoryHarvested.Clear();
            _player.InventoryProducts.Clear();
            AnsiConsole.MarkupLine("[bold fuchsia]PRESTIGE SUCCESSFUL! Aura remains, everything else resets.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]ERROR: You need at least 10000$ to Prestige![/]");
        }
        Thread.Sleep(2500);
    }
}
