using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface IPlanRepository
    {
        Task<IEnumerable<Plan>> GetPlansOfAccount(Guid accountId, string skill);
        Task<bool> UpdateAccountPassPlan(Guid accountId, Guid planId);
    }
}
