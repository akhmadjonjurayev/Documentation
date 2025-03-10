public class PowerSupply
{
    public void TurnOn()
    {
        Console.WriteLine("Power Supply: Turned On.");
    }

    public void TurnOff()
    {
        Console.WriteLine("Power Supply: Turned Off.");
    }
}

public class CPU
{
    public void Start()
    {
        Console.WriteLine("CPU: Booting up...");
    }

    public void ShutDown()
    {
        Console.WriteLine("CPU: Shutting down...");
    }
}

public class Memory
{
    public void Load()
    {
        Console.WriteLine("Memory: Loading data...");
    }
}

public class HardDrive
{
    public void ReadData()
    {
        Console.WriteLine("Hard Drive: Reading system files...");
    }
}

/// <summary>
/// Kompyuter yoqish jarayonini soddalashtiruvchi facade class
/// </summary>
public class CompyuterFacade
{
    private PowerSupply _power;
    private CPU _cpu;
    private Memory _memory;
    private HardDrive _hardDrive;

    public CompyuterFacade()
    {
        _power = new();
        _cpu = new();
        _memory = new();
        _hardDrive = new();
    }

    public void StartComputer()
    {
        Console.WriteLine("\nStarting Computer...");
        _power.TurnOn();
        _cpu.Start();
        _memory.Load();
        _hardDrive.ReadData();
        Console.WriteLine("Computer Started Successfully!\n");
    }

    public void ShutDownComputer()
    {
        Console.WriteLine("Shutting Down Computer...");
        _cpu.ShutDown();
        _power.TurnOff();
        Console.WriteLine("Computer Shut Down Successfully!\n");
    }
}

class Program
{
    static void Main()
    {
        CompyuterFacade compyuter = new CompyuterFacade();

        compyuter.StartComputer();

        compyuter.ShutDownComputer();

        Console.ReadKey();
    }
}