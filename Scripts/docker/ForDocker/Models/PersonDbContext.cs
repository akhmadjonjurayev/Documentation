using Microsoft.EntityFrameworkCore;
public class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
          .HasData(new List<Person>{
             new Person{
                PersonId = Guid.NewGuid(),
                FirstName = "Akhmadjon",
                LastName = "Jurayev",
                BirthDay = new DateTime(2000, 02, 18)
             }
          });
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Person> Persons {get; set;}
}