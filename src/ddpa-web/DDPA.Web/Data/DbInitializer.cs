using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using DDPA.DTO;
using DDPA.Service;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Data
{
    public static class DbInitializer
    {
        public static async Task Seed(ApplicationDbContext context, RoleManager<IdentityRole<int>> roleManager,
            UserManager<ExtendedIdentityUser> userManager, IHostingEnvironment env, IAdminService adminService, IMaintenanceService maintenanceService)
        {
            context.Database.EnsureCreated();

            string adminRole = nameof(Role.ADMINISTRATOR);
            string dpoRole = nameof(Role.DPO);
            string userRole = nameof(Role.USER);
            string deptHeadRole = nameof(Role.DEPTHEAD);

            if (await roleManager.FindByNameAsync(adminRole) == null)
            {
                var ir = await roleManager.CreateAsync(new IdentityRole<int>(adminRole));
            }

            if (await roleManager.FindByNameAsync(dpoRole) == null)
            {
                var ir = await roleManager.CreateAsync(new IdentityRole<int>(dpoRole));
            }

            if (await roleManager.FindByNameAsync(userRole) == null)
            {
                var ir = await roleManager.CreateAsync(new IdentityRole<int>(userRole));
            }

            if (await roleManager.FindByNameAsync(deptHeadRole) == null)
            {
                var ir = await roleManager.CreateAsync(new IdentityRole<int>(deptHeadRole));
            }

            var admin = await userManager.GetUsersInRoleAsync(adminRole);
            var adminUser = string.Empty;

            var x = (context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();

            //validation if database was already seeded
            if (!context.Users.Any())
            {
                var dataPath = Path.Combine(env.ContentRootPath, "Data", "seed");
                if (Directory.Exists(dataPath))
                {
                    var adminSettings = Path.Combine(dataPath, "adminSettings.json");
                    if (File.Exists(adminSettings))
                    {
                        UserDTO sa = JsonConvert.DeserializeObject<UserDTO>(System.IO.File.ReadAllText(adminSettings));

                        if (admin.Count() == 0)
                        {
                            var addAdmin = new ExtendedIdentityUser { UserName = sa.UserName, Email = sa.Email, FirstName = sa.FirstName, LastName = sa.LastName, EmailConfirmed = sa.EmailConfirmed };
                            var result = await userManager.CreateAsync(addAdmin, sa.Password);
                            if (result.Succeeded)
                            {
                                var addrole = await userManager.AddToRoleAsync(addAdmin, adminRole);
                                //File.Delete(adminSettings);
                            }
                            adminUser = addAdmin.Id.ToString();
                        }
                        else
                        {
                            adminUser = admin.FirstOrDefault().Id.ToString();
                        }
                    }

                    var dpoSettings = Path.Combine(dataPath, "dposettings.json");
                    if (File.Exists(dpoSettings))
                    {
                        UserDTO sa = JsonConvert.DeserializeObject<UserDTO>(System.IO.File.ReadAllText(dpoSettings));

                        if (admin.Count() == 0)
                        {
                            var addDpo = new ExtendedIdentityUser { UserName = sa.UserName, Email = sa.Email, FirstName = sa.FirstName, LastName = sa.LastName, EmailConfirmed = sa.EmailConfirmed };
                            var result = await userManager.CreateAsync(addDpo, sa.Password);
                            if (result.Succeeded)
                            {
                                var addrole = await userManager.AddToRoleAsync(addDpo, dpoRole);
                                //File.Delete(adminSettings);
                            }
                        }
                    }

                    var fields = Path.Combine(dataPath, "default-fields.json");
                    if (File.Exists(fields))
                    {
                        List<AddFieldDTO> dto = JsonConvert.DeserializeObject<List<AddFieldDTO>>(System.IO.File.ReadAllText(fields));
                        var addresult = await adminService.AddFields(dto, adminUser);
                        if (addresult.Success)
                        {
                            //File.Delete(fields);
                        }
                    }

                    var departments = Path.Combine(dataPath, "default-departments.json");
                    if (File.Exists(departments))
                    {
                        List<AddDepartmentDTO> dto = JsonConvert.DeserializeObject<List<AddDepartmentDTO>>(System.IO.File.ReadAllText(departments));

                        foreach(AddDepartmentDTO item in dto)
                        {
                           await maintenanceService.CreateDepartment(item, adminUser);
                        }
                    }

                    var modules = Path.Combine(dataPath, "modules.json");
                    if (File.Exists(modules))
                    {
                        List<AddModuleDTO> dto = JsonConvert.DeserializeObject<List<AddModuleDTO>>(System.IO.File.ReadAllText(modules));
                        var addresult = await adminService.AddModules(dto, adminUser);
                        if (addresult.Success)
                        {
                            //File.Delete(modules);
                        }
                    }

                    var datasets = Path.Combine(dataPath, "dataset.json");
                    if (File.Exists(datasets))
                    {
                        List<AddDatasetDTO> dto = JsonConvert.DeserializeObject<List<AddDatasetDTO>>(System.IO.File.ReadAllText(datasets));
                        var addresult = await adminService.AddDatasets(dto, adminUser);
                        if (addresult.Success)
                        {
                            //File.Delete(modules);
                        }
                    }

                    var userGuide = Path.Combine(dataPath, "user-guide.json");
                    if (File.Exists(datasets))
                    {
                        AddResourceDTO dto = JsonConvert.DeserializeObject<AddResourceDTO>(System.IO.File.ReadAllText(userGuide));
                        var addresult = await adminService.CreateUserGuideResource(dto, adminUser);
                        if (addresult.Success)
                        {
                            //File.Delete(modules);
                        }
                    }
                }
            }
        }
    }
}
