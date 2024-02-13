using Microsoft.AspNetCore.SignalR;

namespace RestApi.Hubs
{
    public class ClientHub : Hub<IClientHub>
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).ReceiveNotification(
                $"Client {Context.User?.Identity?.Name} connected.");

            await Groups.AddToGroupAsync(Context.ConnectionId, "mnist");

            await base.OnConnectedAsync();
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task ExitGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
