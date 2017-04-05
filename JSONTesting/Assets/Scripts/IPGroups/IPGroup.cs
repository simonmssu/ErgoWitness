﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This represents a group of IP address that have the same
/// first 3 numbers. I.E. 192.168.1.XXX, all IP's with "192.168.1"
/// would be in this group
/// </summary>
[RequireComponent(typeof(Light))]
public class IPGroup : MonoBehaviour {

    #region Fields
    private float lifeTime = 5f;
    private bool isSpecialTeam; // True if this IP is a blue team
    private float increasePerComputer = 0.1f;

    [SerializeField]  
    private float radius = 5f;
    private float startRadius;
    [SerializeField]
    private float minDistanceApart = 1f;
    [SerializeField]
    private float lightRangeScaler = 5f;    // How much larger is the light range then the 
    private float smoothing = 10f;      // How fast the light will transition
            
    private List<Computer> groupedComputers;

    private Material groupColor;        // The color of the group
    private int groupAddress;          // This is the IP address parsed into integers, with delimeters at the periods
    private string[] stringValues;      // Temp variable used to store an IP split at the '.'
    private int[] tempIntValues;        // Used for comparisons
    private Computer tempObj;         // Use to store a gameobject that I may need
    private int attemptCount;           // This is so that we can break out of finding a position if we try this many times

    private Vector3 temp;           // Store a temp positoin for calcuations
    private Collider[] neighbors;   // Store a temp array of colliders for calculations
    private Light myPointLight;
    private IEnumerator currentScalingRoutine;
    private Vector3 positionWithRadius;
    private bool isDying = false;
    
    #endregion


    #region Mutators

    public int GroupAddress { get { return groupAddress; } set { groupAddress = value; } }
    public Material GroupColor { get { return groupColor; } set { groupColor = value; } }
    public bool IsSpecialTeam { get { return isSpecialTeam; } set { isSpecialTeam = value; } }
    public bool IsDying { get { return isDying; } }

    #endregion

    /// <summary>
    /// Instantiate the list of grouped computers, 
    /// set the position of this
    /// </summary>
    private void Awake()
    {
        // Initialize the list
        groupedComputers = new List<Computer>();

        // Set the attempt count
        attemptCount = 0;
        myPointLight = GetComponent<Light>();
        myPointLight.range = radius * lightRangeScaler;

        startRadius = radius;

        isDying = false;
    }

    /// <summary>
    /// Using the given IP address we will add it to this group
    /// </summary>
    /// <param name="IpAddress"></param>
    public void AddToGroup(int IpAddress)
    {
        // If our dictionary contains this...
        if (DeviceManager.currentDeviceManager.CheckDictionary(IpAddress))
        {
            // Cache the object here
            tempObj = DeviceManager.ComputersDict[IpAddress];

            // Make sure that we know that this is a blue team object if we are blue team
            if(isSpecialTeam)
                tempObj.IsSpecialTeam = true;

            // Add it to our list
            groupedComputers.Add(tempObj);

            // Increaset the radius of the group
            radius += increasePerComputer;

            // Move the object to our spot
            MoveToGroupSpot(tempObj);

            // Set the object's material to the group color, and give it a reference to this as it's group
            tempObj.SetUpGroup(groupColor, this);

            // Increase the size of my light
            // if we are currently scalling the light, then stop
            if (currentScalingRoutine != null)
            {
                StopCoroutine(currentScalingRoutine);
            }

            // Start scaling with a new number! use Radis * 2 becuase it is a radius, and we want to create a sphere
            currentScalingRoutine = ScaleLightRange(radius * 2f, smoothing);

            //  Start the coroutine
            StartCoroutine(currentScalingRoutine);
        }
    }

    /// <summary>
    /// This method will move a gameobject to the group position
    /// </summary>
    private void MoveToGroupSpot(Computer thingToMove)
    {
        attemptCount++;

        // Make the this group the parent of the computer object
        thingToMove.transform.parent = gameObject.transform;

        // Calculate a random spot that is within a certain radius of our positon
        temp = transform.position + UnityEngine.Random.onUnitSphere * radius;

        // Check if I am colliding with any other groups
        neighbors = Physics.OverlapSphere(temp, minDistanceApart);

        // There is something colliding with us, recursively call this function
        if (neighbors.Length > 0 && attemptCount <= 3)
        {
            // Try again
            MoveToGroupSpot(thingToMove);
        }
        else
        {
            // Actually move the object to the position
            thingToMove.transform.position = temp;
        }
    }

    /// <summary>
    /// Smoothly lerp the radius of this object
    /// </summary>
    /// <param name="newRange">The desired radius of this</param>
    /// <returns></returns>
    private IEnumerator ScaleLightRange(float newRange, float smoothingAmount)
    {
        // While I am smaller then what I want to be
        while (myPointLight.range < newRange)
        {
            // Change the range value of this point ligtht
            myPointLight.range = Mathf.Lerp(myPointLight.range, newRange, smoothingAmount * Time.deltaTime);
            // Yield for the end of the frame without generating garbage
            yield return null;
        }
    }

    /// <summary>
    /// Remove a specific IP address from this group.
    /// If this group then has nothing in it, then destroy it.
    /// </summary>
    /// <param name="removeMe">The IP integer of the object that we want to remove</param>
    public void RemoveIp(int removeMe)
    {
        // Remove this computer object from this group
        groupedComputers.Remove(DeviceManager.ComputersDict[removeMe]);

        radius -= increasePerComputer;
        // If the radius is less then the starting radius, then set it back to the start
        if(radius <= startRadius)
        {
            // Set the radius to the original radius
            radius = startRadius;

            // Start scaling with a new number!
            currentScalingRoutine = ScaleLightRange(radius * 2f, Time.fixedDeltaTime);
            // Start the coroutine to scale the light range
            StartCoroutine(currentScalingRoutine);
        }
      
        // If we have nothing in our group and we are not already dying...
        if (groupedComputers.Count <= 0 && !isDying)
        {
            // Start the death cotoutine
            StartCoroutine(Die());
        }

    }

    /// <summary>
    /// This will shrink out light down, remove us from the group manager, and then
    /// destroy this gameobject
    /// </summary>
    /// <returns></returns>
    private IEnumerator Die()
    {
        // We are dying now
        isDying = true;

        // Remove us from the group manager
        IPGroupManager.currentIpGroups.RemoveGroup(groupAddress);

        // If we are currently scaling, then stop
        if (currentScalingRoutine != null)
        {
            StopCoroutine(currentScalingRoutine);
        }

        // Wait for out light to hit 0
        currentScalingRoutine = ScaleLightRange(0f, smoothing * 5f);

        // Wait until our light shrinks down       
        StartCoroutine(currentScalingRoutine);

        // Wait for the light to go away
        yield return new WaitForSeconds(lifeTime);
    
        // Destroy this object
        Destroy(gameObject);
    }

}
