using Microsoft.AspNetCore.SignalR;
using NextYTB.Interface;


namespace NextYTB.Hubs
{
    public class AccountHub : Hub
    {
        public List<ChannelConnect> _listChannelConnection = new();

        
        public async Task Connection(int channelId)
        {
            if (channelId != 0)
            {
                _listChannelConnection.Add(new ChannelConnect { ChannelId = channelId, ConnectionId = Context.ConnectionId });
            }
            await base.OnConnectedAsync();
            await Clients.Client(Context.ConnectionId).SendAsync("Connected", "Channel " + channelId + " has connected.");
        }
    }
}
