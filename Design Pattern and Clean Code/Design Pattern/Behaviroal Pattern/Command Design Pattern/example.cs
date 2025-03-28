public interface ItransactionCommand
{
    void Execute();
    void Undo();
}

public class BankAccount
{
    public string AccountHolder { get; set; }

    public decimal Balance { get; private set; }

    public BankAccount(string accountHolder, decimal balance)
    {
        this.AccountHolder = accountHolder;
        this.Balance = balance;
    }

    public void Deposit(decimal amount) => Balance += amount;

    public void WithDraw(decimal amount)
    {
        if (Balance >= amount)
        {
            Balance -= amount;
        }

        else
            Console.WriteLine($"Insufficient funds, Withdrawal of {amount} failed");
    }

    public void Transfer(BankAccount recipient, decimal amount)
    {
        if (Balance >= amount)
        {
            Balance -= amount;
            recipient.Balance += amount;
            Console.WriteLine($"{amount:C} transferred to {recipient.AccountHolder}. New balance: {Balance:C}");
        }
        else
        {
            Console.WriteLine($"Transfer of {amount:C} to {recipient.AccountHolder} failed due to insufficient funds.");
        }
    }
}

public class DepositCommand : ItransactionCommand
{
    private readonly BankAccount _account;
    private readonly decimal _amount;

    public DepositCommand(BankAccount account, decimal amount)
    {
        this._account = account;
        this._amount = amount;
    }

    public void Execute()
    {
        _account.Deposit(_amount);
    }

    public void Undo()
    {
        _account.WithDraw(_amount);
    }
}

public class WithDrawCommand : ItransactionCommand
{
    private readonly BankAccount _account;
    private readonly decimal _amount;

    public WithDrawCommand(BankAccount account, decimal amount)
    {
        this._account = account;
        this._amount = amount;
    }

    public void Execute()
    {
        _account.WithDraw(_amount);
    }

    public void Undo()
    {
        _account.Deposit(_amount);
    }
}

public class TransferCommand : ItransactionCommand
{
    private readonly BankAccount _from;
    private readonly BankAccount _to;
    private readonly decimal _amount;

    public TransferCommand(BankAccount from, BankAccount to, decimal amount)
    {
        this._from = from;
        this._to = to;
        this._amount = amount;
    }

    public void Execute()
    {
        _from.Transfer(_to, _amount);
    }

    public void Undo()
    {
        _to.Transfer(_from, _amount);
    }
}

public class TranzactionManager
{
    private Stack<ItransactionCommand> _transactionCommandHistory = new();

    public TranzactionManager()
    {
        
    }

    public void ExecutionTransaction(ItransactionCommand command)
    {
        command.Execute();
        _transactionCommandHistory.Push(command);
    }

    public void UndoLastTransaction()
    {
        if (_transactionCommandHistory.Count == 0)
        {
            Console.WriteLine("No transaction to undo");
            return;
        }

        var lastTransaction = _transactionCommandHistory.Pop();
        lastTransaction.Undo();
    }
}

class Program
{
    static void Main()
    {
        // Bank akkauntlarini yaratamiz
        BankAccount aliceAccount = new BankAccount("Alice", 5000);
        BankAccount bobAccount = new BankAccount("Bob", 2000);

        // Tranzaksiya menejerini yaratamiz
        TranzactionManager transactionManager = new();

        // 1. Alice 1000 dollar depozit qiladi
        ItransactionCommand deposit = new DepositCommand(aliceAccount, 1000);
        transactionManager.ExecutionTransaction(deposit);

        ItransactionCommand transfer = new TransferCommand(aliceAccount, bobAccount, 500);
        transactionManager.ExecutionTransaction(transfer);

        ItransactionCommand withdraw = new WithDrawCommand(aliceAccount, 2000);
        transactionManager.ExecutionTransaction(withdraw);

        Console.WriteLine("\n Undo Last transaction");
        transactionManager.UndoLastTransaction();

        Console.ReadKey();
    }
}