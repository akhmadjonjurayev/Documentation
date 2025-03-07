
/// <summary>
/// Component
/// </summary>
public interface IFileSystem
{
    void Display(string indent);
}

/// <summary>
/// Leaf
/// </summary>
public class File : IFileSystem
{
    private string _name;

    public File(string name)
    {
        this._name = name;
    }

    public void Display(string indent)
    {
        Console.WriteLine($"{indent}- File: {_name}");
    }
}

public class Folder : IFileSystem
{
    private string _name;
    private List<IFileSystem> _children;

    public Folder(string name)
    {
        this._name = name;
        _children = new();
    }

    public void Add(IFileSystem child) =>
        _children.Add(child);


    public void Display(string indent)
    {
        Console.WriteLine($"{indent}+ Folder: {_name}");
        foreach (var component in _children)
        {
            component.Display(indent + "  ");
        }
    }
}

class Program
{
    static void Main()
    {
        // Fayllarni yaratish
        File file1 = new File("document.txt");
        File file2 = new File("photo.jpg");
        File file3 = new File("video.mp4");

        // Ichki papkalarni yaratish
        Folder subFolder1 = new Folder("My Pictures");
        subFolder1.Add(file2);

        Folder subFolder2 = new Folder("My Videos");
        subFolder2.Add(file3);

        // Asosiy papkani yaratish
        Folder rootFolder = new Folder("Root Folder");
        rootFolder.Add(file1);
        rootFolder.Add(subFolder1);
        rootFolder.Add(subFolder2);

        // Natijani ekranga chiqarish
        rootFolder.Display("Ahmadjon");
        Console.ReadKey();
    }
}