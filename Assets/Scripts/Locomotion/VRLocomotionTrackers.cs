﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRLocomotionTrackers : Singleton<VRLocomotionTrackers>
{
    [SerializeField] Transform _leftFootTracker;
    public Transform LeftFootTracker { get { return _leftFootTracker; } }
    [SerializeField] Transform _rightFootTracker;
    public Transform RightFootTracker { get { return _rightFootTracker; } }
    [SerializeField] Transform _hipTracker;
    public Transform HipTracker { get { return _hipTracker; } }
    Vector3 _trackingPlane;
    public float DistanceTrackersOnPlane { get { return getDistanceBetweenTrackerOnPlane(_trackingPlane); }  }

    private void Update()
    {
        _trackingPlane = createTrackingPlaneBetweenTrackers();
        Debug.DrawRay(Vector3.zero, _trackingPlane);
    }

    public float getDistanceBetweenTrackerOnPlane(Vector3 trackingPlane)
    {
        Vector3 left = Vector3.ProjectOnPlane(LeftFootTracker.position, trackingPlane);
        Vector3 rigth = Vector3.ProjectOnPlane(RightFootTracker.position, trackingPlane);
        return Vector3.Distance(rigth, left);
    }

    public Vector3 createTrackingPlaneBetweenTrackers()
    {
        Vector3 directionRightToLeft = LeftFootTracker.position - RightFootTracker.position;
        return directionRightToLeft.normalized;
    }
}