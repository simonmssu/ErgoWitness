﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OnEnable, invoke the destroy method after the 
/// field of lifetime. On destroy set as in-active.
/// OnDisable, cancel any invokes on this object
/// </summary>
[RequireComponent(typeof(PooledObject))]
public class NetflowDestroyAfterTime : MonoBehaviour {

    public float lifetime = 5f; // How long this object will stay active
    PooledObject pooledObject;

    void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    /// <summary>
    /// Invoke the Destroy method after the specified lifetime
    /// </summary>
    void OnEnable()
    {
        // Destroy after 2 seconds
        Invoke("Destroy", lifetime);
    }

    /// <summary>
    /// Set as inactive in the hierachy, so that it can
    /// be pooled.
    /// </summary>
    public void Destroy()
    {
        pooledObject.SetPooledInActive();
        // Set this object to in-active
        //gameObject.SetActive(false);
    }

    /// <summary>
    /// Cancel any invokes that are currently happenign
    /// </summary>
    void OnDisable()
    {
        // Cancel any invokes we may have
        CancelInvoke();
    }

}