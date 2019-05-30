using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;

namespace DDPA.Service
{
    public interface ISummaryService
    {
        int CalculatePercentage(int counted, int total);

        Task<DocumentCountSummaryDTO> CollectionSummary();

        Task<DocumentCountSummaryDTO> StorageSummary();

        Task<DocumentCountSummaryDTO> UsageSummary();

        Task<DocumentCountSummaryDTO> TransferSummary();

        Task<DocumentCountSummaryDTO> ArchivalSummary();

        Task<DocumentCountSummaryDTO> DisposalSummary();

        Task<int> CountTotalDocuments();
    }
}