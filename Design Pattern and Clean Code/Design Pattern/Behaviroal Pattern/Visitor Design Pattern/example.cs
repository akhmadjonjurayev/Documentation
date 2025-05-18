public interface IFileVisitor
{
    void Visit(TextFile textFile);
    void Visit(ImageFile imageFile);
}

public interface IFile
{
    void Accept(IFileVisitor visitor);
}

public class TextFile : IFile
{
    public string Name { get; set; }

    public TextFile(string name)
    {
        this.Name = name;
    }

    public void Accept(IFileVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ImageFile : IFile
{
    public string Name { get; set; }

    public ImageFile(string name)
    {
        this.Name = name;
    }

    public void Accept(IFileVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class ExportVisitor : IFileVisitor
{
    public void Visit(TextFile textFile)
    {
        Console.WriteLine($"Exporting text file: {textFile.Name}");
    }

    public void Visit(ImageFile imageFile)
    {
        Console.WriteLine($"Exporting image file: {imageFile.Name}");
    }
}

public class CompressVisitor : IFileVisitor
{
    public void Visit(TextFile textFile)
    {
        Console.WriteLine($"Compressing text file: {textFile.Name}");
    }

    public void Visit(ImageFile imageFile)
    {
        Console.WriteLine($"Compressing image file: {imageFile.Name}");
    }
}

class Program
{
    static void Main()
    {
        List<IFile> files = new List<IFile>()
        {
            new TextFile("Document.txt"),
            new ImageFile("Picture.jpg"),
        };

        var exportVisitor = new ExportVisitor();
        var compressVisitor = new CompressVisitor();

        foreach (var file in files)
        {
            file.Accept(exportVisitor);
            file.Accept(compressVisitor);
        }
        Console.ReadKey();
    }
}