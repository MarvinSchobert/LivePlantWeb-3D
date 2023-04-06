using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class SynchronizedObject : MonoBehaviour
{
    public bool synchronizeTransform;
    public bool initNetworkCreation = true;
    public bool deleteOnClientQuit;
    public string ID = "-1";
    public string reqId;
    public string prefabID;
    public string ownerClientId;
    public bool hasPhysics;
    public bool isNetworkControlled;

    public NetworkManager manager;
    JObject serializedTransform;
    public JObject variables;
    public void Init()
    {
        serializedTransform = new JObject();
        variables = new JObject();
        serializedTransform["position"] = new JObject();
        serializedTransform["rotation"] = new JObject();
        serializedTransform["scale"] = new JObject();
        initNetworkCreation = false;
    }

    public JObject transformToSerialized()
    {
        serializedTransform["position"]["x"] = transform.position.x;
        serializedTransform["position"]["y"] = transform.position.y;
        serializedTransform["position"]["z"] = transform.position.z;
        serializedTransform["rotation"]["x"] = transform.rotation.x;
        serializedTransform["rotation"]["y"] = transform.rotation.y;
        serializedTransform["rotation"]["z"] = transform.rotation.z;
        serializedTransform["rotation"]["w"] = transform.rotation.w;
        serializedTransform["scale"]["x"] = transform.localScale.x;
        serializedTransform["scale"]["y"] = transform.localScale.y;
        serializedTransform["scale"]["z"] = transform.localScale.z;
        return serializedTransform;
    }

    public Vector3 serializedToPosition()
    {
        Vector3 position = new Vector3();
        position.x = (float)serializedTransform["position"]["x"];
        position.y = (float)serializedTransform["position"]["y"];
        position.z = (float)serializedTransform["position"]["z"];
        return position;
    }
    public Quaternion serializedToRotation()
    {
        Quaternion rotation = new Quaternion();
        rotation.x = (float)serializedTransform["rotation"]["x"];
        rotation.y = (float)serializedTransform["rotation"]["y"];
        rotation.z = (float)serializedTransform["rotation"]["z"];
        rotation.w = (float)serializedTransform["rotation"]["w"];
        return rotation;
    }
    public Vector3 serializedToLocalScale()
    {
        Vector3 localScale = new Vector3();
        localScale.x = (float)serializedTransform["scale"]["x"];
        localScale.y = (float)serializedTransform["scale"]["y"];
        localScale.z = (float)serializedTransform["scale"]["z"];
        return localScale;
    }

    public bool transformHasChanged()
    {      
        if (transform.position != serializedToPosition())
        {
            return true;
        }
        if (transform.rotation != serializedToRotation())
        {
            return true;
        }
        if (transform.localScale != serializedToLocalScale())
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// From Unity Object to JSON
    /// </summary>
    /// <returns></returns>
    public JObject serializeSyncObject()
    {
        JObject result = new JObject();
        result["transform"] = transformToSerialized();
        result["id"] = ID;
        result["prefabId"] = prefabID;
        result["name"] = name;
        result["deleteOnClientQuit"] = deleteOnClientQuit.ToString();
        result["ownerClientId"] = ownerClientId;
        result["variables"] = variables;
        result["hasPhysics"] = hasPhysics.ToString();

        return result;
    }

    /// <summary>
    /// From JSON to Unity Object
    /// </summary>
    public void deserializeSyncObject(JObject obj)
    {
        serializedTransform = (JObject) obj["transform"];
        transform.position = serializedToPosition();
        transform.rotation = serializedToRotation();
        transform.localScale = serializedToLocalScale();
        ID = obj["id"].ToString();
        name = obj["name"].ToString();
        ownerClientId = obj["ownerClientId"].ToString();
        variables = (JObject) obj["variables"];
        hasPhysics = (bool) obj["hasPhysics"];
    }
}
