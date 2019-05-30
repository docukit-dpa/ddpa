using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using AutoMapper;
using DDPA.Service.Extensions;

namespace DDPA.Service
{
    public class SummaryService : ISummaryService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;
        protected readonly IValidationService _validationService;
        protected readonly IMapper _mapper;

        public SummaryService(ILogger<AccountService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, IValidationService validationService) 
        {
            _logger = logger;
            _repo = repo;
            _userManager = userManager;
            _validationService = validationService;
            _mapper = this.GetMapper();
        }

        public int CalculatePercentage(int counted, int total)
        {
            double count = Convert.ToDouble(counted);
            double tot = Convert.ToDouble(total);
            double percentage = (count / tot) * 100;
            if(percentage > 0)
            {
                return (int)Math.Round(percentage, 0, MidpointRounding.AwayFromZero);
            }

            else
            {
                return 0;
            }
        }

        public async Task<DocumentCountSummaryDTO> CollectionSummary()
        {
            DocumentCountSummaryDTO summary = new DocumentCountSummaryDTO();
            int percentage = 0;
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            var disposal = await _repo.GetAsync<Document>(filter: f => f.Status == Status.Collection);
            int documentCount = _mapper.Map<List<DocumentDTO>>(disposal).Count;

            percentage = CalculatePercentage(documentCount, documentTotal);

            summary.Count = documentCount;
            summary.Percentage = percentage;

            return summary;
        }

        public async Task<DocumentCountSummaryDTO> StorageSummary()
        {
            DocumentCountSummaryDTO summary = new DocumentCountSummaryDTO();
            int percentage = 0;
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            var disposal = await _repo.GetAsync<Document>(filter: f => f.Status == Status.Storage);
            int documentCount = _mapper.Map<List<DocumentDTO>>(disposal).Count;

            percentage = CalculatePercentage(documentCount, documentTotal);

            summary.Count = documentCount;
            summary.Percentage = percentage;

            return summary;
        }

        public async Task<DocumentCountSummaryDTO> UsageSummary()
        {
            DocumentCountSummaryDTO summary = new DocumentCountSummaryDTO();
            int percentage = 0;
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            var disposal = await _repo.GetAsync<Document>(filter: f => f.Status == Status.Usage);
            int documentCount = _mapper.Map<List<DocumentDTO>>(disposal).Count;

            percentage = CalculatePercentage(documentCount, documentTotal);

            summary.Count = documentCount;
            summary.Percentage = percentage;

            return summary;
        }

        public async Task<DocumentCountSummaryDTO> TransferSummary()
        {
            DocumentCountSummaryDTO summary = new DocumentCountSummaryDTO();
            int percentage = 0;
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            var disposal = await _repo.GetAsync<Document>(filter: f => f.Status == Status.Transfer);
            int documentCount = _mapper.Map<List<DocumentDTO>>(disposal).Count;

            percentage = CalculatePercentage(documentCount, documentTotal);

            summary.Count = documentCount;
            summary.Percentage = percentage;

            return summary;
        }

        public async Task<DocumentCountSummaryDTO> ArchivalSummary()
        {
            DocumentCountSummaryDTO summary = new DocumentCountSummaryDTO();
            int percentage = 0;
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            var disposal = await _repo.GetAsync<Document>(filter: f => f.Status == Status.Archival);
            int documentCount = _mapper.Map<List<DocumentDTO>>(disposal).Count;

            percentage = CalculatePercentage(documentCount, documentTotal);

            summary.Count = documentCount;
            summary.Percentage = percentage;

            return summary;
        }

        public async Task<DocumentCountSummaryDTO> DisposalSummary()
        {
            DocumentCountSummaryDTO summary = new DocumentCountSummaryDTO();
            int percentage = 0;
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            var disposal = await _repo.GetAsync<Document>(filter: f => f.Status == Status.Disposal);
            int documentCount = _mapper.Map<List<DocumentDTO>>(disposal).Count;

            percentage = CalculatePercentage(documentCount, documentTotal);

            summary.Count = documentCount;
            summary.Percentage = percentage;

            return summary;
        }

        public async Task<int> CountTotalDocuments()
        {
            var document = await _repo.GetAllAsync<Document>();
            int documentTotal = _mapper.Map<List<DocumentDTO>>(document).Count;

            return documentTotal;
        }
    }
}