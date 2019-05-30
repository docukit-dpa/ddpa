using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using DDPA.Validation;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace DDPA.Service
{
    public class ResourceService : IResourceService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly IValidationService _validationService;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IQueryService _queryService;

        public ResourceService(ILogger<AccountService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, 
            IValidationService validationService, IHostingEnvironment hostingEnvironment, IQueryService queryService) 
        {
            _logger = logger;
            _repo = repo;
            _userManager = userManager;
            _validationService = validationService;
            _hostingEnvironment = hostingEnvironment;
            _queryService = queryService;
        }

        public async Task<Result> CreateResource(AddResourceDTO dto, string userId, IFormFile file)
        {
            var result = new Result();
            try
            {
                string rootFolder = _hostingEnvironment.WebRootPath;
                //todo: create validation

                /*----------------ADD FILE TO SERVER-----------------*/
                //Getting FileName
                var fileName = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                //Assigning Unique Filename (Guid)
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                var ResourceFolder = "Resource";

                //Getting file Extension
                var FileExtension = Path.GetExtension(fileName);

                var newFileName = fileName;

                // Combines two strings into a path.
                if (!Directory.Exists(Path.Combine(_hostingEnvironment.WebRootPath, ResourceFolder)))
                {
                    Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, ResourceFolder));
                }

                fileName = Path.Combine(_hostingEnvironment.WebRootPath, ResourceFolder) + $@"\{newFileName}";
                bool doFileExist = await _queryService.DoFileExist(ResourceFolder, fileName);

                //Getting file Extension
                FileExtension = Path.GetExtension(fileName);

                // if you want to store path of folder in database
                var PathDB = ResourceFolder + "/" + newFileName;

                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                /*----------------ADD FILE TO SERVER-----------------*/

                var resource = new Resource
                {
                    NameOfDocument = dto.NameOfDocument,
                    TypeOfDocument = dto.TypeOfDocument,
                    FilePath = PathDB,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                _repo.Create(resource);
                await _repo.SaveAsync();

                result.Id = resource.Id.ToString();
                result.Message = "Resource has been successfully Added.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding resource";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateResource: {0}", e.Message);
            }

            return result;
        }
    }
}