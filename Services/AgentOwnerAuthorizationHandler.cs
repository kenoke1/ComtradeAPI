using Microsoft.AspNetCore.Authorization;

namespace ComtradeAPI.Services
{
    public class AgentOwnerAuthorizationHandler : AuthorizationHandler<AgentOwnerRequirement>
    {
        protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AgentOwnerRequirement requirement)
        {
            var userRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Admins can access everything
            if (userRole == "Admin")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if user is the agent owner
            var agentIdClaim = context.User.FindFirst("AgentId")?.Value;
            if (int.TryParse(agentIdClaim, out var userAgentId) && userAgentId == requirement.AgentId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
