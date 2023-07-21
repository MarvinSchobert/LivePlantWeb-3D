using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    NetworkConnector_Receive receiver;
    NetworkConnector_Send sender;
    public bool isClientLeader;
    public List<SynchronizedObject> synchronizedObjects;
    public List<SynchronizedObject> waitForIDResponse;

    public enum Platform
    {
        WebGL, Windows, Android
    }
    public Platform platform = Platform.WebGL;

    public List<SynchronizedObject> multiplayerObjectDatabase;

    public string clientID = "";
    public string userName = "Player";
    public int clientPort = 5555;

    // Start is called before the first frame update
    public void Start()
    {
        synchronizedObjects = new List<SynchronizedObject>();
        waitForIDResponse = new List<SynchronizedObject>();
        receiver = GetComponent<NetworkConnector_Receive>();   
        sender = GetComponent<NetworkConnector_Send>();
        receiver.manager = this;
        sender.manager = this;

        // multiplayerObjectDatabase initiieren:
        GameObject[] allResources = Resources.LoadAll("", typeof(GameObject)).Cast<GameObject>().ToArray();
        foreach (GameObject go in allResources)
        {
            if (go.GetComponent<SynchronizedObject>() != null)
            {
                multiplayerObjectDatabase.Add(go.GetComponent<SynchronizedObject>());
            }
        }


        StartCoroutine(UpdateSyncObjects());
    }

    IEnumerator UpdateSyncObjects()
    {
        yield return new WaitForSeconds(1.0f);
        while (true)
        {
            JObject message = new JObject();
            JArray sendObjects = new JArray();
            message["sendObjects"] = sendObjects;

            // Findet alle SynchronizedObjects in der Szene und wenn sie hier noch nicht drin sind, erstellen
            GameObject[] allSyncObjects = GameObject.FindGameObjectsWithTag("SynchronizedObject");
            foreach (GameObject go in allSyncObjects)
            {
                SynchronizedObject obj = go.GetComponent<SynchronizedObject>();
                if (obj != null && !synchronizedObjects.Contains(obj))
                {
                    synchronizedObjects.Add(obj);
                }
            }

            if (clientID != "")
            {
                foreach (SynchronizedObject syncObject in synchronizedObjects)
                {
                    // Create and share with Network?
                    if (syncObject.initNetworkCreation)
                    {
                        syncObject.manager = this;
                        syncObject.ownerClientId = clientID;
                        syncObject.Init();
                        JObject data = new JObject();
                        data = syncObject.serializeSyncObject();
                        syncObject.reqId = Random.Range(1, 50000).ToString();
                        data["reqId"] = syncObject.reqId.ToString();
                        data["type"] = "CREATE";
                        data["clientId"] = clientID;
                        sendObjects.Add(data);
                        waitForIDResponse.Add(syncObject);
                    }

                    // Update and share with Network?
                    else if (syncObject.ID != "-1" && syncObject.synchronizeTransform && syncObject.transformHasChanged())
                    {
                        JObject data = new JObject();
                        data = syncObject.serializeSyncObject();
                        data["type"] = "UPDATE";
                        data["id"] = syncObject.ID;
                        data["clientId"] = clientID;
                        sendObjects.Add(data);
                    }

                    // Delete and share with Network?


                }
                if (sendObjects.Count > 0)
                {
                    Debug.Log("[NETWORK_MANAGER]: Sending Message with " + sendObjects.Count + " items. \n" + message.ToString());
                    sender.sendObject(message);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    public string guiText;

    public void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, 300));
        GUILayout.BeginHorizontal();
        GUILayout.Box(guiText);
        if (GUILayout.Button("Quit"))
        {
            sender.Request_Unregister();
            Application.Quit();
        }
        GUILayout.EndHorizontal();
        if (!receiver.initialized)
        {
            userName = GUILayout.TextField(userName);
            int.TryParse(GUILayout.TextField(clientPort.ToString()), out clientPort);
            if (GUILayout.Button("Initialize Network"))
            {
                sender.Initialize();
                receiver.Initialize();               
                guiText += "\n[NETWORK_MANAGER] Initialization successful";
            }
        }
        GUILayout.EndArea();
    }
    public void CreateSyncObjectFromMessage(JObject obj)
    {
        foreach (SynchronizedObject s in multiplayerObjectDatabase)
        {
            if (s.prefabID == obj["prefabId"].ToString())
            {
                GameObject newObject = GameObject.Instantiate(s.gameObject);
                SynchronizedObject syncObject = newObject.GetComponent<SynchronizedObject>();
                syncObject.manager = this;
                syncObject.Init();
                syncObject.deserializeSyncObject(obj);
                break;
            }
        }        
    }

    public void UpdateSyncObjectFromMessage(JObject obj)
    {
        foreach (SynchronizedObject s in synchronizedObjects)
        {
            if (s.ID == obj["id"].ToString())
            {
                SynchronizedObject syncObject = s.GetComponent<SynchronizedObject>();
                syncObject.deserializeSyncObject(obj);
                break;
            }
        }
    }
    
}
