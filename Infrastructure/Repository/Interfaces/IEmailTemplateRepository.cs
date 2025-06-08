using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface IEmailTemplateRepository
    {
        public Task<(string Subject, string BodyHtml)> GetEmailTemplate(string type);
    }
}
