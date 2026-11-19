using Microsoft.EntityFrameworkCore;
using MvcAjaxModalCrud.Models;

namespace MvcAjaxModalCrud.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
    }
}