using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;


namespace WebApi.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
        
        private string GetIdentityName()
        {
            var identityName = (Context.User as ClaimsPrincipal).GetObjectId();
            return string.IsNullOrWhiteSpace(identityName) ? "Anonymous" : identityName;
        }

        public override async Task OnConnectedAsync()
        {
            var identityName = GetIdentityName();
            await Groups.AddToGroupAsync(Context.ConnectionId, identityName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var identityName = GetIdentityName();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, identityName);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
