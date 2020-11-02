using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace TcpChat
{
    public static class ChatStore
    {
        public static Dictionary<string, SocketClient> ConnectedClients { get; } = new Dictionary<string, SocketClient>();
        public static Dictionary<string, string> ChatsClientMapping { get; } = new Dictionary<string, string>();
        
        public static Dictionary<string, List<SocketClient>> Chats = new Dictionary<string, List<SocketClient>>();
        
        public static void HandleNewClient(SocketClient client, string connectToChat)
        {
            ConnectedClients.Add(client.SocketId, client);
            if (!Chats.ContainsKey(connectToChat))
                Chats.Add(connectToChat, new List<SocketClient>());
            
            Chats[connectToChat].Add(client);
            ChatsClientMapping.Add(client.SocketId, connectToChat);
            connectToChat = connectToChat.Split().First();
            client.SendPacketIntoSocket($"Now you in {connectToChat} canal" + "\r\n");
        }

        private static void SwapChat(string chatName, SocketClient client)
        {
            ChatsClientMapping.Remove(client.SocketId);
            foreach (KeyValuePair<string, List<SocketClient>> keyValue in Chats)
            {
                foreach (var user in keyValue.Value)
                {
                    if (user.SocketId == client.SocketId)
                    {
                        keyValue.Value.Remove(user);
                        break;
                    }
                }
            }
            if (!Chats.ContainsKey(chatName))
                Chats.Add(chatName, new List<SocketClient>());
            Chats[chatName].Add(client);
            ChatsClientMapping.Add(client.SocketId, chatName);
            client.SendPacketIntoSocket($"Now you in {chatName.Split().First()} canal" + "\r\n");
            foreach (var clientInChat in Chats[chatName])
            {
                if (client.SocketId == clientInChat.SocketId) 
                {
                    foreach (KeyValuePair<string, List<SocketClient>> keyValuePair in Chats)
                    {
                        foreach (var user in keyValuePair.Value)
                        {
                            if (chatName == keyValuePair.Key)
                            {
                                if (client.SocketId != user.SocketId)
                                {
                                    client.SendPacketIntoSocket($"{user.ClientName} in this canal" + "\r\n");
                                }
                            }   
                        }
                    } 
                    continue;
                }
                clientInChat.SendPacketIntoSocket($"{client.ClientName}: enter to this canal" + "\r\n");
            }   
        }
        
        public static void HandleName(string clientName, SocketClient client)
        {
            client.ClientName = clientName;
            client.SendPacketIntoSocket("Enter canal: ");
        }
        
        public static void HandleNewMessage(byte[] message, SocketClient client, int readBytes)
        {
            var messageToText = Encoding.ASCII.GetString(message, 0, readBytes);
            if (client.ClientName == "")
            {
                ChatStore.HandleName(messageToText, client);
                client.ClientName = client.ClientName.Split().First();
            }
            else
            {
                if (!ConnectedClients.ContainsKey(client.SocketId))
                {
                    ChatStore.HandleNewClient(client, messageToText);    
                }
                else
                {
                    var targetChat = ChatsClientMapping[client.SocketId];

                    if (messageToText.StartsWith("/"))
                        Commands(messageToText, client, targetChat);
                    else
                    {
                        foreach (var clientInChat in Chats[targetChat])
                        {
                            if (client.SocketId == clientInChat.SocketId) 
                            {
                                clientInChat.SendPacketIntoSocket($"You: {messageToText}" + "\r\n"); 
                                continue;
                            }
                            clientInChat.SendPacketIntoSocket($"{client.ClientName}: {messageToText}" + "\r\n");
                        }   
                    }
                }
            }
        }

        private static void Commands(string messageToText, SocketClient client, string targetChat)
        {
            var chatName = messageToText.Split(' ').Last();
            messageToText = messageToText.Split().First();
            switch (messageToText)
            {
                case "/swap":
                    SwapChat(chatName, client);
                    foreach (var clientInChat in Chats[targetChat])
                    {
                        if (client.SocketId != clientInChat.SocketId)
                            clientInChat.SendPacketIntoSocket($"{client.ClientName}: leave this canal" + "\r\n");
                    }   
                    break;
                case "/disconnect":
                    Disconnect(client);
                    break;
                default:
                    client.SendPacketIntoSocket("Command not found..." + "\r\n");
                    break;
            }
        }

        private static void Disconnect(SocketClient client)
        {
            ConnectedClients.Remove(client.SocketId);
            ChatsClientMapping.Remove(client.SocketId);
            foreach (var chat in Chats)
            {
                chat.Value.Remove(client);
            }
            
        }
    }
}