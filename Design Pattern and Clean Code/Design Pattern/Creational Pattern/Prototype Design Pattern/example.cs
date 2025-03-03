public interface IRopobotPrototype
{
    IRopobotPrototype Clone();
    void Show();
}

public class Robot : IRopobotPrototype
{
    public string Model { get; set; }
    public string Processor { get; set; }
    public int BatteryLife { get; set; }

    public Robot(string model, string processor, int batteryLife)
    {
        this.Model = model;
        this.Processor = processor;
        this.BatteryLife = batteryLife;
    }

    /// <summary>
    /// Shallow copy
    /// </summary>
    /// <returns></returns>
    public IRopobotPrototype Clone()
    {
        return (IRopobotPrototype)this.MemberwiseClone();
    }

    public void Show()
    {
        Console.WriteLine($"Model: {Model}, Processor: {Processor}, BatteryLife: {BatteryLife}");
    }
}

class Program
{
    static void Main()
    {
        // Asl robot obyektini yaratamiz
        Robot originalRobot = new Robot("T-800", "AI-Processor v1", 24);
        originalRobot.Show();

        // Obyekt nusxasini yaratamiz
        Robot clonedRobot = (Robot)originalRobot.Clone();
        clonedRobot.Model = "T-850"; // Modelni o‘zgartiramiz
        clonedRobot.Show();

        Console.ReadKey();
    }
}