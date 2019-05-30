using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.DTO
{
    public class SMTPOptions
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string FromName { get; set; }

        public string FromAddress { get; set; }

        public string DeliveryMethod { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public bool EnableSsl { get; set; }
    }
}
