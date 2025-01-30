
var staff_1 = Guid.NewGuid();
var staff_2 = Guid.NewGuid();

string counter_sign = "for_sign";
string counter_execution = "for_execution";

DbContextHelper.SeedCounter(staff_1);
DbContextHelper.SeedCounter(staff_2);

Func<Counter, bool> func_1 = c => c.MenuIdentifier == counter_sign && c.StaffId == staff_1;
Func<Counter, bool> func_2 = c => c.MenuIdentifier == counter_execution && c.StaffId == staff_1;

var parametr = Expression.Parameter(typeof(Counter), "c");

var expression_1 = Expression.Invoke(Expression.Constant(func_1), parametr);
var expression_2 = Expression.Invoke(Expression.Constant(func_2), parametr);

var expression_3 = Expression.OrElse(expression_1, expression_2);
var expression_4 = Expression.Lambda<Func<Counter, bool>>(expression_3, parametr).Compile();

var counters = _db.Counters.Where(expression_4).ToList();