using Microsoft.AspNetCore.Authorization;

namespace ComtradeAPI.Services
{
    public class AgentOwnerRequirement : IAuthorizationRequirement
    {
        public AgentOwnerRequirement(int agentId)
        {
            AgentId = agentId;
        }

        public int AgentId { get; set; }   

        
    }
}
