public class Burger
{
    public string Bread { get; set; }

    public string Meat { get; set; }

    public string Sauce { get; set; }

    public bool Cheese { get; set; }

    public bool Lettuce { get; set; }

    public void Show()
    {
        Console.WriteLine($"Burger {Bread} non, {Meat} go'sht, {Sauce} souse, " +
            $"{(Cheese ? "pishloq bor" : "pishloq yo'q")}, " +
            $"{(Lettuce ? "salat bor" : "salat yo'q")}");
    }
}

public interface IBurgerBuilder
{
    IBurgerBuilder SetBread(string bread);
    IBurgerBuilder SetMeat(string meat);
    IBurgerBuilder SetSauce(string sauce);
    IBurgerBuilder AddCheese(bool cheese);
    IBurgerBuilder AddLettuce(bool lettuce);
    Burger Build();
}

public class BurgerBuilder : IBurgerBuilder
{
    private Burger _burger = new Burger();

    public IBurgerBuilder SetBread(string bread)
    {
        _burger.Bread = bread;
        return this;
    }

    public IBurgerBuilder SetMeat(string meat)
    {
        _burger.Meat = meat;
        return this;
    }

    public IBurgerBuilder SetSauce(string sauce)
    {
        _burger.Sauce = sauce;
        return this;
    }

    public IBurgerBuilder AddCheese(bool cheese)
    {
        _burger.Cheese = cheese;
        return this;
    }

    public IBurgerBuilder AddLettuce(bool lettuce)
    {
        _burger.Lettuce = lettuce;
        return this;
    }

    public Burger Build()
    {
        return _burger;
    }
}

public class BurgerMaster
{
    public Burger MakeCheeseBurger(IBurgerBuilder builder)
    {
        return builder.SetBread("O'rtacha non")
            .SetMeat("Mol go'shti")
            .SetSauce("Mayonez")
            .AddCheese(true)
            .AddLettuce(true)
            .Build();
    }

    public Burger MakeChikenBurger(IBurgerBuilder builder)
    {
        return builder.SetBread("Qalin non")
            .SetMeat("Tovuq go'shti")
            .SetSauce("Ketchup")
            .AddCheese(false)
            .AddLettuce(true)
            .Build();
    }
}

class Program
{
    static void Main()
    {
        BurgerMaster master = new();
        IBurgerBuilder builder = new BurgerBuilder();

        Console.WriteLine("Cheese Burger tayyorlanmoqda ....");
        Burger cheeseBurger = master.MakeCheeseBurger(builder);
        cheeseBurger.Show();

        Console.WriteLine("Chicken Burger tayyorlanmoqda ....");
        Burger chickenBurger = master.MakeChikenBurger(builder);
        chickenBurger.Show();

        Console.ReadKey();
    }
}