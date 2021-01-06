using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLine : MonoBehaviour
{
    [HideInInspector]
    public bool drawEnabled = false;

    DatabaseAPI database;
    PermissionsManager permissionsManager;

    public GameObject drawButtonGO;
    public GameObject TrailPrefab;

    const int MAX_POSITIONS = 1000;
    Vector3[] TrailRecorded = new Vector3[MAX_POSITIONS];

    Image icon;

    void Start()
    {
        database = GameObject.Find("DatabaseManager").GetComponent<DatabaseAPI>();
        permissionsManager = GameObject.Find("PermissionsManager").GetComponent<PermissionsManager>();

        database.ListenForTrailChanges(DrawOthersTrails, Debug.Log);

        icon = drawButtonGO.GetComponent<Image>();

        //IMPORTANT: Augmenter l'occurence = augmenter les risques de latences importantes
        //Recommended: 3f~5f
        InvokeRepeating("SaveOwnTrailToDatabase", 0f, 2f);
    }

    void Update()
    {
        if (drawEnabled)
        {
            if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
                || Input.GetMouseButton(0)))
            {

                Plane objPlane = new Plane(Camera.main.transform.forward * -1, this.transform.position);

                Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayDistance;
                if (objPlane.Raycast(mRay, out rayDistance))
                    this.transform.position = mRay.GetPoint(rayDistance);
            }
        }

        //Number of TrailRenderer positions
        int numberOfPositions = GetComponent<TrailRenderer>().GetPositions(TrailRecorded);
        //Debug.Log("Log from App: Current user's Trail has positions: " + numberOfPositions);

    }

    void SaveOwnTrailToDatabase()
    {
        database.ResetTrail(permissionsManager.uniqueID, new Trail(TrailRecorded, permissionsManager.uniqueID));
    }

    //Draw the trails of the other users
    void DrawOthersTrails(Trail trail)
    {
        Debug.Log("Drawing Trail for user #" + trail.user);

        if (trail.user != permissionsManager.uniqueID)
            {
                GameObject lineToDraw;
                if (GameObject.Find("Trail-" + trail.user) == null)
                    {
                    GameObject intanciatedTrail = Instantiate(TrailPrefab);
                    intanciatedTrail.name = "Trail-" + trail.user;
                    lineToDraw = intanciatedTrail;
                    }
                else
                    {
                    lineToDraw = GameObject.Find("Trail-" + trail.user);
                    }


                int activePositions = 0;
                for (int i = 0; i < trail.positions.Length; i++)
                    {

                    if (trail.positions[i] != Vector3.zero)
                        {
                        activePositions++;
                        }
                    }

                lineToDraw.GetComponent<LineRenderer>().SetVertexCount(activePositions);

                for (int i = 0; i < trail.positions.Length; i++)
                {

                    if ((trail.positions[i] != Vector3.zero) && (i < activePositions))
                    {
                        print(trail.positions[i] + " VS " + Vector3.zero);
                        lineToDraw.GetComponent<LineRenderer>().SetPosition(i, trail.positions[i]);
                    }
                }
            }

    }

    public void DrawAction()
    {
        if (drawEnabled)
        {
            drawEnabled = false;
            icon.color = new Color32(120, 197, 193, 200);
        }
        else
        {
            drawEnabled = true;
            icon.color = new Color32(217, 4, 35, 200);
        }
    }

    public void ResetDrawingsAction()
    {
        GetComponent<TrailRenderer>().Clear();
        GameObject[] othersTrails = GameObject.FindGameObjectsWithTag("Trail");
        foreach (GameObject othersTrail in othersTrails)
        {
            Destroy(othersTrail);
        }
        database.DeleteAllTrails(() =>
        {
            Debug.Log("Log from Firebase: All Trails data deleted.");
            
        },
        exception => Debug.LogError(exception));
    }

}