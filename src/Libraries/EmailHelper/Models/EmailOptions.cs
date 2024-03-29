using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailHelper.Models
{
    public class EmailOptions
    {
        public string? Environment { get; set; }
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }
        public string? MailFrom { get; set; }
        public string? SenderName { get; set; }
    }
}