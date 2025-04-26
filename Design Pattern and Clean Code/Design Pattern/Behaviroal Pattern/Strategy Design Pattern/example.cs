public interface ITaxStrategy
{
    decimal CalculateTax(decimal amount);
}

public class UzbekistanTaxStrategy : ITaxStrategy
{
    public decimal CalculateTax(decimal amount)
    {
        return amount * 0.12m; // 12% tax
    }
}

public class RussiaTaxStrategy : ITaxStrategy
{
    public decimal CalculateTax(decimal amount)
    {
        return amount * 0.13m; // 13% tax
    }
}

public class UKTaxStrategy : ITaxStrategy
{
    public decimal CalculateTax(decimal amount)
    {
        return amount * 0.20m; // 20% tax
    }
}

public class TaxCalculator
{
    private ITaxStrategy _taxStrategy;

    public TaxCalculator(ITaxStrategy taxStrategy)
    {
        _taxStrategy = taxStrategy;
    }

    public decimal CalculateTax(decimal amount)
    {
        return _taxStrategy.CalculateTax(amount);
    }
}

class Program
{
    static void Main()
    {
        decimal amount = 1000;

        // Har bir mintaqa uchun alohida strategiya
        var uzCalc = new TaxCalculator(new UzbekistanTaxStrategy());
        var usCalc = new TaxCalculator(new RussiaTaxStrategy());
        var ukCalc = new TaxCalculator(new UKTaxStrategy());

        Console.WriteLine($"🇺🇿 O‘zbekistonda jami: {uzCalc.CalculateTax(amount)}");
        Console.WriteLine($"🇺🇸 AQSHda jami: {usCalc.CalculateTax(amount)}");
        Console.WriteLine($"🇬🇧 Angliyada jami: {ukCalc.CalculateTax(amount)}");


        Console.ReadKey();
    }
}