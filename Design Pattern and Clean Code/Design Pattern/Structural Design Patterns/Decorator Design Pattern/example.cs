/// <summary>
/// Umumiy interface
/// </summary>
public interface IDrink
{
    string GetDescription();
    double GetCost();
}

public class Coffee : IDrink
{
    public double GetCost()
    {
        return 2.00;
    }

    public string GetDescription()
    {
        return "Sample Coffee";
    }
}

/// <summary>
/// Ichimliklarga qo'shimchalar qo'shish uchun decorator
/// </summary>
public abstract class DrinkDecorator : IDrink
{
    private readonly IDrink _drink;

    protected DrinkDecorator(IDrink drink)
    {
        this._drink = drink;
    }

    public virtual double GetCost()
    {
        return _drink.GetCost();
    }

    public virtual string GetDescription()
    {
        return _drink.GetDescription();
    }
}

public class Sugar : DrinkDecorator
{
    public Sugar (IDrink drink) : base(drink)
    { }

    public override string GetDescription()
    {
        return base.GetDescription() + ", Sugar";
    }

    public override double GetCost()
    {
        return base.GetCost() + 0.50;
    }
}

public class Milk : DrinkDecorator
{
    public Milk(IDrink drink) : base(drink)
    {
        
    }

    public override string GetDescription()
    {
        return base.GetDescription() + ", Milk";
    }

    public override double GetCost()
    {
        return base.GetCost() + 1.00;
    }
}

public class Chocolate : DrinkDecorator
{
    public Chocolate(IDrink drink) : base(drink)
    {
        
    }

    public override string GetDescription()
    {
        return base.GetDescription() + ", Chocolate";
    }

    public override double GetCost()
    {
        return base.GetCost() + 1.50;
    }
}

class Program
{
    static void Main()
    {
        IDrink coffee = new Coffee();
        Console.WriteLine($"{coffee.GetDescription()} - {coffee.GetCost()}");

        IDrink coffeeWithSugar = new Sugar(coffee);
        Console.WriteLine($"{coffeeWithSugar.GetDescription()} - {coffeeWithSugar.GetCost()}");

        IDrink coffeeWithMilk = new Milk(coffeeWithSugar);
        Console.WriteLine($"{coffeeWithMilk.GetDescription()} - {coffeeWithMilk.GetCost()}");

        IDrink coffeeWithChocolate = new Chocolate(coffeeWithMilk);
        Console.WriteLine($"{coffeeWithChocolate.GetDescription()} - {coffeeWithChocolate.GetCost()}");
        Console.ReadKey();
    }
}