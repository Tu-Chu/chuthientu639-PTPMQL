using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Models;
using MvcAjaxModalCrud.Models;

namespace DemoMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Person { get; set; }
        public DbSet<Khoa> Khoa { get; set; }
        public DbSet<SinhVien> SinhVien { get; set; }
        public DbSet<Employee> Employees => Set<Employee>();
    }
}