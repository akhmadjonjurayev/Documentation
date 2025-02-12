public interface ITransport
{
    void Deliver();
}

public class Truck : ITransport
{
    public void Deliver()
    {
        Console.WriteLine("Delivering by truck");
    }
}

public class Ship : ITransport
{
    public void Deliver()
    {
        Console.WriteLine("Delivering by ship");
    }
}

public class Bike : ITransport
{
    public void Deliver()
    {
        Console.WriteLine("Delivering by bike");
    }
}

public class Bus : ITransport
{
    public void Deliver()
    {
        Console.WriteLine("Delivering by bus");
    }
}

public class TransportFactory
{
    public static ITransport CreateTransport(string type)
    {
        switch (type.ToLower())
        {
            case "truck":
                return new Truck();
            case "ship":
                return new Ship();
            case "bike":
                return new Bike();
            case "bus":
                return new Bus();
            default:
                throw new ArgumentException($"Invalid transport type, {type}");
        }
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Transport turini tanlang (car/bike/bus): ");
        string transportType = Console.ReadLine();

        try
        {
            ITransport transport = TransportFactory.CreateTransport(transportType);
            transport.Deliver();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Xatolik: " + ex.Message);
        }

        Console.ReadKey();
    }
}