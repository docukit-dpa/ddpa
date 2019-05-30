using DDPA.Commons.Enums;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPA.Service
{
    public class BaseService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;

        protected int _currentCompanyId;

        public BaseService(ILogger logger, IRepository repo)
        {
            _logger = logger;
            _repo = repo;

        }

        public async Task<Dictionary<int, ExtendedIdentityUser>> GetUsersById(List<int> ids)
        {
            var response = new Dictionary<int, ExtendedIdentityUser>();

            if (ids != null && ids.Count() > 0)
            {
                var users = await _repo.GetAsync<ExtendedIdentityUser>(
                      filter: c => ids.Contains(c.Id)
                );

                response = users.ToDictionary(i => i.Id, v => v); ;
            }

            return response;

        }

        public async Task<Dictionary<string, ExtendedIdentityUser>> GeUsersByUserName(List<string> usernames)
        {
            var response = new Dictionary<string, ExtendedIdentityUser>();

            if (usernames != null && usernames.Count() > 0)
            {
                var users = await _repo.GetAsync<ExtendedIdentityUser>(
                      filter: c => usernames.Contains(c.UserName)
                );

                response = users.ToDictionary(i => i.UserName, v => v); ;
            }

            return response;

        }
    }
}
