using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;

public class NetworkConnector_Receive : MonoBehaviour
{
    int clientPort;
    public NetworkManager manager;
    public bool initialized;
    UdpClient receivingUdpClient;

    //Creates an IPEndPoint to record the IP Address and port number of the sender.
    // The IPEndPoint will allow you to read datagrams sent from any source.
    IPEndPoint RemoteIpEndPoint;
    Thread receiveThread;

    List<JObject> data = new List<JObject>();

   

    // Start is called before the first frame update
    public void Initialize()
    {
        data = new List<JObject>();
        clientPort = manager.clientPort;
        if (manager.platform != NetworkManager.Platform.WebGL)
        {
            receiveThread = new Thread(
               new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            initialized = true;
        }
    }
    public void Update()
    {
        if (data.Count > 0)
        {
            progressData(0);
        }
    }

    // Method for Device and messaging direct via Unity
    private void ReceiveData()
    {
        receivingUdpClient = new UdpClient(clientPort);
        RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        manager.guiText += "\n[RECEIVER] Start ReceiveData Successfull";
        Debug.Log(clientPort);
        while (true)
        {
            try
            {
                manager.guiText += "\n[RECEIVER] In Loop";
                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                JObject obj = JObject.Parse(returnData);
                manager.guiText += "\n[RECEIVER] Got Data";
                if (data.Count > 10 && obj["type"].ToString() != "ChangeInfo")
                {
                    // Wenn zu viele Nachrichten eingehen, die ChangeInfos ignorieren. Alle anderen werden weiterhin berücksichtigt.
                }
                else
                {
                    data.Add(obj);
                }


            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                manager.guiText += "\n[NETWORK_MANAGER] Error Receive: " + e.ToString();
            }
        }
    }   

    void progressData(int idx)
    {
        manager.guiText += "\n[RECEIVER] Receiving Message";

        switch (data[idx]["type"].ToString())
        {
            case "CREATE":                
                Debug.Log("[CREATING] Creating Sync Object from network message.");
                manager.CreateSyncObjectFromMessage(data[idx]);
                break;
            case "READ":
                Debug.Log("Reading");
                break;
            case "UPDATE":
                Debug.Log("[UPDATING] Updating Sync Object from network message.");
                manager.UpdateSyncObjectFromMessage(data[idx]);
                break;
            case "DELETE":
                // TODO
                Debug.Log("Deleting");
                break;
            case "CLIENT_LEADERSHIP":
                Debug.Log("[RECEIVE] Got message to activate client leadership.");
                manager.isClientLeader = true;
                break;
            case "CREATE_ID_RESPONSE":
                Debug.Log("[RECEIVE] Got Create_ID_Response with message: " + data[idx].ToString());
                for (int i = 0; i < manager.waitForIDResponse.Count; i++)
                {
                    if (manager.waitForIDResponse[i].reqId != null && data[idx]["reqId"] != null && manager.waitForIDResponse[i].reqId.ToString() == data[idx]["reqId"].ToString())
                    {
                        manager.waitForIDResponse[i].ID = data[idx]["id"].ToString();
                        manager.waitForIDResponse.RemoveAt(i);
                        break;
                    }
                }
                break;
            case "REGISTER_ID_RESPONSE":
                Debug.Log("[RECEIVE] Got Register_ID_Response with message: " + data[idx].ToString());                
                manager.clientID = data[idx]["id"].ToString();   
                break;
            default:
                Debug.Log(data[idx].ToString());
                break;
        }
        data.RemoveAt(idx);
    }
    public void OnApplicationQuit()
    {
        receivingUdpClient.Close();
        receiveThread.Abort();
    }
}
