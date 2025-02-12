
public interface IButton
{
    void Render();
}

public interface ICheckBox
{
    void Check();
}

public interface IUIFactory
{
    IButton CreateButton();
    ICheckBox CreateCheckBox();
}

public class WindowsButton : IButton
{
    public void Render()
    {
        Console.WriteLine("Windows tugmasi chizildi");
    }
}

public class WindowsCheckBox : ICheckBox
{
    public void Check()
    {
        Console.WriteLine("Windows CheckBox belgilandi");
    }
}

public class MacOsButton : IButton
{
    public void Render()
    {
        Console.WriteLine("MacOs tugmasi chizildi");
    }
}

public class MacOsCheckBox : ICheckBox
{
    public void Check()
    {
        Console.WriteLine("MacOs CheckBox belgilandi");
    }
}

public class WindowsFactory : IUIFactory
{
    public IButton CreateButton()
    {
        return new WindowsButton();
    }

    public ICheckBox CreateCheckBox() => new WindowsCheckBox();
}

public class MacOsFactory : IUIFactory
{
    public IButton CreateButton()
    {
        return new MacOsButton();
    }

    public ICheckBox CreateCheckBox() => new MacOsCheckBox();
}

public class Application
{
    private readonly IButton _button;
    private readonly ICheckBox _checkBox;

    public Application(IUIFactory factory)
    {
        _button = factory.CreateButton();
        _checkBox = factory.CreateCheckBox();
    }

    public void Render()
    {
        _button.Render();
        _checkBox.Check();
    }
}

class Program
{
    static void Main()
    {
        IUIFactory factory;

        string osType = "Windows";

        if (osType == "Windows")
        {
            factory = new WindowsFactory();
        }
        else
        {
            factory = new MacOsFactory();
        }

        Application app = new Application(factory);
        app.Render();
        Console.ReadKey();
    }
}