using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class RessourceManager : MonoBehaviour
{
    public string RessourceID;
    public float processTime = 5.0f;
    public Transform input;
    public Transform output;
    public SimulationsManager simManager;
    public JObject currentTask;

    public Transform ObjectMove;
  

    List<Product> batch = new List<Product>();

    public List<SimulationsManager.Processes> Skills;

    /// <summary>
    /// Wie viele Teile auf einmal maximal bearbeitet werden können
    /// </summary>
    public int maxLoadAmount;

    public enum RessourceState
    {
        Idle, Processing, Broken, Preparing
    }
    public RessourceState state = RessourceState.Idle;

    private void Start()
    {
        if (processTime == 0)
        {
            processTime = 5.0f;
        }
        StartCoroutine(Routine());
    }

    public void HandOverProducts(List<Product> products)
    {
        foreach (Product p in products)
        {
            batch.Add(p);
        }
    }


    public IEnumerator Routine()
    {
        

        while (true)
        {
            //if (simManager == null || simManager.GameManager == null || !simManager.GameManager.isClientLeader)
            //{
            //    yield return new WaitForSeconds(0.5f);
            //    continue;
            //}

            switch (state)
            {
                case RessourceState.Idle:
                    break;
                case RessourceState.Processing:
                    while (state == RessourceState.Processing)
                    {
                        Debug.Log("Working on Task:\n" + currentTask.ToString());
                        // Jetzt Aufgabe ausführen
                        SimulationsManager.Processes process = SimulationsManager.Processes.Null;
                        System.Enum.TryParse(currentTask["processID"].ToString(), out process);

                        switch (process)
                        {
                            case SimulationsManager.Processes.Null:
                                Debug.Log("WARNING: Process Routine is null!");
                                break;
                            case SimulationsManager.Processes.Zusammensetzen:
                                yield return StartCoroutine(Zusammensetzen());
                                break;
                            case SimulationsManager.Processes.FuegenDurchLoeten:
                                yield return StartCoroutine(FuegenDurchLoeten());
                                break;
                            case SimulationsManager.Processes.Speichern:
                                yield return StartCoroutine(Speichern());
                                break;
                            case SimulationsManager.Processes.MengenVeraendern:
                                yield return StartCoroutine(MengenVeraendern());
                                break;
                            case SimulationsManager.Processes.Bewegen:
                                yield return StartCoroutine(Bewegen());
                                break;
                            case SimulationsManager.Processes.Sichern:
                                yield return StartCoroutine(Sichern());
                                break;
                            case SimulationsManager.Processes.Kontrollieren:
                                yield return StartCoroutine(Kontrollieren());
                                break;
                        }

                    }                   
                    break;
                case RessourceState.Broken: 
                    break;
                default:
                    break;
            }
            yield return null;
        }
    }

    // Ausführungen der Prozesse

    // Fügen DIN 8593
    public IEnumerator Zusammensetzen()
    {
        Debug.Log("Ressource " + RessourceID + " setzt jetzt Produkte zusammen.");
        yield return null;
    }

    public IEnumerator FuegenDurchLoeten()
    {
        Debug.Log("Ressource " + RessourceID + " lötet jetzt die Produkte zusammen.");
        yield return null;
    }

    // Handhaben VDI 2860

    public IEnumerator Speichern(Product[] products = null, RessourceManager target = null)
    {
        Debug.Log("Ressource " + RessourceID + " speichert jetzt die Produkte.");
        // Wo herausfinden
        Vector3 targetPos = Vector3.zero;
        for (int i = 0; i < simManager.ressources.Count; i++)
        {
            if (simManager.ressources[i].RessourceID == currentTask["wo"].ToString())
            {
                targetPos = simManager.ressources[i].input.position;
                break;
            }
        }

        // Hinlaufen
        yield return StartCoroutine(MoveToTarget(targetPos));
        Debug.Log("Point 20");
        // yield return new WaitForSeconds(3.0f/ simManager.simulationScale);
        // Jetzt Waren hinzufügen
        for (int i = 0; i < ((JArray)currentTask["outputProducts"]).Count; i++)
        {
            Debug.Log("Will produce Item " + ((JArray)currentTask["outputProducts"])[i].ToString());
            // Instanz von diesem Produkt hinzufügen
            GameObject ng = simManager.CreateNewProduct((JObject)((JArray)currentTask["outputProducts"])[i]);
            ng.transform.position = targetPos;
        }
        state = RessourceState.Idle;
        yield return simManager.UpdateTaskStatus(currentTask, "complete");
        //simManager.GameManager.sender.OnFinishedProductionTask(currentTask["taskID"].ToString());
        yield return null;
    }

    public IEnumerator MengenVeraendern()
    {
        Debug.Log("Ressource " + RessourceID + " verändert jetzt die Mengen der Produkte.");
        yield return null;
    }

    public IEnumerator Bewegen()
    {
        Debug.Log("Ressource " + RessourceID + " bewegt jetzt die Produkte.");
        // Ware finden
        Vector3 targetPos = Vector3.zero;

        // Zuerst selbst zur StartPosition bewegen
        for (int i = 0; i < simManager.ressources.Count; i++)
        {
            if (simManager.ressources[i].RessourceID == currentTask["startRessource"].ToString())
            {
                targetPos = simManager.ressources[i].output.position;
                break;
            }
        }
        yield return StartCoroutine(MoveToTarget(targetPos));

        List<Product> p = new List<Product>();
        for (int i = 0; i < ((JArray)currentTask["inputProducts"]).Count; i++)
        {
            // Konkrete ProduktID eingeplant
            if (((JArray)currentTask["inputProducts"])[i]["produktId"].ToString() != "")
            {
                Debug.Log("Need to process Item " + ((JArray)currentTask["inputProducts"])[i]["produktId"].ToString());
                // zuerst schauen, ob das Objekt schon nahe am Input liegt:
                Collider[] cols = Physics.OverlapSphere(input.position, 2.0f);
                bool success = false;
                foreach (Collider col in cols)
                {
                    if (col.GetComponent<Product>() != null && col.GetComponent<Product>().productID == ((JArray)currentTask["inputProducts"])[i]["produktId"].ToString())
                    {
                        Debug.Log("Input item is close to input");
                        p.Add(col.GetComponent<Product>());
                        success = true;
                        break;
                    }
                }
                if (!success)
                {
                    for (int j = 0; j < simManager.products.Count; j++)
                    {

                        if (simManager.products[j].productID == ((JArray)currentTask["inputProducts"])[i]["produktId"].ToString())
                        {
                            targetPos = simManager.products[j].transform.position;
                            Debug.Log("Material ist weiter weg und es muss dahin gelaufen werden.");
                            yield return StartCoroutine(MoveToTarget(targetPos));
                            p.Add(simManager.products[j]);
                            break;
                        }
                    }
                }
            }
            else if (((JArray)currentTask["inputProducts"])[i]["materialstamm"].ToString() != "")
            {
                Debug.Log("Need to process Item " + ((JArray)currentTask["inputProducts"])[i]["materialstamm"].ToString());
                // zuerst schauen, ob das Objekt schon nahe am Input liegt:
                Collider[] cols = Physics.OverlapSphere(input.position, 2.0f);
                bool success = false;
                foreach (Collider col in cols)
                {
                    if (col.GetComponent<Product>() != null && col.GetComponent<Product>().artikelStammNummer == ((JArray)currentTask["inputProducts"])[i]["materialstamm"].ToString())
                    {
                        Debug.Log("Input item is close to input");
                        p.Add(col.GetComponent<Product>());
                        success = true;
                        break;
                    }
                }
                // ist nicht in der Nähe, sondern muss von weiter weg bezogen werden (--> ne Art Kanban?)
                if (!success)
                {
                    for (int j = 0; j < simManager.products.Count; j++)
                    {
                        if (simManager.products[j].productID == "" && simManager.products[j].artikelStammNummer == ((JArray)currentTask["inputProducts"])[i]["materialstamm"].ToString())
                        {
                            targetPos = simManager.products[j].transform.position;
                            Debug.Log("Material ist weiter weg und es muss dahin gelaufen werden.");
                            yield return StartCoroutine(MoveToTarget(targetPos));
                            p.Add(simManager.products[j]);
                            break;
                        }
                    }
                }
                else
                {

                }
            }
        }
        if (p.Count != ((JArray)currentTask["inputProducts"]).Count)
        {
            Debug.Log("Input Products not available!");
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            yield return simManager.UpdateTaskStatus(currentTask, "processed");
            // Ziel finden und hinlaufen      
            targetPos = Vector3.zero;
            if (currentTask["targetRessource"] != null)
            {
                for (int i = 0; i < simManager.ressources.Count; i++)
                {
                    if (simManager.ressources[i].RessourceID == currentTask["targetRessource"].ToString())
                    {
                        targetPos = simManager.ressources[i].input.position;
                        break;
                    }
                }
            }
            // Alle zu transportierenden Produkte mitnehmen
            Transform par = null;
            for (int i = 0; i < p.Count; i++)
            {
                p[i].transform.position = input.position;
                par = p[i].transform.parent;
                p[i].transform.parent = transform;
            }
            // Hinlaufen
            yield return StartCoroutine(MoveToTarget(targetPos));

            for (int i = 0; i < p.Count; i++)
            {
                p[i].transform.position = targetPos;
                p[i].transform.parent = par;
            }
            yield return simManager.UpdateTaskStatus(currentTask, "complete");
             //simManager.GameManager.sender.OnFinishedProductionTask(currentTask["taskID"].ToString());
        }
        state = RessourceState.Idle;
        yield return null;
    }

    public IEnumerator Sichern()
    {
        Debug.Log("Ressource " + RessourceID + " sichert jetzt die Produkte.");
        yield return null;
    }

    public IEnumerator Kontrollieren()
    {
        Debug.Log("Ressource " + RessourceID + " kontrolliert jetzt die Produkte.");
        yield return null;
    }




    // Helpers:

    IEnumerator MoveToTarget(Vector3 target, float sqrStopDistance = 0.01f, float moveSpeed = 1.5f)
    {
        float pY = target.y;
        target.y = ObjectMove.position.y;
        int rnd = Random.Range(0, 1);
        Debug.Log("Moving to target: " + target.ToString());
        // Hier ggf. komplexeren NavMesh machen

        while (ObjectMove != null && Quaternion.Angle (ObjectMove.rotation, Quaternion.LookRotation(target - ObjectMove.position)) > 1)
        {
            if (rnd == 0) ObjectMove.Rotate(0, -145 * Time.deltaTime, 0);     
            else if (rnd == 0) ObjectMove.Rotate(0, 145 * Time.deltaTime, 0);     
            yield return null;
        }
        target.y = pY;
        while (ObjectMove != null && Vector3.SqrMagnitude(target - ObjectMove.position) > sqrStopDistance)
        {
            ObjectMove.position += ((target - ObjectMove.position).normalized * Time.deltaTime * moveSpeed);
            yield return null;
        }
    }

}
