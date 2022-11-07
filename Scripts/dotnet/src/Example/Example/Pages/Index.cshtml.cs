using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Example.Models;
namespace AspNetCoreDemo.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PersonDbContext _person;
    [BindProperty]
    public List<Person> Persons {get;set;}

    public IndexModel(ILogger<IndexModel> logger, PersonDbContext person)
    {
        this._person = person;
        _logger = logger;
    }

    public void OnGet()
    {
        Persons = _person.Person.ToList();
    }
}
