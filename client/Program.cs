﻿using System.Collections.Immutable;
using System.ComponentModel;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LibData;

// SendTo();
class Program
{
    static void Main(string[] args)
    {
        ClientUDP.start();
    }
}

public class Setting
{
    public int ServerPortNumber { get; set; }
    public string? ServerIPAddress { get; set; }
    public int ClientPortNumber { get; set; }
    public string? ClientIPAddress { get; set; }
}

class ClientUDP
{

    //TODO: [Deserialize Setting.json]
    static string configFile = @"../../../../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);


    public static void start()
    {

        //TODO: [Create endpoints and socket]
        byte[] buffer = new byte[1000];
        byte[] msg = new byte[1000];

        Socket sock;
        IPAddress ipAddress = IPAddress.Parse(setting.ClientIPAddress);
        IPEndPoint ServerEndpoint = new IPEndPoint(ipAddress, 32000);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteEP = (EndPoint)sender;

        //TODO: [Create and send HELLO]
        Message message = new Message
        {
            MsgId = 1,
            MsgType = MessageType.Hello,
            Content = "ikbendata"
        };
        string json = JsonSerializer.Serialize(message);
        try
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Console.WriteLine("Sending HELLO to server");
            msg = Encoding.ASCII.GetBytes(json);
            sock.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);

            //TODO: [Receive and print Welcome from server]

            int b = sock.ReceiveFrom(buffer, ref remoteEP);
            string data = Encoding.ASCII.GetString(buffer, 0, b);
            Console.WriteLine("Server said " + JsonSerializer.Deserialize<Message>(data).MsgType);
            sock.Close();
        }
        catch
        {
            Console.WriteLine("\n Socket Error. Terminating");
        }




        // TODO: [Create and send DNSLookup Message]
        Message message_DNSLookup = new Message
        {
            MsgId = 3,
            MsgType = MessageType.DNSLookup,
            Content = "www.mywebsite.com"
        };
        string json2 = JsonSerializer.Serialize(message_DNSLookup);
        try
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Console.WriteLine("Sending DNS Lookup to server");
            msg = Encoding.ASCII.GetBytes(json2);
            sock.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);

            //TODO: [Receive and print DNSLookupReply from server]

            int b = sock.ReceiveFrom(buffer, ref remoteEP);
            string data = Encoding.ASCII.GetString(buffer, 0, b);
            Console.WriteLine("Server said " + JsonSerializer.Deserialize<Message>(data).Content);
            sock.Close();
        }
        catch
        {
            Console.WriteLine("\n Socket Error. Terminating");
        }


        //TODO: [Send Acknowledgment to Server]

        // TODO: [Send next DNSLookup to server]
        // repeat the process until all DNSLoopkups (correct and incorrect onces) are sent to server and the replies with DNSLookupReply

        //TODO: [Receive and print End from server]





    }
}