using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SimulationsManager : MonoBehaviour
{

    public float simulationScale = 1;

    public float taskRefreshRate = 3.0f;

    public List<RessourceManager> ressources = new List<RessourceManager>();
    public GameObject ProductPrefab;
    public List<Product> products = new List<Product>();

    public enum Processes
    {
        Zusammensetzen, FuegenDurchLoeten, Speichern, MengenVeraendern, Bewegen, Sichern, Kontrollieren, Null
    }


    [System.Serializable]
    public struct materialstamm
    {
        public string materialbezeichnung;
        public string materialstammID;
        public string produktTyp;
        public string bezug;
    }

    public List<materialstamm> materialstammdaten;

    public void Start()
    {
        StartCoroutine(ProductionRoutine());
        StartCoroutine(SyncMaterialstammData());

    }

    IEnumerator SyncMaterialstammData()
    {
        while (true)
        {
            //if (GameManager == null || !GameManager.isClientLeader)
            //{
            //    yield return new WaitForSeconds(0.5f);
            //    continue;
            //}
            // Bekomme alle Produktinfos:
            UnityWebRequest uwr = UnityWebRequest.Get("localhost:3000/erpSystem/materialstamm");
            yield return uwr.SendWebRequest();
            JArray stammdaten = JArray.Parse(uwr.downloadHandler.text);
            materialstammdaten = new List<materialstamm>();
            materialstammdaten.Clear();
            for (int i = 0; i < stammdaten.Count; i++)
            {
                materialstamm m = new materialstamm();
                m.materialbezeichnung = stammdaten[i]["itemName"].ToString();
                m.materialstammID = stammdaten[i]["itemId"].ToString();
                m.produktTyp = stammdaten[i]["produktTyp"].ToString();
                m.bezug = stammdaten[i]["typ"].ToString();
                materialstammdaten.Add(m);
            }


            yield return new WaitForSeconds(8.0f);
        }
    }

    IEnumerator ProductionRoutine()
    {
        //GameManager = GameObject.FindWithTag("GameManagement").GetComponent<GameMangagement>();
        while (true)
        {
            //if (GameManager == null || !GameManager.isClientLeader)
            //{
            //    yield return new WaitForSeconds(0.5f);
            //    continue;
            //}
            // Die ausstehenden Aufgaben regelm‰ﬂig abfragen
            UnityWebRequest uwr = UnityWebRequest.Get("localhost:3000/mes/getActiveProductionTasks");
            yield return uwr.SendWebRequest();
            JArray productionTasks = JArray.Parse(uwr.downloadHandler.text);

            // Gesamtes Equipment in der Szene finden:


            for (int i = 0; i < productionTasks.Count; i++)
            {
                JObject task = (JObject)productionTasks[i];
               

                if (task["taskStatus"].ToString() == "active")
                {
                    SimulationsManager.Processes process = SimulationsManager.Processes.Null;
                    System.Enum.TryParse(task["processID"].ToString(), out process);
                    for (int j = 0; j < ressources.Count; j++)
                    {
                        if (ressources[j].state == RessourceManager.RessourceState.Idle && ressources[j].Skills.Contains(process) && ressources[j].RessourceID == task["ressourceID"].ToString())
                        {
                            ressources[j].state = RessourceManager.RessourceState.Processing;
                            Debug.Log("Ressource Nr " + j + " is processing Task Nr " + i + " now with Process " + task["processID"].ToString());
                            ressources[j].currentTask = task;
                        }
                    }
                }
            }
            if (taskRefreshRate <= 0) taskRefreshRate = 1.0f;
            yield return new WaitForSeconds(taskRefreshRate);
        }
    }

    public IEnumerator UpdateTaskStatus(JObject task, string newStatus)
    {
        task["taskStatus"] = newStatus;
        ((JArray)task["taskHistory"]).Add(JToken.Parse("{\"status\":\"" + newStatus + "\", \"time\":\"" + System.DateTime.Now + "\"}"));
        string s = task.ToString(Newtonsoft.Json.Formatting.None);
        UnityWebRequest uwr2 = new UnityWebRequest("localhost:3000/mes/updateProductionTask", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(s);
        uwr2.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        uwr2.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr2.SetRequestHeader("Content-Type", "application/json");
        yield return uwr2.SendWebRequest();
    }

    public GameObject CreateNewProduct(JObject prod)
    {

        Debug.Log("Creating Product with the Stamm-ID: " + prod["materialstamm"]);
        for (int i = 0; i < materialstammdaten.Count; i++)
        {
            if (materialstammdaten[i].materialstammID == prod["materialstamm"].ToString())
            {
                GameObject newProduct = Instantiate(ProductPrefab, Vector3.zero, Quaternion.identity);
                GameObject productHolderObject = GameObject.Find("ProductHolder");
                if (productHolderObject == null) productHolderObject = new GameObject("ProductHolder");
                newProduct.transform.parent = productHolderObject.transform;
                newProduct.name = materialstammdaten[i].materialbezeichnung;
                newProduct.GetComponent<Product>().artikelStammNummer = prod["materialstamm"].ToString();
                newProduct.GetComponent<Product>().productID = prod["produktId"].ToString();
                newProduct.GetComponent<Product>().produktBezeichnung = materialstammdaten[i].materialbezeichnung;
                newProduct.GetComponent<Product>().produktTyp = materialstammdaten[i].produktTyp;
                newProduct.GetComponent<Product>().bezugsArt = materialstammdaten[i].bezug;
                products.Add(newProduct.GetComponent<Product>());
                return newProduct;
            }
        }
        return null;
    }   

}
