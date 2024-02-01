using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace ConsoleAppDop3hw5Core;

class Program
{
    static void Main()
    {
        using (var db = new ApplicationContext())
        {
            db.Database.EnsureCreated();
            InitializeMenuItems(db);
            var menuItems = db.MenuItems.Include(m => m.Children).ToList();
            // Получение корневых элементов меню
            var rootMenuItems = menuItems.Where(m => m.ParentId == null).ToList();
            // Функция для рекурсивного отображения иерархии 
            void DisplayMenu(IEnumerable<MenuItem> menuItems, int depth = 0)
            {
                foreach (var item in menuItems)
                {
                    Console.WriteLine($"{new string(' ', depth * 2)}{item.Title}");
                    DisplayMenu(item.Children, depth + 1); 
                }
            }
            DisplayMenu(rootMenuItems);
        }
    }
    private static void InitializeMenuItems(ApplicationContext db)
    {
        if (!db.MenuItems.Any())
        {
            var fileMenuItem = new MenuItem { Title = "File" };
            var editMenuItem = new MenuItem { Title = "Edit" };
            var viewMenuItem = new MenuItem { Title = "View" };

            var openMenuItem = new MenuItem { Title = "Open", Parent = fileMenuItem };
            var saveMenuItem = new MenuItem { Title = "Save", Parent = fileMenuItem };
            var saveAsMenuItem = new MenuItem { Title = "Save As", Parent = fileMenuItem };

            var toHardDriveMenuItem = new MenuItem { Title = "To hard-drive..", Parent = saveAsMenuItem };
            var toOnlineDriveMenuItem = new MenuItem { Title = "To online-drive..", Parent = saveAsMenuItem };

            db.MenuItems.AddRange(new[] { fileMenuItem, editMenuItem, viewMenuItem, openMenuItem, saveMenuItem, saveAsMenuItem, toHardDriveMenuItem, toOnlineDriveMenuItem });
            db.SaveChanges();
        }
    }

    public class MenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? ParentId { get; set; }
        public MenuItem Parent { get; set; }
        public ICollection<MenuItem> Children { get; set; }

        public MenuItem()
        {
            Children = new HashSet<MenuItem>();
        }
    }
    public class ApplicationContext : DbContext
    {
        public DbSet<MenuItem> MenuItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-4PCU5RA\\SQLEXPRESS;Database=MenuItem;Trusted_Connection=True;TrustServerCertificate=True;");
                optionsBuilder.LogTo(e => Debug.WriteLine(e), new[] { RelationalEventId.CommandExecuted });
            }
        }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
        
    }
}
