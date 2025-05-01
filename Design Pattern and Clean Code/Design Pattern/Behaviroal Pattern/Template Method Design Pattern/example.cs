public abstract class DataProcessor
{
    public void Process()
    {
        Connect();
        FetchData();
        ParseData();
        DisConnect();
    }

    protected abstract void Connect();
    protected abstract void FetchData();
    protected abstract void ParseData();
    public virtual void DisConnect()
    {
        Console.WriteLine("Disconnecting from source...");
    }
}

public class DatabaseProcessor : DataProcessor
{
    protected override void Connect()
    {
        Console.WriteLine("Connection database ...");
    }

    protected override void FetchData()
    {
        Console.WriteLine("Fetching data from database ...");
    }

    protected override void ParseData()
    {
        Console.WriteLine("Parsing database data ...");
    }
}

public class ApiProcessor : DataProcessor
{
    protected override void Connect()
    {
        Console.WriteLine("Connection to API ...");
    }

    protected override void FetchData()
    {
        Console.WriteLine("Fetching data from API ...");
    }

    protected override void ParseData()
    {
        Console.WriteLine("Parsing API data ...");
    }
}

class Program
{
    static void Main()
    {
        DataProcessor databaseProcessor = new DatabaseProcessor();
        databaseProcessor.Process();

        Console.WriteLine("--------------------");

        DataProcessor apiProcessor = new ApiProcessor();
        apiProcessor.Process();

        Console.ReadKey();
    }
}