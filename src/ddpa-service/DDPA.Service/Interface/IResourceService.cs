using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;
using Microsoft.AspNetCore.Http;

namespace DDPA.Service
{
    public interface IResourceService
    {
        Task<Result> CreateResource(AddResourceDTO dto, string userId, IFormFile file);
    }
}