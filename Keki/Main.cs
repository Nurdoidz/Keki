//          ╒═════════════════════════════════════════════════════════╕
//          │                          Keki                           │
//          │           A CLI application to manage stacks            │
//          ╘═════════════════════════════════════════════════════════╛
using System.CommandLine;

string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keki");
_ = Directory.CreateDirectory(appDataPath);
string settingsPath = Path.Combine(appDataPath, "settings.json");
Keki.Settings settings = Keki.Settings.Load(settingsPath);
string cakesPath = Path.Combine(appDataPath, "cake");
string cakePath = Path.Combine(cakesPath, $"{settings.Cake}.txt");

_ = Directory.CreateDirectory(cakesPath);
if (!File.Exists(cakePath)) {
    File.WriteAllText(cakePath, "");
}

var rootCommand = new RootCommand("Keki: Stack the cake, bake it, eat it.");

var layerArgument = new Argument<string?>(
        name: "layer",
        description: "Name of the layer.",
        getDefaultValue: () => null);
var cakeArgument = new Argument<string?>(
        name: "cake",
        description: "Name of the cake.",
        getDefaultValue: () => null);
var removeAllOption = new Option<bool>(
        name: "--all",
        description: "Remove all cakes.");
var removeCurrentOption = new Option<bool>(
        name: "--current",
        description: "Remove current cake.");

var clearCommand = new Command("clear", "Eat the cake.");
clearCommand.SetHandler(ClearItems);
clearCommand.AddAlias("eat");
rootCommand.Add(clearCommand);

var listCommand = new Command("list", "List all layers in the cake.");
listCommand.SetHandler(ListItems);
rootCommand.Add(listCommand);

var popCommand = new Command("pop", "Eat the top layer of the cake.");
popCommand.SetHandler(PopItem);
rootCommand.Add(popCommand);

var pushCommand = new Command("push", "Add a layer onto the cake.");
pushCommand.AddArgument(layerArgument);
pushCommand.SetHandler((name) => PushItem(name!), layerArgument);
rootCommand.Add(pushCommand);

var cakeCommand = new Command("cake", "Manage cakes in the bakery.");

var setCakeCommand = new Command("set", "Set the current cake.");
setCakeCommand.AddArgument(cakeArgument);
setCakeCommand.SetHandler((name) => SetCake(name!), cakeArgument);
cakeCommand.Add(setCakeCommand);

var listCakeCommand = new Command("list", "List all cakes in the bakery.");
listCakeCommand.SetHandler(ListCakes);
cakeCommand.Add(listCakeCommand);

var removeCakeCommand = new Command("remove", "Remove a cake from the bakery.");
removeCakeCommand.AddArgument(cakeArgument);
removeCakeCommand.AddOption(removeAllOption);
removeCakeCommand.AddOption(removeCurrentOption);
removeCakeCommand.SetHandler((name, all, current) => RemoveCake(name!, all!, current!), cakeArgument, removeAllOption, removeCurrentOption);
cakeCommand.Add(removeCakeCommand);

cakeCommand.Handler = listCakeCommand.Handler;
rootCommand.Add(cakeCommand);

rootCommand.Handler = listCommand.Handler;

return await rootCommand.InvokeAsync(args);

void ClearItems() {
    File.WriteAllText(cakePath, "");
    Icing("No more layers in the cake.", ConsoleColor.Green);
}

void ListItems() {
    IceTheCake(GetTheCake());
}

void PopItem() {
    if (!File.Exists(cakePath)) {
        Icing("No layers in the cake.", ConsoleColor.Red);
        return;
    }

    Stack<string> cake = GetTheCake();

    if (cake.TryPop(out var item)) {
        Icing($"Popped \"{item}\".", ConsoleColor.Green);
    }

    IceTheCake(cake);
    BakeTheCake(cake);
}

void PushItem(string? name) {
    Stack<string> cake = GetTheCake();

    if (name == null) {
        Console.Write("Item name: ");
        string newName = Console.ReadLine();

        if (newName != null && newName.Length > 0) name = newName;
        else {
            Icing("Cancelled.", ConsoleColor.Red);
            IceTheCake(cake);
            return;
        }
    }

    cake.Push(name);
    IceTheCake(cake);
    BakeTheCake(cake);
}

