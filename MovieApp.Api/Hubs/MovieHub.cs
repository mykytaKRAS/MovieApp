using Microsoft.AspNetCore.SignalR;

namespace MovieApp.Api.Hubs
{
    public class MovieHub : Hub
    {
        // Called when a client connects
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Method to broadcast movie activity
        public async Task SendMovieActivity(string message)
        {
            await Clients.All.SendAsync("ReceiveMovieActivity", message);
        }
    }
}