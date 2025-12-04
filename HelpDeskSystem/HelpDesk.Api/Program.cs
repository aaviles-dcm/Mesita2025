using HelpDesk.Data;
using HelpDesk.Data.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Create database if not exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    if (!dbContext.Categories.Any())
    {
        dbContext.Categories.AddRange(
            new Category { Name = "Hardware", Description = "Hardware-related issues" },
            new Category { Name = "Software", Description = "Software installation and bugs" },
            new Category { Name = "Network", Description = "Network and connectivity issues" },
            new Category { Name = "Account", Description = "User account and access issues" },
            new Category { Name = "Other", Description = "Other issues" },
            new Category { Name = "Keyboard", Description = "Keyboars and Kits" }
        );
        dbContext.SaveChanges();
    }

    if (!dbContext.Users.Any())
    {
        dbContext.Users.AddRange(
            new User { UserId = Guid.NewGuid(), DisplayName = "Alice User", DomainUsername = "DOMAIN\\alice", Email = "alice@example.com", Role = UserRole.User, IsActive = true },
            new User { UserId = Guid.NewGuid(), DisplayName = "Bob User", DomainUsername = "DOMAIN\\bob", Email = "bob@example.com", Role = UserRole.User, IsActive = true },
            new User { UserId = Guid.NewGuid(), DisplayName = "Charlie Engineer", DomainUsername = "DOMAIN\\charlie", Email = "charlie@example.com", Role = UserRole.Engineer, IsActive = true },
            new User { UserId = Guid.NewGuid(), DisplayName = "David Admin", DomainUsername = "DOMAIN\\david", Email = "david@example.com", Role = UserRole.Administrator, IsActive = true },
            new User { UserId = Guid.NewGuid(), DisplayName = "Lucas Engieer", DomainUsername = "DOMAIN\\lucas", Email = "lucas@example.com", Role = UserRole.Engineer, IsActive = true }
        );
        dbContext.SaveChanges();
    }

    if (!dbContext.Tickets.Any())
    {
        var user = dbContext.Users.FirstOrDefault(u => u.DomainUsername == "DOMAIN\\alice");
        var engineer = dbContext.Users.FirstOrDefault(u => u.Role == UserRole.Engineer);
        var hardwareCategory = dbContext.Categories.FirstOrDefault(c => c.Name == "Hardware");
        var softwareCategory = dbContext.Categories.FirstOrDefault(c => c.Name == "Software");

        if (user != null && hardwareCategory != null && softwareCategory != null)
        {
            dbContext.Tickets.AddRange(
                new Ticket
                {
                    Title = "Laptop Overheating",
                    Description = "My laptop shuts down randomly.",
                    Status = TicketStatus.New,
                    CategoryId = hardwareCategory.CategoryId,
                    CreatedById = user.UserId,
                    DateCreated = DateTime.UtcNow,
                    Priority = 1
                },
                new Ticket
                {
                    Title = "Cannot Install VS Code",
                    Description = "Installation fails with error 123.",
                    Status = TicketStatus.InProgress,
                    CategoryId = softwareCategory.CategoryId,
                    CreatedById = user.UserId,
                    AssignedEngineerId = engineer?.UserId,
                    DateCreated = DateTime.UtcNow.AddDays(-1),
                    Priority = 2
                }
            );
            dbContext.SaveChanges();
        }
    }

    // Seed Engineer Categories
    var engineerUser = dbContext.Users.Include(u => u.Categories).FirstOrDefault(u => u.Role == UserRole.Engineer);
    var allCategories = dbContext.Categories.ToList();

    if (engineerUser != null && allCategories.Any())
    {
        foreach (var cat in allCategories)
        {
            if (!engineerUser.Categories.Any(c => c.CategoryId == cat.CategoryId))
            {
                engineerUser.Categories.Add(cat);
            }
        }
        dbContext.SaveChanges();
    }

    // Seed Passwords
    if (!dbContext.UserPasswords.Any())
    {
        var users = dbContext.Users.ToList();
        foreach (var u in users)
        {
            dbContext.UserPasswords.Add(new UserPassword { UserId = u.UserId, Password = "abc123456" });
        }
        dbContext.SaveChanges();
    }
}

app.Run();
