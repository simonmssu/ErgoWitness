﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ben Hoffman
/// This class holds the Data that this computer has, and a list
/// of computers that it is conenct to
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Fade_UI))]
public class Computer : MonoBehaviour {

    #region Fields
    public Source computerSourceInfo;          // The data that I care about for each PC
    /// <summary>
    /// Use a linked list for this because it is better for insertion
    /// but the same for searching, there are only benefits to this
    /// </summary>
    public LinkedList<GameObject> connectedPCs;
    private Fade_UI UI;         // The UI for me to use
    public double numHits;     // How many times have we seen this IP get hit?
    private LineRenderer lineRend;  // The line renderer
    #endregion

    #region Mutators
    public Source ComputerSourceInfo { get { return computerSourceInfo; } set { computerSourceInfo = value; } }
    public double NumHits { get { return numHits; } }
    #endregion

    /// <summary>
    /// Author: Ben Hoffman
    /// Set up the components that I need
    /// </summary>
    void Awake()
    {
        connectedPCs = new LinkedList<GameObject>();
        UI = GetComponent<Fade_UI>();

        numHits = 1;

        lineRend = GetComponent<LineRenderer>();
        lineRend.SetPosition(0, Vector3.zero);
    }

    /// <summary>
    /// Author: Ben Hoffman
    /// This will let me set my UI only when I have data
    /// </summary>
    /// <param name="myData"></param>
    public void SetData(Source myData)
    {
        computerSourceInfo = myData;

        UpdateUI();
    }

    /// <summary>
    /// Author: Ben Hoffman
    /// Purpose of method: To add the given computer to my
    /// list of connected PC's
    /// </summary>
    /// <param name="connectedToMe">the PC that is connected to me</param>
    public void AddConnectedPC(GameObject connectedToMe)
    {
        if(connectedToMe == null)
        {
            return;
        }

        // If I do not already know of this PC, and it's not myself...
        if (!connectedPCs.Contains(connectedToMe) && connectedToMe != gameObject)
        {
            numHits++;
            // Add the connection to my linked list
            connectedPCs.AddLast(connectedToMe);

            // Add the position to the line renderer    
            lineRend.SetPosition(1, transform.InverseTransformPoint(connectedToMe.transform.position));        
        }
    }

    /// <summary>
    /// Author: Ben Hoffman
    /// Purpose of method: To update the UI of this object on 
    /// start and when a new connection is added
    /// </summary>
    private void UpdateUI()
    {
        // Set all my UI data
        if(computerSourceInfo != null)
            UI.SetValues(computerSourceInfo);

        // Change the color of the  line renderer material based on the protocol
        switch (computerSourceInfo.transport)
        {
            case ("tcp"):
                // Light Gray color
                lineRend.material.color = Color.gray;
                break;
            case ("udp"):
                // Light blue
                lineRend.material.color = Color.blue;
                break;
            case ("http"):
                // Light green
                lineRend.material.color = Color.green;
                break;
            case ("https"):
                // Vibrant green
                lineRend.material.color = Color.green;              
                break;
        }
    }

}
