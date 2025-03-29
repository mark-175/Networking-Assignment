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
    static string configFile = @"../../../../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);

    // TODO: [Read the JSON file and return the list of DNSRecords]

    static string DNSRecords = File.ReadAllText(@"../../../DNSrecords.json");

    static DNSRecord[] dNSRecords = JsonSerializer.Deserialize<DNSRecord[]>(DNSRecords);




    public static void start()
    {


        // TODO: [Create a socket and endpoints and bind it to the server IP address and port number]
        byte[] buffer = new byte[1000];
        byte[] msg = new byte[1000];
        string data = null;
        string json = null;
        Socket sock;
        IPAddress ipAddress = IPAddress.Parse(setting.ServerIPAddress);
        IPEndPoint localEndpoint = new IPEndPoint(ipAddress, 32000);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteEP = (EndPoint)sender;


        // TODO:[Receive and print a received Message from the client]
        try
        {
            sock = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(localEndpoint);
            while (true)
            {
                Console.WriteLine("\n Waiting for the next client message..");
                int b = sock.ReceiveFrom(buffer, ref remoteEP);
                data = Encoding.ASCII.GetString(buffer, 0, b);
                Message clientMsg = JsonSerializer.Deserialize<Message>(data);
                Console.WriteLine("A message received from " + remoteEP.ToString() + " " + clientMsg.MsgType);

                // TODO:[Receive and print Hello]

                if (clientMsg.MsgType == MessageType.Hello)
                {
                    Console.WriteLine("Hello");

                    // TODO:[Send Welcome to the client]

                    Message message = new Message
                    {
                        MsgId = 2,
                        MsgType = MessageType.Welcome,
                        Content = "Welkom"
                    };
                    json = JsonSerializer.Serialize(message);
                    msg = Encoding.ASCII.GetBytes(json);
                    sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
                }
                if (clientMsg.MsgType == MessageType.DNSLookup)
                {

                    // TODO:[Receive and print DNSLookup]

                    Console.WriteLine(clientMsg.Content);

                    // TODO:[Query the DNSRecord in Json file]
                    bool isfound = false;
                    foreach (DNSRecord record in dNSRecords)
                    {
                        if (clientMsg.Content.ToString() == record.Name)
                        {
                            Message message = new Message
                            {
                                MsgId = 4,
                                MsgType = MessageType.DNSLookupReply,
                                Content = record
                            };
                            json = JsonSerializer.Serialize(message);
                            msg = Encoding.ASCII.GetBytes(json);
                            sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
                            isfound = true;
                        }

                    }
                    if (!isfound)
                    {
                        Message message = new Message
                        {
                            MsgId = 4,
                            MsgType = MessageType.Error,
                            Content = "Domain not found"
                        };
                        json = JsonSerializer.Serialize(message);
                        msg = Encoding.ASCII.GetBytes(json);
                        sock.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
                        isfound = true;
                    }
                    // TODO:[If found Send DNSLookupReply containing the DNSRecord]

                }


            }
        }
        catch
        {
            Console.WriteLine("\n Socket Error. Terminating");
        }













        // TODO:[If not found Send Error]


        // TODO:[Receive Ack about correct DNSLookupReply from the client]


        // TODO:[If no further requests receieved send End to the client]

    }


}