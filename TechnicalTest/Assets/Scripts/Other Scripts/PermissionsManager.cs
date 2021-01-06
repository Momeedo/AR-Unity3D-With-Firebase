using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PermissionsManager : MonoBehaviour
{
    public bool serverSide = false;

    [HideInInspector]
    public string uniqueID;

    public Text permissionLevelText;
    public GameObject buttonResetDrawings;

    private void Awake()
    {
        //Generate the ID of the session
        uniqueID = Guid.NewGuid().ToString("N");

        if (serverSide)
        {
            permissionLevelText.text = "Server Side (#" + uniqueID + ")";
            buttonResetDrawings.SetActive(true);
        }
        else
        {
            permissionLevelText.text = "Client Side (#" + uniqueID + ")";
        }
    }
}
