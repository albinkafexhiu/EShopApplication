using System;

namespace EShop.Domain.DomainModels
{
    public class EmailMessage : BaseEntity
    {
        public string? MailTo { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }

        // true = successfully sent, false = failed / pending
        public bool Status { get; set; }
    }
}