using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<Plan>> GetPlansOfAccount(Guid accountId, string skill);
        Task<bool> updateAccountPassPlan(Guid accountId, Guid planId);
    }
}
