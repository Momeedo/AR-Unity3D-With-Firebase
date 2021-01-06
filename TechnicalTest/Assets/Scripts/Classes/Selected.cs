using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Selected
{
    public string selector; //For future implementation if we add seperate selection by Server user
    public string name;

    public Selected(string selector, string name)
        {
        this.selector = selector;
        this.name = name;
        }
}
