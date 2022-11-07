using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.Models;
public class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> option) : base(option)
    {
         //var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
         Database.EnsureCreated();
    }
    public DbSet<Person> Person { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasData(new List<Person>{
            new Person()
            {
                FirstName = "Iroda",
                LastName = "Hasonova",
                Birthday = new DateTime(2001,5,11)
            },
            new Person(){
                FirstName = "Ahmadjon",
                LastName = "Jo'rayev",
                Birthday = new DateTime(2000,2,18)
            }
        });
        base.OnModelCreating(modelBuilder);
    }
}