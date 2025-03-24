public class Customer
{
    public string Name { get; set; }

    public int Income { get; set; }

    public int CreditScore { get; set; }

    public bool HasGuarantor { get; set; }

    public Customer(string name, int income, int creditScore, bool hasguarantor)
    {
        this.Name = name;
        this.Income = income;
        this.CreditScore = creditScore;
        this.HasGuarantor = hasguarantor;
    }
}

public abstract class CreditApprovalHandler
{
    protected CreditApprovalHandler _nextHandler;

    public void SetNextHandler(CreditApprovalHandler nextHandler)
    {
        _nextHandler = nextHandler;
    }

    public abstract void HandleRequest(Customer customer);
}

public class Incomehandler : CreditApprovalHandler
{
    public override void HandleRequest(Customer customer)
    {
        if (customer.Income > 50000)
        {
            Console.WriteLine("Income handler approved the request ✅");
            if (_nextHandler != null)
            {
                _nextHandler.HandleRequest(customer);
            }
        }
        else
        {
            Console.WriteLine("Income handler denied the request ❌");
        }
    }
}

public class CreditScoreHandler : CreditApprovalHandler
{
    public override void HandleRequest(Customer customer)
    {
        if (customer.CreditScore > 700)
        {
            Console.WriteLine("CreditHistoryCheck: Approved ✅");
            if (_nextHandler != null)
            {
                _nextHandler.HandleRequest(customer);
            }
        }
        else
        {
            Console.WriteLine("CreditHistoryCheck: Denied ❌");
        }
    }
}

public class GuarantorHandler : CreditApprovalHandler
{
    public override void HandleRequest(Customer customer)
    {
        if (customer.HasGuarantor)
        {
            Console.WriteLine("GuarantorHandler: Approved ✅");
            if (_nextHandler != null)
            {
                Console.WriteLine("GuarantorCheck: Approved ✅");
                Console.WriteLine("Final Decision: Loan Approved! 🎉");
            }
        }
        else
        {
            Console.WriteLine("GuarantorHandler: Denied ❌");
        }
    }
}

class Program
{
    static void Main()
    {

        Customer customer = new Customer("John", 60000, 750, true);

        CreditApprovalHandler incomeCheck = new Incomehandler();
        CreditApprovalHandler creditCheck = new CreditScoreHandler();
        CreditApprovalHandler guarantorCheck = new GuarantorHandler();

        incomeCheck.SetNextHandler(creditCheck);
        creditCheck.SetNextHandler(guarantorCheck);

        Console.WriteLine($"Processing Loan Request for {customer.Name}...\n");
        incomeCheck.HandleRequest(customer);

        Console.ReadKey();
    }
}