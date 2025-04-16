public interface ISubscriber
{
    void Notify(string message);
}

/// <summary>
/// ISubject interfaysi
/// </summary>
public interface ITelegramChennel
{
    void Attach(ISubscriber subscriber);
    void Detach(ISubscriber subscriber);
    void Notify(string message);
}

public class Subscriber : ISubscriber
{
    public string Name { get; set; }

    public Subscriber(string name)
    {
        this.Name = name;
    }

    public void Notify(string message)
    {
        Console.WriteLine($"{Name} received message: {message}");
    }
}

public class TelegramChannel : ITelegramChennel
{
    public string Name { get; set; }

    private List<ISubscriber> subscribers = new List<ISubscriber>();

    public TelegramChannel(string name)
    {
        this.Name = name;
    }

    public void Attach(ISubscriber subscriber)
    {
        subscribers.Add(subscriber);
    }

    public void Detach(ISubscriber subscriber)
    {
        subscribers.Remove(subscriber);
    }

    public void Notify(string message)
    {
        Console.WriteLine($"New message in {Name} channel: {message}");
        foreach (var subscriber in subscribers)
        {
            subscriber.Notify(message);
        }
    }
}

class Program
{
    static void Main()
    {
        var channel = new TelegramChannel("Yulduzlar aybdor");

        var subscriber1 = new Subscriber("Ali");
        var subscriber2 = new Subscriber("Vali");
        var subscriber3 = new Subscriber("Olim");

        channel.Attach(subscriber1);
        channel.Attach(subscriber2);
        channel.Attach(subscriber3);

        channel.Notify("Salom, bugun Yulduzlar aybdor kanalida yangi xabar bor!");

        Console.ReadKey();
    }
}