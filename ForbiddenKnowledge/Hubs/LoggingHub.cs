using Microsoft.AspNetCore.SignalR;
using Audit.Core;

namespace ForbiddenKnowledge.Hubs
{
	//A "Hub" is a class used to manage real-time communication between the server and clients using SignalR (websockets).
	//Activity that happens over SignalR/Websockets won't be recorded by the HTTP logging middleware.
	//In order to ensure comprehensive logging of user actions, this SignalR logging hub uses Audit.NET
	//to log SignalR activity.
    public class LoggingHub : Hub
    {
		public override async Task OnConnectedAsync()
		{
			using (var auditScope = AuditScope.Create("SignalR Connection", () => new
			{
				ConnectionId = Context.ConnectionId,
				Event = "Connected",
				Timestamp = DateTime.UtcNow
			}))
			{
				await base.OnConnectedAsync();
			}
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			using (var auditScope = AuditScope.Create("SignalR Disconnection", () => new
			{
				ConnectionId = Context.ConnectionId,
				Event = "Disconnected",
				Timestamp = DateTime.UtcNow
			}))
			{
				await base.OnDisconnectedAsync(exception);
			}
		}

		public async Task SendMessage(string message)
		{
			using (var auditScope = AuditScope.Create("SignalR Message", () => new
			{
				ConnectionId = Context.ConnectionId,
				Message = message,
				Timestamp = DateTime.UtcNow
			}))
			{
				await Clients.All.SendAsync("ReceiveMessage", message);
			}
		}
	}
}
