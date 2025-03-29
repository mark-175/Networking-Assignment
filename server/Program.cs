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

    // TODO: [Read the JSON file and return the list of DNSRecords]




    public static void start()
    {


        // TODO: [Create a socket and endpoints and bind it to the server IP address and port number]
        byte[] buffer = new byte[1000];
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress IpAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint LocalEndpoint = new IPEndPoint(IpAddress, 32000);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteEp = sender;
        socket.Bind(LocalEndpoint);
        // TODO:[Receive and print a received Message from the client]

        // TODO:[Receive and print Hello]
        Message data = null;
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for messages...");
                int b = socket.ReceiveFrom(buffer, ref remoteEp);

                string json = Encoding.ASCII.GetString(buffer, 0, b);
                data = JsonSerializer.Deserialize<Message>(json)!;

                Message message = new Message { MsgId = 1, MsgType = MessageType.Hello, Content = "Hello" };
                Console.WriteLine(message);
                Console.WriteLine("Hello");
                string ToSend = JsonSerializer.Serialize(message);
                byte[] msg = Encoding.ASCII.GetBytes(json);
                socket.SendTo(msg, msg.Length, SocketFlags.None, remoteEp);

            }

        }
        catch (ArgumentNullException Ex)
        {
            Message message = new Message { MsgId = 1, MsgType = MessageType.Error, Content = $"Error while Deserializing the message : {Ex.Message}" };
            string json = JsonSerializer.Serialize(message);
            byte[] msg = Encoding.ASCII.GetBytes(json);
            socket.SendTo(msg, msg.Length, SocketFlags.None, remoteEp);
        }





        // TODO:[Send Welcome to the client]


        // TODO:[Receive and print DNSLookup]


        // TODO:[Query the DNSRecord in Json file]

        // TODO:[If found Send DNSLookupReply containing the DNSRecord]

        // TODO:[If not found Send Error]

        // TODO:[Receive Ack about correct DNSLookupReply from the client]

        // TODO:[If no further requests receieved send End to the client]

    }


}