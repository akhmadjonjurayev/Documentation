public interface IATMState
{
    void InsertCard();
    void EjectCard();
    void EnterPin(int pin);
    void WithdrawCash(int amount);
}

public class NoCardState : IATMState
{
    private readonly ATMContext _atmContext;

    public NoCardState(ATMContext atmContext)
    {
        _atmContext = atmContext;
    }

    public void InsertCard()
    {
        Console.WriteLine("Card Inserted.");
        _atmContext.SetState(_atmContext.HasCardState);
    }

    public void EjectCard()
    {
        Console.WriteLine("No card to eject.");
    }

    public void EnterPin(int pin)
    {
        Console.WriteLine("Insert card first");
    }

    public void WithdrawCash(int amount)
    {
        Console.WriteLine("Insert card first");
    }
}

public class HasCardState : IATMState
{
    private readonly ATMContext _atmContext;

    public HasCardState(ATMContext aTMContext)
    {
        this._atmContext = aTMContext;
    }

    public void InsertCard()
    {
        Console.WriteLine("Card already inserted.");
    }

    public void EjectCard()
    {
        Console.WriteLine("Card Ejected.");
        _atmContext.SetState(_atmContext.NoCardState);
    }

    public void EnterPin(int pin)
    {
        if (pin == 7777)
        {
            Console.WriteLine("Correct PIN.");
            _atmContext.SetState(_atmContext.AuthorizedState);
        }
        else Console.WriteLine("Incorrect PIN.");
    }

    public void WithdrawCash(int amount)
    {
        Console.WriteLine("Enter PIN first.");
    }
}

public class AuthorizedState : IATMState
{
    private readonly ATMContext _atmContext;

    public AuthorizedState(ATMContext atmContext)
    {
        _atmContext = atmContext;
    }

    public void InsertCard()
    {
        Console.WriteLine("Card already inserted.");
    }

    public void EjectCard()
    {
        Console.WriteLine("Card Ejected.");
        _atmContext.SetState(_atmContext.NoCardState);
    }

    public void EnterPin(int pin)
    {
        Console.WriteLine("Already authorized.");
    }

    public void WithdrawCash(int amount)
    {
        if (_atmContext.CashAvailable >= amount)
        {
            Console.WriteLine($"Withdrawing {amount}.");
            _atmContext.CashAvailable -= amount;

            if (_atmContext.CashAvailable == 0)
            {
                Console.WriteLine("Out of cash.");
                _atmContext.SetState(_atmContext.OutOfMoneyState);
            }
        }
        else Console.WriteLine("Not enough cash available.");
    }
}

public class OutOfMoneyState : IATMState
{
    private readonly ATMContext _atmContext;

    public OutOfMoneyState(ATMContext atmContext)
    {
        _atmContext = atmContext;
        _atmContext.SetState(_atmContext.NoCardState);
    }

    public void InsertCard()
    {
        Console.WriteLine("ATM is out of cash.");
    }

    public void EjectCard()
    {
        Console.WriteLine("No card to eject.");
    }

    public void EnterPin(int pin)
    {
        Console.WriteLine("ATM is out of cash.");
    }

    public void WithdrawCash(int amount)
    {
        Console.WriteLine("ATM is out of cash.");
    }
}

public class ATMContext
{
    public readonly IATMState NoCardState;
    public readonly IATMState HasCardState;
    public readonly IATMState AuthorizedState;
    public readonly IATMState OutOfMoneyState;

    private IATMState currentState;
    public int CashAvailable { get; set; }

    public ATMContext()
    {
        NoCardState = new NoCardState(this);
        HasCardState = new HasCardState(this);
        AuthorizedState = new AuthorizedState(this);
        OutOfMoneyState = new OutOfMoneyState(this);
    }

    public void SetState(IATMState state)
    {
        currentState = state;
    }

    public void InsertCard() => currentState.InsertCard();
    public void EjectCard() => currentState.EjectCard();
    public void EnterPin(int pin) => currentState.EnterPin(pin);
    public void WithdrawCash(int amount) => currentState.WithdrawCash(amount);
}

class Program
{
    static void Main()
    {
        ATMContext atm = new ATMContext();

        atm.CashAvailable = 1000;

        atm.InsertCard();
        atm.EnterPin(7777);
        atm.WithdrawCash(200);
        atm.WithdrawCash(900);
        atm.WithdrawCash(800);

        atm.InsertCard();
        atm.EjectCard();


        Console.ReadKey();
    }
}