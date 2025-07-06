using Core.Models;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository planRepository;

        public PlanService(IPlanRepository planRepository)
        {
            this.planRepository = planRepository;
        }

        public async Task<Plan> getCurrentPlanOfAccount(Guid accountId, string skill)
        {
            try
            {

                var plans = await planRepository.GetPlansOfAccount(accountId, skill);
                var firstNotDonePlan = plans.FirstOrDefault(p => !p.isDone);

                if (firstNotDonePlan != null)
                {
                    return firstNotDonePlan;
                }
                else
                {
                    return plans.Last();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task<IEnumerable<Plan>> GetPlansOfAccount(Guid accountId, string skill)
        {
            try
            {
                return planRepository.GetPlansOfAccount(accountId, skill);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task<bool> updateAccountPassPlan(Guid accountId, Guid planId)
        {
            try
            {
                return planRepository.UpdateAccountPassPlan(accountId, planId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
