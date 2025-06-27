using backend_english.Request;
using backend_english.Response;
using Core.Models;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService planService;
        public PlanController(IPlanService lanService)
        {
            planService = lanService;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans(Guid accountId, string? skill = "speaking")
        {
            var plans = await planService.GetPlansOfAccount(accountId, skill);
            return Ok(new ApiResponse<IEnumerable<Plan>>(200, null, plans));
        }

        [HttpPost("complete")]
        public async Task<IActionResult> MarkPlanAsCompleted([FromBody] CompletePlanRequest request)
        {
            if (request == null || request.accountId == Guid.Empty || request.planId == Guid.Empty)
            {
                return BadRequest("Invalid data.");
            }

            var result = await planService.updateAccountPassPlan(request.accountId, request.planId);

            if (result)
                return Ok(new { message = "Plan marked as completed." });
            else
                return StatusCode(500, "Failed to update progress.");
        }
    }
}
