using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;

namespace DDPA.Service
{
    public interface IAccountService
    {
        Task<Result> CreateUser(AddUserDTO dto);
    }
}