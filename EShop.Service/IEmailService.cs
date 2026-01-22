using EShop.Domain.DomainModels;

namespace EShop.Service
{
    public interface IEmailService
    {
        bool SendEmailAsync(EmailMessage message);
    }
}