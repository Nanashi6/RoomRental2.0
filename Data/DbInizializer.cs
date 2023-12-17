using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RoomRental.Models;
using System;

namespace RoomRental.Data
{
    public static class DbInizializer
    {
        public static async Task Inizialize(RoomRentalsContext context, UserManager<User> userManager)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            if (!context.Organizations.Any())
            {
                for (int i = 0; i < 100; i++)
                {
                    context.Add(new Organization() { Name = $"Organization {i + 1}", PostalAddress = $"Address {i + 1}" });
                }
                context.SaveChanges();
            }

            var organizations = context.Organizations.ToArray();
            if (!context.Users.Any()) 
            {
                /*var admin = new User
                {
                    Surname = $"Ёвженко",
                    Name = $"Юрий",
                    Lastname = $"Дмитриевич",
                    UserName = $"Admin",
                    Email = $"admin@mail.com",
                    OrganizationId = organizations[0].OrganizationId,
                    Organization = organizations[0]
                };
                var resultAdmin = await userManager.CreateAsync(admin, "123456");
                if (resultAdmin.Succeeded)
                {
                    var res = await userManager.AddToRolesAsync(admin, new List<string>() { "User", "Admin" });
                }*/

                for (int i = 0; i < 100; i++)
                {
                    var user = new User
                    {
                        Surname = $"Surname{i+1}",
                        Name = $"Name{i+1}",
                        Lastname = $"Lastname{i+1}",
                        UserName = $"User{i + 1}",
                        Email = $"user{i + 1}@mail.com",
                        OrganizationId = organizations[i].OrganizationId,
                        Organization = organizations[i]
                    };

                    var result = await userManager.CreateAsync(user, "123456");
                    if (result.Succeeded)
                    {
                        var res = await userManager.AddToRolesAsync(user, new List<string>() { "User" });
                    }
                }
            }

            if (!context.Buildings.Any())
            {
                for (int i = 0; i < 500; i++)
                {
                    int ownerId = rnd.Next(organizations.Count()) + 1;
                    context.Buildings.Add(new Building() { Name = $"Building {i + 1}", PostalAddress = $"Address {i + 1}", Floors = rnd.Next(11), Description = $"Description {i + 1}", OwnerOrganizationId = ownerId, FloorPlan = $"\\Images\\FloorPlans\\1.jpg", OwnerOrganization = organizations[ownerId - 1] });
                }
                context.SaveChanges();
            }

            var buildings = context.Buildings.ToArray();
            if (!context.Rooms.Any())
            {

                for (int i = 1; i <= buildings.Length; i++)
                {
                    for (int j = 0; j < 10; j++)
                        context.Add(new Room() { Area = rnd.Next(50, 100), BuildingId = i, RoomNumber = j + 1, Description = $"Description {(i - 1) * 10 + (j + 1)}", Building = context.Buildings.ToArray()[i - 1] });
                }

                context.SaveChanges();
            }

            var rooms = context.Rooms.ToArray();
            if (!context.RoomImages.Any())
            {
                for (int i = 0; i < 5000; i++)
                {
                    int roomId = i + 1;
                    context.Add(new RoomImage() { RoomId = roomId, ImagePath = "\\Images\\Rooms\\1.jpg", Room = rooms[roomId - 1] });
                }
                context.SaveChanges();
            }

            if (!context.Rentals.Any())
            {
                for (int i = 0; i < 10000; i++)
                {
                    int rentalOrgId = rnd.Next(organizations.Count()) + 1;
                    DateTime checkInDate = GenerateRandomDate();
                    DateTime checkOutDate = checkInDate.AddMonths(1);
                    DateTime paymentDate = GenerateRandomDate(checkInDate, checkOutDate);

                    int roomId = rnd.Next(rooms.Count());
                    var room = rooms[roomId];
                    
                    var building = buildings.Single(b => b.BuildingId == room.BuildingId);

                    var organization = organizations.Single(e => e.OrganizationId == building.OwnerOrganizationId);

                    var person = context.Users.Single(e => e.OrganizationId == organization.OrganizationId);

                    decimal amount = room.Area * 3;

                    context.Add(new Rental() 
                    { 
                        RoomId = room.RoomId, 
                        RentalOrganizationId = rentalOrgId, 
                        CheckInDate = checkInDate, 
                        CheckOutDate = checkOutDate,
                        Amount = amount,
                        Room = room, 
                        RentalOrganization = organizations[rentalOrgId-1] 
                    });
                    context.Add(new Invoice() 
                    { 
                        RoomId = room.RoomId,
                        RentalOrganizationId = rentalOrgId, 
                        ConclusionDate = checkInDate, 
                        ResponsiblePersonId = person.Id, 
                        PaymentDate = paymentDate,
                        Amount = GetModifiedAmount(amount),
                        Room = room,
                        RentalOrganization = organizations[rentalOrgId - 1], 
                        ResponsiblePerson = person
                    });
                }

                var admin = new User
                {
                    Surname = $"Ёвженко",
                    Name = $"Юрий",
                    Lastname = $"Дмитриевич",
                    UserName = $"Admin",
                    Email = $"admin@mail.com",
                    OrganizationId = organizations[0].OrganizationId,
                    Organization = organizations[0]
                };
                var resultAdmin = await userManager.CreateAsync(admin, "123456");
                if (resultAdmin.Succeeded)
                {
                    var res = await userManager.AddToRolesAsync(admin, new List<string>() { "User", "Admin" });
                }

                context.SaveChanges();
            }
        }  
        private static DateTime GenerateRandomDate(DateTime? minDate = null, DateTime? maxDate = null)
        {
            Random random = new(Guid.NewGuid().GetHashCode());
            DateTime startDate = minDate ?? new DateTime(2021, 1, 1);
            DateTime endDate = maxDate ?? new DateTime(2023, 12, 31);

            TimeSpan timeSpan = endDate - startDate;
            TimeSpan randomSpan = new TimeSpan((long)(random.NextDouble() * timeSpan.Ticks));
            DateTime randomDate = startDate + randomSpan;

            return randomDate;
        }
        public static decimal GetModifiedAmount(decimal amount)
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 101); // Генерируем случайное число от 1 до 100

            if (randomNumber <= 75)
            {
                // 75% случаев: возвращаем исходное значение amount
                return amount;
            }
            else
            {
                // 25% случаев: возвращаем значение, меньшее, чем amount
                decimal modifiedAmount = amount * 0.75m; // Пример: 75% от исходного значения
                return modifiedAmount;
            }
        }
    }
}
