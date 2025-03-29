using System.Collections.Immutable;
using System.ComponentModel;
using System.Net;
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
    static string configFile = @"../Setting.json";
    static string configContent = File.ReadAllText(configFile);
    static Setting? setting = JsonSerializer.Deserialize<Setting>(configContent);
    static byte[] buffer = new byte[1000];


    public static void start()
    {


        //TODO: [Create endpoints and socket]
        Message MessageObj = new Message { MsgId = 1, MsgType = MessageType.Hello, Content = "Hello" };
        string json = JsonSerializer.Serialize(MessageObj);
        byte[] msg = Encoding.ASCII.GetBytes(json);

        DNSRecord dNSRecord = new DNSRecord
        {
            Type = "A",
            Name = "www.test.com"
        };

        Message DNSLookupMessage = new Message
        {
            MsgId = 2,
            MsgType = MessageType.DNSLookup,
            Content = dNSRecord,
        };

        Socket socket;
        IPAddress ServerIP = IPAddress.Parse(setting.ServerIPAddress);

        IPEndPoint ServerEndpoint = new IPEndPoint(ServerIP, setting.ServerPortNumber);


        IPAddress ClientIP = IPAddress.Parse(setting.ClientIPAddress);
        IPEndPoint sender = new IPEndPoint(ClientIP, 2002);
        EndPoint remoteEP = (EndPoint)sender;

        //TODO: [Create and send HELLO]

        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // socket.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);

            //TODO: [Receive and print Welcome from server]
            Message? ResponseMessage = SendMessage(socket, MessageObj, ServerEndpoint, remoteEP);
            if (ResponseMessage != null)
            {
                Console.WriteLine("=====Server Response======\n" + ResponseMessage);
                Console.WriteLine("Response Content: " + ResponseMessage.Content);
            }

            // TODO: [Create and send DNSLookup Message]

            



        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured:" + ex.Message);

        }




        //TODO: [Receive and print DNSLookupReply from server]


        //TODO: [Send Acknowledgment to Server]

        // TODO: [Send next DNSLookup to server]
        // repeat the process until all DNSLoopkups (correct and incorrect onces) are sent to server and the replies with DNSLookupReply

        //TODO: [Receive and print End from server]





    }

    public static Message? SendMessage(Socket socket, Message MessageObj, EndPoint ServerEndpoint, EndPoint RemoteEndpoint)
    {
        string json = JsonSerializer.Serialize(MessageObj);
        byte[] msg = Encoding.ASCII.GetBytes(json);
        socket.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);
        int b = socket.ReceiveFrom(buffer, ref RemoteEndpoint);

        string JsonResponse = Encoding.ASCII.GetString(buffer, 0, b);
        Message RespnseMessage = JsonSerializer.Deserialize<Message>(JsonResponse)!;

        return RespnseMessage;



    }
}