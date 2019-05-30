using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;

namespace DDPA.Service
{
    public interface IMaintenanceService
    {
        Task<Result> CreateUser(AddUserDTO dto);

        Task<Result> UpdateUser(UpdateUserDTO dto);

        Result DeleteUser(string id);

        Task<Result> ChangePasswordUser(ChangePasswordUserDTO dto);

        Task<Result> CreateField(AddFieldDTO dto, string userId);

        Task<Result> UpdateField(UpdateFieldDTO dto);

        Result DeleteField(string id);

        Task<Result> CreateFieldItem(AddFieldItemDTO dto);

        Task<Result> UpdateFieldItem(UpdateFieldItemDTO dto);

        Result DeleteFieldItem(string id);

        Task<Result> AddSubModuleField(AddSubModuleFieldDTO dto);

        Result BulkDeleteField(List<string> id);

        Result BulkDeleteUser(List<string> id);

        Task<Result> CreateLifeCycleField(AddFieldDTO dto, string userId);

        Task<Result> UpdateSubModuleField(UpdateFieldDTO dto);

        Task<Result> UpdateDepartment(UpdateDepartmentDTO dto);

        Task<Result> CreateDepartment(AddDepartmentDTO dto, string userId);

        Result DeleteDepartment(string id);

        Result BulkDeleteDepartment(List<string> id);

        Task<Result> AddDataset(AddDatasetDTO dto, string userId);

        Task<Result> UpdateDataset(UpdateDatasetDTO dto);

        Result DeleteDataset(string id);

        Task<Result> AddFieldToDataset(string datasetId, string fieldId);

        Result DeleteDatasetField(string id);

        Result BulkDeleteDataset(List<string> id);

        Task<Result> UpdateLifeCycleField(UpdateFieldDTO dto);

        Task<Result> ChangeCompanyName(UpdateCompanyInfoDTO dto, string userId);

        Task<Result> SetUp(string userId);

    }
}