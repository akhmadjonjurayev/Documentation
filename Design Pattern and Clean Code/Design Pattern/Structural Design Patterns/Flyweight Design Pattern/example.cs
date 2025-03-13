public class TreeType
{
    public string Name { get; set; }

    public string Color { get; set; }

    public string Texture { get; set; }

    public TreeType(string name, string color, string texture)
    {
        this.Color = color;
        this.Name = name;
        this.Texture = texture;
    }

    public void Display(int x, int y)
    {
        Console.WriteLine($"TreeType: {Name}, Color: {Color}, Texture: {Texture} at ({x},{y})");
    }
}

public class TreeFactory
{
    public static Dictionary<string, TreeType> _treeTypes = new();

    public static TreeType GetTreeType(string name, string color, string texture)
    {
        string key = string.Format("{0}_{1}_{2}", name, color, texture);
        if (!_treeTypes.ContainsKey(key))
        {
            _treeTypes[key] = new TreeType(name, color, texture);
            Console.WriteLine("Creating new TreeType, key - {0}", key);
        }

        return _treeTypes[key];
    }
}

public class Tree
{
    private int _x;

    private int _y;

    private TreeType _treeType;

    public Tree(int x, int y, TreeType treeType)
    {
        this._x = x;
        this._y = y;
        this._treeType = treeType;
    }

    public void Display()
    {
        _treeType.Display(_x, _y);
    }
}


class Program
{
    static void Main()
    {
        var forest = new List<Tree>(1_000_000);

        for (int i = 0; i < 1_000_000; i++)
        {
            int x = Random.Shared.Next(0, 1000);
            int y = Random.Shared.Next(0, 1000);

            TreeType treeType = TreeFactory.GetTreeType("Oak", "Green", "Smooth");
            forest.Add(new Tree(x, y, treeType));
        }

        for (int i = 0; i < 5; i++)
        {
            forest[i].Display();
        }

        Console.WriteLine($"Total Tree Types Created: {TreeFactory._treeTypes.Count}");
        Console.ReadKey();
    }
}