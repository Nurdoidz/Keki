//          ╒═════════════════════════════════════════════════════════╕
//          │                          Keki                           │
//          │           A CLI application to manage stacks            │
//          ╘═════════════════════════════════════════════════════════╛
using System.CommandLine;

string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keki", "cake.txt");

var rootCommand = new RootCommand("Keki: Stack the cake, bake it.");

var nameArgument = new Argument<string?>(
        name: "name",
        description: "Name of the layer.");

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
pushCommand.AddArgument(nameArgument);
pushCommand.SetHandler((name) => PushItem(name!), nameArgument);
rootCommand.Add(pushCommand);

rootCommand.Handler = listCommand.Handler;

return await rootCommand.InvokeAsync(args);

void ClearItems() {
    File.WriteAllText(path, "");
    Icing("No more layers in the cake.", ConsoleColor.Green);
}

void ListItems() {
    if (!File.Exists(path)) {
        Icing("No layers in the cake.", ConsoleColor.Red);
        return;
    }

    Stack<string> cake = GetTheCake();

    if (cake.Count == 0) {
        Icing("No layers in the cake.", ConsoleColor.Red);
        return;
    }

    IceTheCake(cake);
}

void PopItem() {
    if (!File.Exists(path)) {
        Icing("No layers in the cake.", ConsoleColor.Red);
        return;
    }

    Stack<string> cake = GetTheCake();

    if (cake.TryPop(out var item)) {
        Icing($"Popped \"{item}\".", ConsoleColor.Green);

        if (cake.Count == 0) {
            Icing("No more layers in the cake.", ConsoleColor.Yellow);
        }

        IceTheCake(cake);

    }
    else {
        Icing("No layers in the cake.", ConsoleColor.Red);
    }

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

void IceTheCake(Stack<string> cake) {
    bool isFirst = true;
    foreach (string layer in cake) {
        if (isFirst) {
            Icing($"{layer} <--", ConsoleColor.Cyan);
            isFirst = false;
        }
        else Console.WriteLine(layer);
    }
}

void BakeTheCake(Stack<string> cake) {
    _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
    using StreamWriter writer = new(path);
    while (cake.Count > 0) {
        writer.WriteLine(cake.Pop());
    }
}

Stack<string> GetTheCake() {
    Stack<string> reversed = new();
    if (File.Exists(path)) {
        foreach (string line in File.ReadLines(path)) {
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


