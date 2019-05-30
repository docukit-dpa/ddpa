using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using Microsoft.Extensions.Options;

namespace DDPA.Service
{
    public class SupportService : ISupportService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;
        protected readonly IValidationService _validationService;

        public SupportService(ILogger<SupportService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, IValidationService validationService) 
        {
            _logger = logger;
            _repo = repo;
            _userManager = userManager;
            _validationService = validationService;
        }

        public async Task<Result> SendEmail(SendMailDTO dto, IOptions<SMTPOptions> SMTPOptions)
        {
            Result result = new Result();
            try
            {
                var message = new MimeMessage();

                //from address
                message.From.Add(new MailboxAddress(dto.name, SMTPOptions.Value.UserName));

                //to address
                message.To.Add(new MailboxAddress("receiver", SMTPOptions.Value.UserName));

                //subject
                message.Subject = "DDPA Inquiry";

                //body
                string header = "Dear docukit,";
                string body = dto.message;
                string footer = "Regards,\n" + dto.name + ", " + dto.organization + "\n" + dto.email;

                message.Body = new TextPart("plain")
                {
                    Text = header + "\n\n" + body + "\n\n\n" + footer
                };

                //cofiguration
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", SMTPOptions.Value.Port, SMTPOptions.Value.EnableSsl);
                    
                    //aqjx... MAIL-APP password
                    await client.AuthenticateAsync(SMTPOptions.Value.UserName, SMTPOptions.Value.Password);

                    client.Send(message);

                    client.Disconnect(true);
                    
                    result.Message = "Mail has been successfully sent.";
                    result.Success = true;
                    return result;
                };
            } 
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Success = false;
                return result;
                throw;
            }
        }
    }
}