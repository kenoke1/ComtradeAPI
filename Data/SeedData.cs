using ComtradeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ComtradeAPI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(CampaignDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Agents.AnyAsync())
            {
                return;

            }

            var agents = new[]
            {
                new Agent {AgentCode = "AGT01", Name = "Asim Asim", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agent {AgentCode = "AGT02", Name = "Basim Basim", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agent {AgentCode = "AGT03", Name = "Casim Casim", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agent {AgentCode = "AGT04", Name = "Dasim Dasim", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Agent {AgentCode = "AGT05", Name = "Easim Easim", IsActive = true, CreatedAt = DateTime.UtcNow },

            };
            await context.Agents.AddRangeAsync(agents);


            // seed users after agent are saved and get ids

            var users = new[]
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Password: Admin123!
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "alice",
                    Email = "alice@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent123!"), // Password: Agent123!
                    Role = "Agent",
                    AgentId = agents[0].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "bob",
                    Email = "bob@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                    Role = "Agent",
                    AgentId = agents[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "manager",
                    Email = "manager@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"), // Password: Manager123!
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }

            };
            await context.Users.AddRangeAsync(users);

            var customer = new[]
            {
                new Customer { CustomerId = "CUST01", Name = "Fudo Fudo", Email = "fudo.fudo@mail.com", PhoneNumber = "+1234567890", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },
                new Customer { CustomerId = "CUST02", Name = "Bfudo Bfudo", Email = "bfudo.bfudo@mail.com", PhoneNumber = "+1234567891", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },
                new Customer { CustomerId = "CUST03", Name = "Cfudo Cfudo", Email = "cfudo.cfudo@mail.com", PhoneNumber = "+1234567892", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },
                new Customer { CustomerId = "CUST04", Name = "Dfudo Dfudo", Email = "dfudo.dfudo@mail.com", PhoneNumber = "+1234567893", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },
                new Customer { CustomerId = "CUST05", Name = "Efudo Efudo", Email = "efudo.efudo@mail.com", PhoneNumber = "+1234567894", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },
                new Customer { CustomerId = "CUST06", Name = "Gfudo Gfudo", Email = "gfudo.gfudo@mail.com", PhoneNumber = "+1234567895", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },
                new Customer { CustomerId = "CUST05", Name = "Hfudo Hfudo", Email = "hfudo.hfudo@mail.com", PhoneNumber = "+1234567896", IsLoyalCustomer = true, CreatedAt = DateTime.UtcNow },

            };
            await context.Customers.AddRangeAsync(customer);

            await context.SaveChangesAsync();
            Console.WriteLine("Db seeded successfully!");
            Console.WriteLine("");
            Console.WriteLine("Default Users Created:");
            Console.WriteLine("   Admin    - Username: admin    | Password: Admin123!");
            Console.WriteLine("   Manager  - Username: manager  | Password: Manager123!");
            Console.WriteLine("   Agent    - Username: alice    | Password: Agent123!");
            Console.WriteLine("   Agent    - Username: bob      | Password: Agent123!");
        }
    }
}
