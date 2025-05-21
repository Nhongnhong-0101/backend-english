using backend_english.Response.Pronounciation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Infrastructure.Services.Interfaces
{
    public interface IValueService
    {
        public Task<AssessmentResponse?> sendToAzure(string sententce, IFormFile recored);
        public AssessmentResponse FakeResponse();
    }
}
