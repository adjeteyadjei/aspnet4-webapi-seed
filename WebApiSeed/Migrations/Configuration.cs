namespace WebApiSeed.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Models.AppDbContext>
    {
        public UserManager<User> UserManager { get; private set; }

        public Configuration()
            : this(new UserManager<User>(new UserStore<User>(new AppDbContext())))
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        public Configuration(UserManager<User> userManager) { UserManager = userManager; }

        protected override void Seed(AppDbContext context)
        {
            #region Roles [Privileges]
            var roles = new List<IdentityRole>
                        {
                            new IdentityRole {Name = Privileges.CanViewDashboard},
                            new IdentityRole {Name = Privileges.CanViewReport},
                            new IdentityRole {Name = Privileges.CanViewSetting},
                            new IdentityRole {Name = Privileges.CanViewAdministration},
                            new IdentityRole {Name = Privileges.CanViewUser},
                            new IdentityRole {Name = Privileges.CanCreateUser},
                            new IdentityRole {Name = Privileges.CanUpdateUser},
                            new IdentityRole {Name = Privileges.CanDeleteUser},
                            new IdentityRole {Name = Privileges.CanViewRole},
                            new IdentityRole {Name = Privileges.CanCreateRole},
                            new IdentityRole {Name = Privileges.CanUpdateRole},
                            new IdentityRole {Name = Privileges.CanDeleteRole},

                        };

            roles.ForEach(r => context.Roles.AddOrUpdate(q => q.Name, r));
            var a = "";
            roles.ForEach(q => a += q.Name + ",");
            #endregion

            #region App Roles
            var adminProfile = new Profile
            {
                Name = "Administrator",
                Notes = "Administrator Role",
                Privileges = a.Trim(','),
                Locked = true
            };
            #endregion

            #region Users
            var userManager = new UserManager<User>(new UserStore<User>(context))
            {
                UserValidator = new UserValidator<User>(UserManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                }
            };

            //Admin User
            if (UserManager.FindByNameAsync("Admin").Result == null)
            {
                var res = userManager.CreateAsync(new User
                {
                    Name = "Administrator",
                    Profile = adminProfile,
                    UserName = "Admin",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Locked = true,
                }, "admin@app");

                if (res.Result.Succeeded)
                {
                    var userId = userManager.FindByNameAsync("Admin").Result.Id;
                    roles.ForEach(q => userManager.AddToRole(userId, q.Name));
                }
            }

            #endregion

            #region Update Roles
            roles.ForEach(q => context.Roles.AddOrUpdate(q));
            #endregion

            #region AppSettings
            var appSettings = new List<AppSetting>
            {
                new AppSetting {Name = ConfigKeys.AppTitle, Value = "Seed App", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.Logo, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.ToolbarColour, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.EmailAccountName, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.EmailApiKey, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.EmailSender, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.SmsApiKey, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"},
                new AppSetting {Name = ConfigKeys.SmsSender, Value = "", Locked = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System", ModifiedAt = DateTime.UtcNow, ModifiedBy = "System"}
            };

            foreach (var setting in appSettings.Where(p => !context.AppSettings.Any(x => x.Name == p.Name)))
            {
                context.AppSettings.Add(setting);
            }
            #endregion

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
