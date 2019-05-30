using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;
using Microsoft.Extensions.Options;

namespace DDPA.Service
{
    public interface ISupportService
    {
        Task<Result> SendEmail(SendMailDTO dto, IOptions<SMTPOptions> SMTPOptions);
    }
}