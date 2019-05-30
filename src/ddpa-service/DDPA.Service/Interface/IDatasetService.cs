using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;

namespace DDPA.Service
{
    public interface IDatasetService
    {
        Task<Result> AddDataset(AddDatasetDTO dto, string userId);

        Task<Result> UpdateDataset(UpdateDatasetDTO dto);

        Result DeleteDataset(string id);

        Task<Result> AddFieldToDataset(string datasetId, string fieldId);

        Result DeleteDatasetField(string id);

        Task<Result> AddIssue(IssueDTO dto, string userId, string userDept, string userRole);

        Task<Result> EditIssue(IssueDTO dto, string userId, string userDept);


    }
}