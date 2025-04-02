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

        Message ACKMessage = new Message
        {
            MsgId = 3,
            MsgType = MessageType.Ack,
            Content = "Ack message",
        };

        Socket socket;
        IPAddress ServerIP = IPAddress.Parse(setting.ServerIPAddress);

        IPEndPoint ServerEndpoint = new IPEndPoint(ServerIP, setting.ServerPortNumber);


        IPAddress ClientIP = IPAddress.Parse(setting.ClientIPAddress);
        IPEndPoint sender = new IPEndPoint(ClientIP, 2002);
        EndPoint remoteEP = (EndPoint)sender;

        //TODO: [Create and send HELLO]
        Message? ResponseMessage = null;
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // socket.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);
            Console.WriteLine("Client Side Started...\n");

            //TODO: [Receive and print Welcome from server]
            ResponseMessage = SendMessage(socket, MessageObj, ServerEndpoint, remoteEP);
            Console.WriteLine("\n========= Sending a Hello message =========\n");
            PrintMessage(ResponseMessage);

            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"Count: {i}");

                if (i >= 2)
                {
                    DNSLookupMessage = new Message
                    {
                        MsgId = 5,
                        MsgType = MessageType.DNSLookup,
                        Content = new DNSRecord
                        {
                            Type = "Z",
                            Name = "unkown.com"
                        },
                    };
                }
                // TODO: [Create and send DNSLookup Message]
                ResponseMessage = SendMessage(socket, DNSLookupMessage, ServerEndpoint, remoteEP);
                Console.WriteLine("\n========= Sending a DNSLookup message =========\n");

                //TODO: [Receive and print DNSLookupReply from server]
                PrintMessage(ResponseMessage);

                //TODO: [Send Acknowledgment to Server]
                ACKMessage.MsgId = ResponseMessage.MsgId;
                ResponseMessage = SendMessage(socket, ACKMessage, ServerEndpoint, remoteEP);
                Console.WriteLine("\n========= Sending an Acknowledgment message =========\n");
                PrintMessage(ResponseMessage);
            }



        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured:" + ex.Message);

        }







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

    private static void PrintMessage(Message? message)
    {
        Thread.Sleep(2000);
        if (message == null)
        {
            Console.WriteLine("No Message Recieved");
        }
        else
        {
            Console.WriteLine($"Recieved a message of type {message.MsgType}\n");
            Console.WriteLine("Message Id: " + message.MsgId);
            Console.WriteLine("Content: " + message.Content);
        }
    }

}