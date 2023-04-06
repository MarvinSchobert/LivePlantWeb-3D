using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
public class NetworkConnector_Send : MonoBehaviour
{
    private string IP; 
    public int clientPort; 
    public int serverPort;
    public NetworkManager manager;
    public bool initialized;
   
    IPEndPoint remoteEndPoint;
    UdpClient client;

    //Creates an IPEndPoint to record the IP Address and port number of the sender.
    // The IPEndPoint will allow you to read datagrams sent from any source.
    IPEndPoint RemoteIpEndPoint;

    
    public void OnApplicationQuit()
    {
        Request_Unregister();
    }

    public void Initialize()
    {
        manager.guiText += "\nSetting Target IP-Adress";
        IP = "192.168.137.1";
        //IP = "127.0.0.1";
        serverPort = 33333;

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), serverPort);
        manager.guiText += "\nSuccess 0";
        client = new UdpClient();
        
        Debug.Log("Is connected to client");
        manager.guiText += "\nSuccess 1";
        Request_Register();
        initialized = true;
        manager.guiText += "\nSuccess 2";
    }
    public void sendObject(JObject obj)
    {
        manager.guiText += "\nSending";
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(obj.ToString());
            client.Send(data, data.Length, remoteEndPoint);
            manager.guiText += "\nSending Successfull";
        }
        catch (Exception err)
        {
            Debug.Log("Error");
            manager.guiText += "Send data not Successfull " + err.ToString();
        }
    }
    public void Request_Register()
    {
        JObject rqt = new JObject();
        rqt["type"] = "REGISTER";
        rqt["userName"] = manager.userName;
        rqt["port"] = clientPort.ToString();

        JArray sendObjects = new JArray();
        sendObjects.Add(rqt);

        JObject obj = new JObject();
        obj["sendObjects"] = sendObjects;

        sendObject(obj);
    }
    public void Request_Unregister()
    {
        JObject rqt = new JObject();
        rqt["type"] = "UNREGISTER";
        rqt["userName"] = "Marvin Schobert WebGL";
        rqt["port"] = clientPort.ToString();
        rqt["clientId"] = manager.clientID;

        
        JArray sendObjects = new JArray();
        sendObjects.Add(rqt);

        JObject obj = new JObject();
        obj["sendObjects"] = sendObjects;
        
        sendObject(obj);
    }

}
