using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    GameObject tempServerGO, tempClientGO;
    DatabaseAPI database;
    public PermissionsManager permissionsManager;

    // Start is called before the first frame update
    void Start()
        {
        tempServerGO = null;
        tempClientGO = null;

        database = GameObject.Find("DatabaseManager").GetComponent<DatabaseAPI>();
        database.ListenForSelectedObject(SelectObject, Debug.Log);

        //Reset selection to avoid the leak material bug on Edit mode
        GameObject[] interactables = GameObject.FindGameObjectsWithTag("Interactable");
        foreach (GameObject interactable in interactables)
            {
            if (interactable.TryGetComponent(out Renderer renderer))
                {
                renderer.material.SetColor("_Color", Color.white);
                }
            else
                {
                foreach (Transform child in interactable.transform)
                    {
                    child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                    }
                }
            }
    }

    void Update()
    {
        //If Server Side, enable selecting
        if (permissionsManager.serverSide)
        {
            if (((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) || (Input.GetMouseButton(0)))
            {
                Ray raycast;
                if (Input.touchCount > 0)
                {
                    raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                }
                else
                {
                    raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
                }
                RaycastHit raycastHit;
                if (Physics.Raycast(raycast, out raycastHit))
                {
                    if (raycastHit.collider.CompareTag("Interactable"))
                    {
                        //Debug.Log(raycastHit.collider.name);
                        SelectObjectForServer(raycastHit);
                    }
                }
            }
        }
    }

    //Logic to execute when an object is selected
    //WARNING: When using this during edit mode, the colored Material WILL LEAK and be kept for the next play,
    //It's needed to re-select objects to reset to default material
    //This can be fixed by using a shared material
    void SelectObject(Selected selected)
    {
        //Check if an Object is Selected initially (0 is when no Object is selected on a new Database)
        if (selected.name != "0")
            {
            Debug.Log("Log from Firebase: An Object with the name (" + selected.name + ") is selected by a Server. Processing with logic...");
            GameObject selectedGameObject = GameObject.Find(selected.name);

            //PS: in this case, if there are 2 Servers, the inactive Server is considered as Client and therefore
            //uses the logic of the Selction for the Client
            SelectObjectForClient(selectedGameObject);
            }
        else
            {
            Debug.Log("Log from Firebase: No Objects are selected initially.");
            }
    }


    void SelectObjectForServer(RaycastHit raycastHit)
    {
        GameObject hitGameObject = raycastHit.collider.gameObject;

        //Update selected Object in Database
        database.UpdateSelectObject("Server", hitGameObject.name, () => Debug.Log("Log from App: Object (" + hitGameObject.name
            + ") selected."), exception => Debug.LogError(exception));

        //Checking if GameObject has a Renderer
        if (hitGameObject.TryGetComponent(out Renderer rendCurrent))
            {
            rendCurrent.material.SetColor("_Color", Color.red);
            }
        else
            {
            //Else access the materials of its childs
            foreach (Transform child in raycastHit.collider.gameObject.transform)
                {
                child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                }
            }
        if (tempServerGO && (tempServerGO != raycastHit.collider.gameObject))
            {
            if (tempServerGO.TryGetComponent(out Renderer rendPrevious))
                {
                rendPrevious.material.SetColor("_Color", Color.white);
                }
            else
                {
                foreach (Transform child in tempServerGO.transform)
                    {
                    child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                    }
                }
            }
        tempServerGO = raycastHit.collider.gameObject;
    }

    //Similar logic to Server Side but with different checks (On Gameobject exits instead of RaycastHit)
    void SelectObjectForClient(GameObject selectedGameObject)
    {
        if (selectedGameObject)
            {
            if (selectedGameObject.TryGetComponent(out Renderer rendCurrent))
                {
                rendCurrent.material.SetColor("_Color", Color.red);
                }
            else
                {
                foreach (Transform child in selectedGameObject.transform)
                    {
                    child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                    }
                }
            }

        if (tempClientGO)
            {
            if (tempClientGO.TryGetComponent(out Renderer rendPrevious))
                {
                rendPrevious.material.SetColor("_Color", Color.white);
                }
            else
                {
                foreach (Transform child in tempClientGO.transform)
                    {
                    child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                    }
                }
            }
        tempClientGO = selectedGameObject;
        }


    }
