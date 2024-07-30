using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Trail
{
    public string user;
    public Vector3[] positions;

    public Trail(Vector3[] positions, string user)
        {
        this.positions = positions;
        this.user = user;
        }
}
