public interface ITypeCPrinter
{
    void PrintDocument(string FullName);
}

public interface IUSBPrinter
{
    void Print(string FirstName, string LastName);
}

public class TypeCPrinter : ITypeCPrinter
{
    public void PrintDocument(string FullName)
    {
        Console.WriteLine("Printing document: " + FullName);
    }
}

public class USBPrinterAdapter : IUSBPrinter
{
    private readonly ITypeCPrinter _typeCPrinter;

    public USBPrinterAdapter(ITypeCPrinter typeCPrinter)
    {
        this._typeCPrinter = typeCPrinter;
    }

    public void Print(string FirstName, string LastName)
    {
        _typeCPrinter.PrintDocument($"{FirstName} {LastName}");
    }
}

class Program
{
    static void Main()
    {
        TypeCPrinter typeCPrinter = new TypeCPrinter();

        IUSBPrinter usbPrinter = new USBPrinterAdapter(typeCPrinter);

        usbPrinter.Print("John", "Doe");

        Console.ReadKey();
    }
}