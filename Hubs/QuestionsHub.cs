using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

// this is websocket hub
namespace netcore_api.Hubs
{
    public class QuestionsHub : Hub
    {

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Message", "Successfully connected to awesome websocket party");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {           
            await Clients.Caller.SendAsync("Message", "Disconnected from awesome websocket party");
            await base.OnDisconnectedAsync(exception);
        }
        
        public async Task SubscribeQuestion(int questionId)
        {
            // TODO: - add the client to a group of clients of intersted in getting the updates
            // TODO: - send a message to the client to indicate that the subscription has been accepted.
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Questions-{questionId}");
            await Clients.Caller.SendAsync("Message", $"Subscribed successfully to Questions-{questionId}");
        }

        public async Task UnsubscribeQuestion(int questionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Questions-{questionId}");
            await Clients.Caller.SendAsync("Message", $"Unsubscribed from group Questions-{questionId} succesfully");
        }
    }

}
