using System;
using System.Data;
using System.Data.SqlTypes;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using LibData;

// ReceiveFrom();
class Program
{
    static void Main(string[] args)
    {
        ServerUDP.start();
    }
}

public class Setting
{
    public int ServerPortNumber { get; set; }
    public string? ServerIPAddress { get; set; }
    public int ClientPortNumber { get; set; }
    public string? ClientIPAddress { get; set; }
}


class ServerUDP
{
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);
    static byte[] buffer = new byte[1000];

    // TODO: [Read the JSON file and return the list of DNSRecords]




    public static void start()
    {


        // TODO: [Create a socket and endpoints and bind it to the server IP address and port number]
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress IpAddress = IPAddress.Parse(setting.ServerIPAddress);
        IPEndPoint LocalEndpoint = new IPEndPoint(IpAddress, setting.ServerPortNumber); //setting.ServerPortNumber

        IPEndPoint sender = new IPEndPoint(IPAddress.Parse(setting.ClientIPAddress), setting.ClientPortNumber); //IPAddress.Parse(setting.ClientIPAddress), setting.ClientPortNumber
        EndPoint remoteEp = sender;
        socket.Bind(LocalEndpoint);
        Message? ClientRequest = null;
        Message? ServerResponse = null;
        bool HelloRecieved = false;
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for messages...\n");

                // TODO:[Receive and print a received Message from the client]
                ClientRequest = RecieveMessage(socket, ref remoteEp);

                if (ClientRequest.MsgType != MessageType.Hello && !HelloRecieved)
                {
                    Console.WriteLine("\n========= Send Error to client =========\n");
                    ServerResponse = new Message { MsgId = ClientRequest.MsgId, MsgType = MessageType.Error, Content = "Cannot send any messages before the handshake" };
                    SendMessage(socket, ServerResponse, remoteEp);
                    continue;
                }

                switch (ClientRequest.MsgType)
                {
                    case MessageType.Hello:
                        // TODO:[Receive and print Hello] Done above

                        // TODO:[Send Welcome to the client]
                        HelloRecieved = true;
                        ServerResponse = new Message { MsgId = ClientRequest.MsgId, MsgType = MessageType.Welcome, Content = "Welcome" };
                        SendMessage(socket, ServerResponse, remoteEp);
                        break;
                    case MessageType.DNSLookup:

                        // TODO:[Receive and print DNSLookup]

                        ServerResponse = DNSLookUp("DNSrecords.json", ClientRequest);


                        SendMessage(socket, ServerResponse, remoteEp);

                        break;
                    case MessageType.Ack:
                        ServerResponse = new Message { MsgId = ClientRequest.MsgId, MsgType = MessageType.End, Content = "End message" };
                        SendMessage(socket, ServerResponse, remoteEp);
                        break;
                    default:
                        break;
                }




            }

        }
        catch (ArgumentNullException Ex)
        {
            Message message = new Message { MsgId = ClientRequest.MsgId, MsgType = MessageType.Error, Content = $"Error while Deserializing the message : {Ex.Message}" };
            SendMessage(socket, message, remoteEp);
        }
        catch (Exception Ex)
        {
            Console.WriteLine("Error: " + Ex.ToString());
        }









        // TODO:[Query the DNSRecord in Json file]

        // TODO:[If found Send DNSLookupReply containing the DNSRecord]

        // TODO:[If not found Send Error]

        // TODO:[Receive Ack about correct DNSLookupReply from the client]

        // TODO:[If no further requests receieved send End to the client]

    }
    private static void SendMessage(Socket socket, Message MessageObj, EndPoint RemoteEp)
    {
        string json = JsonSerializer.Serialize(MessageObj);
        byte[] msg = Encoding.ASCII.GetBytes(json);
        socket.SendTo(msg, msg.Length, SocketFlags.None, RemoteEp);

        Console.WriteLine($"\n============ {MessageObj.MsgType} Message Sent ============\n");
        PrintSentMessage(MessageObj);
    }


    private static Message? RecieveMessage(Socket socket, ref EndPoint RemoteEp)
    {
        int b = socket.ReceiveFrom(buffer, ref RemoteEp);
        // TODO:[Receive and print a received Message from the client]
        string json = Encoding.ASCII.GetString(buffer, 0, b);
        Message Request = JsonSerializer.Deserialize<Message>(json)!;
        Console.WriteLine("\n========= Message recieved =========\n");
        PrintRecievedMessage(Request);

        return Request;
    }

    private static void PrintRecievedMessage(Message message)
    {
        if (message == null)
        {
            Console.WriteLine("No Message Recieved");
            return;
        }

        Console.WriteLine($"Recieved a message of type {message.MsgType} | Id : {message.MsgId}");


        if (message.Content is JsonElement jsonElement)
        {
            try
            {
                var record = JsonSerializer.Deserialize<DNSRecord>(jsonElement.GetRawText());
                if (record is not null)
                {
                    Console.WriteLine("Content:");
                    Console.WriteLine($"Type: {record.Type} | Name: {record.Name} | Value: {record.Value ?? "No Value provided"} | TTL: {(record.TTL.HasValue ? record.TTL : "No value provided")} | Priority: {(record.Priority.HasValue ? record.Priority.Value.ToString() : "No value provided")}");
                    return;
                }
            }
            catch (JsonException e)
            {
                if (message.MsgType == MessageType.DNSLookup || message.MsgType == MessageType.DNSLookupReply)
                {
                    Console.WriteLine("An unexpected error has occured: " + e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error has occured: " + e.Message);

            }
            Console.WriteLine($"Content: {message.Content}");
        }
    }
    private static void PrintSentMessage(Message message)
    {
        Console.WriteLine($"Sending a message of type {message.MsgType} | Id : {message.MsgId}");

        try
        {
            if (message.Content!.GetType() == typeof(DNSRecord))
            {
                var record = (DNSRecord)message.Content!;
                Console.WriteLine("Content:");
                Console.WriteLine($"Type: {record.Type} | Name: {record.Name} | Value: {record.Value ?? "No Value provided"} | TTL: {(record.TTL.HasValue ? record.TTL : "No value provided")} | Priority: {(record.Priority.HasValue ? record.Priority.Value.ToString() : "No value provided")}");
                return;

            }

        }
        catch (Exception e)
        {

            Console.WriteLine("An unexpected error has occured: " + e.Message);
        }

        Console.WriteLine($"Content: {message.Content}");
    }


    private static Message DNSLookUp(string path, Message LookUpRequest)
    {
        var RequestedDNS = JsonSerializer.Deserialize<DNSRecord>(LookUpRequest.Content.ToString());
        if (RequestedDNS.Type == null || RequestedDNS.Name == null)
        {
            return new Message { MsgId = LookUpRequest.MsgId, MsgType = MessageType.Error, Content = "A DNS record must contain a name and a type" };
        }
        try
        {


            string FileContent = File.ReadAllText(path);
            DNSRecord[]? DNSRecords = JsonSerializer.Deserialize<DNSRecord[]>(FileContent);

            List<DNSRecord> MatchingDNSRecords = DNSRecords.Where(r => r.Type == RequestedDNS.Type && r.Name == RequestedDNS.Name).ToList();
            if (MatchingDNSRecords.Count == 0)
            {
                return new Message { MsgId = LookUpRequest.MsgId, MsgType = MessageType.Error, Content = "The DNS record you were looking for couldn't be found" };

            }
            return new Message { MsgId = LookUpRequest.MsgId, MsgType = MessageType.DNSLookupReply, Content = MatchingDNSRecords[0] };

        }
        catch (Exception)
        {
            return new Message { MsgId = LookUpRequest.MsgId, MsgType = MessageType.Error, Content = "An error occured while looking for the DNS record" };
        }

    }

}
