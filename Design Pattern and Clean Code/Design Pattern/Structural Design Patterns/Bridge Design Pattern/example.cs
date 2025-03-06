public interface IFileOperations
{
    void WriteToFile(string fileName, string content);
    string ReadFromFile(string fileName);
}

public class WindowsFileOperations : IFileOperations
{
    public void WriteToFile(string fileName, string content)
    {
        File.WriteAllText(fileName, content);
        Console.WriteLine($"Windows: \"{fileName}\" fayliga yozildi.");
    }

    public string ReadFromFile(string fileName)
    {
        string content = File.ReadAllText(fileName);
        Console.WriteLine($"Windows: \"{fileName}\" faylidan o‘qildi.");
        return content;
    }
}

public class LinuxFileOperations : IFileOperations
{
    public void WriteToFile(string fileName, string content)
    {
        File.WriteAllText(fileName, content);
        Console.WriteLine($"Linux: \"{fileName}\" fayliga yozildi.");
    }

    public string ReadFromFile(string fileName)
    {
        string content = File.ReadAllText(fileName);
        Console.WriteLine($"Linux: \"{fileName}\" faylidan o‘qildi.");
        return content;
    }
}

public class FileWriter
{
    protected IFileOperations _fileOperations;

    public FileWriter(IFileOperations fileOperations)
    {
        _fileOperations = fileOperations;
    }

    public virtual void SaveData(string fileName, string content)
    {
        _fileOperations.WriteToFile(fileName, content);
    }

    public virtual string LoadData(string fileName)
    {
        return _fileOperations.ReadFromFile(fileName);
    }
}

public class EncryptedFileWriter : FileWriter
{
    public EncryptedFileWriter(IFileOperations fileOperations) : base(fileOperations) { }

    public override void SaveData(string fileName, string content)
    {
        string encryptedContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));
        Console.WriteLine("Ma’lumot shifrlanib saqlandi.");
        _fileOperations.WriteToFile(fileName, encryptedContent);
    }

    public override string LoadData(string fileName)
    {
        string encryptedContent = _fileOperations.ReadFromFile(fileName);
        string decryptedContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedContent));
        Console.WriteLine("Ma’lumot shifrdan yechildi.");
        return decryptedContent;
    }
}

class Program
{
    static void Main()
    {
        string fileName = "data.txt";
        string content = "Hello, Bridge Pattern";

        IFileOperations windowsOps = new WindowsFileOperations();
        IFileOperations linuxOps = new LinuxFileOperations();

        FileWriter windowsFileWriter = new FileWriter(windowsOps);
        windowsFileWriter.SaveData(fileName, content);
        Console.WriteLine("O‘qilgan ma’lumot: " + windowsFileWriter.LoadData(fileName));

        Console.WriteLine("\n---\n");

        EncryptedFileWriter linuxFileWriter = new EncryptedFileWriter(linuxOps);
        linuxFileWriter.SaveData(fileName, content);
        Console.WriteLine("O'qilgan ma'lumot: " + linuxFileWriter.LoadData(fileName));

        Console.ReadKey();
    }
}