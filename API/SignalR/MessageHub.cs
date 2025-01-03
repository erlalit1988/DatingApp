﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, 
        IMapper mapper, IHubContext<PresenceHub> presenceHub): Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];

            if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new HubException("Cannot join group");
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            var message = await messageRepository.GetMessageThread(Context.User.GetUserName(), otherUser!);

            await Clients.Caller.SendAsync("ReceiveMessageThread", message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User?.GetUserName() ?? throw new Exception("Could not get user"); 

            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot message yourself");

            var sender = await userRepository.GetUserByNameAsync(username);
            var recipient = await userRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);
            if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
                throw new HubException("Cannot send message at this time");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await messageRepository.GetMessageGroup(groupName);

            if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionForUser(recipient.UserName);
                if (connections != null && connections?.Count != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageRecived",
                        new
                        {
                            username = sender.UserName,
                            knownAs = sender.KnownAs
                        });
                }
            }

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

       

        private async Task<Group> AddToGroup(string groupName)
        {
            var username = Context.User?.GetUserName() ?? throw new Exception("Cannot ge username");
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };

            if(group == null)
            {
                group = new Group { Name = groupName };
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            if(await messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);

            var connection = group?.Connections.FirstOrDefault(x =>x.ConnectionId == Context.ConnectionId);
            //await messageRepository.GetConnection(Context.ConnectionId);

            if(connection !=null && group !=null)
            {
                messageRepository.RemoveConnection(connection);
               if(await messageRepository.SaveAllAsync()) return group;
            }

            throw new HubException("Failed to remove from group");
        }
        private string GetGroupName(string caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}