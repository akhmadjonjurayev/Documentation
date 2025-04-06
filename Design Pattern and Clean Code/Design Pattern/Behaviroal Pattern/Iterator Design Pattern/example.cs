public class Employee
{
    public string Name
    {
        get; set;
    }

    public Employee(string name)
    {
        this.Name = name;
    }
}

public interface Iterator
{
    bool HasNext();
    Employee Next();
}

public interface IEmployeeCollection
{
    Iterator CreateIterator();
}

public class DepartmentIterator : Iterator
{
    private List<Employee> _employees;
    private int _position = 0;

    public DepartmentIterator(List<Employee> employees)
    {
        this._employees = employees;
    }

    public bool HasNext()
    {
        return _position < _employees.Count;
    }

    public Employee Next()
    {
        return _employees[_position++];
    }
}

public class ITDepartment : IEmployeeCollection
{
    private List<Employee> _employees;

    public ITDepartment()
    {
        _employees = new List<Employee>();
    }

    public void AddEmployee(Employee employee)
    {
        _employees.Add(employee);
    }

    public Iterator CreateIterator()
    {
        return new DepartmentIterator(_employees);
    }
}

public class HrDepartmentIterator : Iterator
{
    private Employee[] _employees;
    private int _position = 0;

    public HrDepartmentIterator(Employee[] employees)
    {
        this._employees = employees;
    }

    public bool HasNext()
    {
        return _position < _employees.Length && _employees[_position] != null;
    }

    public Employee Next()
    {
        return _employees[_position++];
    }
}

public class HrDepartment : IEmployeeCollection
{
    private Employee[] _employees;
    private int _index = 0;

    public HrDepartment(int size)
    {
        _employees = new Employee[size];
    }

    public void AddEmployee(Employee employee)
    {
        if (_index < _employees.Length)
        {
            _employees[_index++] = employee;
        }
        else
        {
            Console.WriteLine("No more space to add employees.");
        }
    }

    public Iterator CreateIterator()
    {
        return new HrDepartmentIterator(_employees);
    }
}

class Program
{
    static void Main()
    {
        ITDepartment department = new ITDepartment();
        department.AddEmployee(new Employee("John Doe"));
        department.AddEmployee(new Employee("Jane Smith"));

        HrDepartment hrDepartment = new HrDepartment(5);
        hrDepartment.AddEmployee(new Employee("Alice Johnson"));
        hrDepartment.AddEmployee(new Employee("Bob Brown"));
        hrDepartment.AddEmployee(new Employee("Charlie Davis"));
        hrDepartment.AddEmployee(new Employee("Diana Prince"));
        hrDepartment.AddEmployee(new Employee("Eve Adams"));

        Console.WriteLine("IT Department:");
        PrintDepartment(department.CreateIterator());

        Console.WriteLine("HR Department:");
        PrintDepartment(hrDepartment.CreateIterator());

        Console.ReadKey();
    }

    private static void PrintDepartment(Iterator iterator)
    {
        while (iterator.HasNext())
        {
            var employee = iterator.Next();
            Console.WriteLine($"Employee: {employee.Name}");
        }
    }
}