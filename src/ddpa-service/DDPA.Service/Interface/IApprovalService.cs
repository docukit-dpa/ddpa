using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;

namespace DDPA.Service
{
    public interface IApprovalService
    {
        Task<Result> ApproveDocuments(List<string> ids, string userRole, string userId);

        Task<Result> ReworkDocument(string id, string userRole, string userId, string comment);

        Task<Result> BulkDeleteDrafts(List<string> id);
    }
}