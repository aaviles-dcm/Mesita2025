using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Hubs
{
    public class TicketHub : Hub
    {
        // We can add methods here if clients need to send messages to the server,
        // but for now we only need the server to notify clients.
        
        public async Task SendTicketUpdate(int ticketId)
        {
            await Clients.All.SendAsync("TicketUpdated", ticketId);
        }
    }
}