void SetCake(string? name) {
    if (name == null) {
        Console.Write("Cake name: ");
        string newName = Console.ReadLine();

        if (newName != null && newName.Length > 0) name = newName;
        else {
            Icing("Cancelled.", ConsoleColor.Red);
            return;
        }
    }

    settings.Cake = name;
    settings.Save(settingsPath);
    cakePath = Path.Combine(cakesPath, $"{name}.txt");
    Icing($"Set the cake to \"{name}\".", ConsoleColor.Green);
    IceTheCake(GetTheCake());
}

void ListCakes() {
    _ = Directory.CreateDirectory(cakesPath);
    string[] cakeFiles = Directory.GetFiles(cakesPath, "*.txt");
    List<string> cakes = new();

    for (int i = 0; i < cakeFiles.Length; i++) {
        cakes.Add(Path.GetFileNameWithoutExtension(cakeFiles[i]));
    }

    if (cakes.Count == 0) {
        Icing("No cakes in the bakery.", ConsoleColor.Red);
        return;
    }

    for (int i = 0; i < cakes.Count; i++) {
        if (cakes[i] == settings.Cake) {
            Icing($"{cakes[i]} <--", ConsoleColor.Cyan);
        }
        else Console.WriteLine(cakes[i]);
    }
}

void RemoveCake(string? name, bool all = false, bool current = false) {
    if (all) {
        DirectoryInfo directory = new(cakesPath);
        FileInfo[] files = directory.GetFiles();
        Console.Write($"Remove all {files.Length} cakes? (y/n): ");
        if (Console.ReadLine().ToLower() == "y") {
            for (int i = 0; i < files.Length; i++) {
                File.Delete(files[i].FullName);
            }
            Icing("Removed all cakes.", ConsoleColor.Green);
            return;
        }
        else {
            Icing("Cancelled.", ConsoleColor.Red);
            return;
        }
    }
    if (current) {
        File.Delete(cakePath);
        Icing($"Removed \"{settings.Cake}\" cake.", ConsoleColor.Green);
        return;
    }
    if (name == null) {
        Console.Write("Cake name: ");
        string newName = Console.ReadLine();

        if (newName != null && newName.Length > 0) name = newName;
        else {
            Icing("Cancelled.", ConsoleColor.Red);
            return;
        }
    }

    if (File.Exists(Path.Combine(cakesPath, $"{name}.txt"))) {
        File.Delete(Path.Combine(cakesPath, $"{name}.txt"));
        Icing($"Removed the cake \"{name}\".", ConsoleColor.Green);
        IceTheCake(GetTheCake());
    }
    else {
        Icing($"The cake \"{name}\" does not exist.", ConsoleColor.Red);
    }
}

void IceTheCake(Stack<string> cake) {
    Console.Write("Cake: ");
    Icing(settings.Cake, ConsoleColor.Magenta);
    Console.WriteLine(new string('-', settings.Cake.Length + 6));

    if (cake.Count == 0) {
        Icing("No layers in the cake.", ConsoleColor.Red);
        return;
    }

    bool isFirst = true;
    for (int i = 0; i < cake.Count; i++) {
        string layer = cake.ElementAt(i);
        if (isFirst) {
            Icing($"{layer} <--", ConsoleColor.Cyan);
            isFirst = false;
        }
        else Console.WriteLine(layer);
    }
}

void BakeTheCake(Stack<string> cake) {
    _ = Directory.CreateDirectory(Path.GetDirectoryName(cakePath));
    using StreamWriter writer = new(cakePath);
    while (cake.Count > 0) {
        writer.WriteLine(cake.Pop());
    }
}

Stack<string> GetTheCake() {
    Stack<string> reversed = new();
    if (File.Exists(cakePath)) {
        foreach (string line in File.ReadLines(cakePath)) {
            reversed.Push(line);
        }
    }

    Stack<string> cake = new();
    while (reversed.Count > 0) {
        cake.Push(reversed.Pop());
    }

    return cake;
}

void Icing(string line, ConsoleColor color) {
    Console.ForegroundColor = color;
    Console.WriteLine(line);
    Console.ResetColor();
}
