using Microsoft.EntityFrameworkCore;
using ContactManager.Models;
using ContactManager.Models.ContactManager.Models;

namespace ContactManager
{
    public class ContactDbContext : DbContext
    {
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=contacts.db");
        }

        public ContactDbContext()
        {
            // Ensures the database is created and applies any pending migrations
            Database.Migrate();
        }
    }
}