﻿using UnityEngine;
using System;
/// <summary>
/// Can be used to store key values of Unity's ConfigurableJoint
/// </summary>

[Serializable]
public class JointSettings
{
    public HumanBodyBones bone;
    public string jointName;

    // we do not want to save the foldout state in json
    [NonSerialized]
    public bool showInEditor = false;
    [NonSerialized]
    public bool showAngularXDriveInEditor = false;
    [NonSerialized]
    public bool showAngularYZDriveInEditor = false;
    [NonSerialized]
    public bool showAngularLimitsInEditor = false;

    public float angularXDriveSpring;
    public float angularXDriveDamper;
    public float maxForceX;

    public float angularYZDriveSpring;
    public float angularYZDriveDamper;
    public float maxForceYZ;

    public float angularLimitLowX;
    public float angularLimitHighX;
    public float angularLimitY;
    public float angularLimitZ;

    public Vector3 primaryAxis;
    public Vector3 secondaryAxis;

    //Rigidbody
    public bool gravity = true;
    public float mass = 0;
    public Vector3 centerOfMass = Vector3.zero;
    public Vector3 inertiaTensor = Vector3.one;

    [NonSerialized]
    public JointDrive angularXDrive;
    [NonSerialized]
    public JointDrive angularYZDrive;

    /// <summary>
    /// Used for global joint settings.
    /// </summary>
    /// <param name="bone"></param>
    /// <param name="angularXDriveSpring"></param>
    /// <param name="angularXDriveDamper"></param>
    /// <param name="maxForceX"></param>
    /// <param name="angularYZDriveSpring"></param>
    /// <param name="angularYZDriveDamper"></param>
    /// <param name="maxForceYZ"></param>
    public JointSettings(HumanBodyBones bone, float angularXDriveSpring, float angularXDriveDamper, float maxForceX, float angularYZDriveSpring, float angularYZDriveDamper, float maxForceYZ)
    {
        this.bone = bone;
        this.angularXDriveDamper = angularXDriveDamper;
        this.angularXDriveSpring = angularXDriveSpring;
        this.maxForceX = maxForceX;
        this.angularYZDriveDamper = angularYZDriveDamper;
        this.angularYZDriveSpring = angularYZDriveSpring;
        this.maxForceYZ = maxForceYZ;
    }

    /// <summary>
    /// A container for the most important ConfigurableJoint and Rigidbody parameters.
    /// </summary>
    /// <param name="bone">The HumanBodyBone of the body part of the joint.</param>
    /// <param name="angularXDrive"></param>
    /// <param name="angularYZDrive"></param>
    public JointSettings(HumanBodyBones bone, ConfigurableJoint joint)
    {
        this.bone = bone;
        jointName = bone.ToString();

        angularXDrive = joint.angularXDrive;
        angularXDriveDamper = joint.angularXDrive.positionDamper;
        angularXDriveSpring = joint.angularXDrive.positionSpring;
        maxForceX = joint.angularXDrive.maximumForce;

        angularYZDrive = joint.angularYZDrive;
        angularYZDriveDamper = joint.angularYZDrive.positionDamper;
        angularYZDriveSpring = joint.angularYZDrive.positionSpring;
        maxForceYZ = joint.angularYZDrive.maximumForce;

        angularLimitLowX = joint.lowAngularXLimit.limit;
        angularLimitHighX = joint.highAngularXLimit.limit;
        angularLimitY = joint.angularYLimit.limit;
        angularLimitZ = joint.angularZLimit.limit;

        primaryAxis = joint.axis;
        secondaryAxis = joint.secondaryAxis;

        Rigidbody rb = joint.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            gravity = rb.useGravity;
            mass = rb.mass;
            centerOfMass = rb.centerOfMass;
            inertiaTensor = rb.inertiaTensor;
        }

    }
    public JointSettings(JointSettings copy)
    {
        bone = copy.bone;
        angularXDrive = copy.angularXDrive;
        angularYZDrive = copy.angularYZDrive;
    }

    /// <summary>
    /// Creates just a blank JointSettings, use if values are unknown at point of creation.
    /// </summary>
    public JointSettings()
    {

    }
    /// <summary>
    /// Constructor for storing tuning results for the editor.
    /// </summary>
    /// <param name="individualJoint"></param>
    /// <param name="joint"></param>
    public JointSettings(string individualJoint, ConfigurableJoint joint)
    {
        this.bone = (HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), individualJoint.Remove(individualJoint.Length - 1));
        this.jointName = individualJoint;

        angularXDrive = joint.angularXDrive;
        angularXDriveDamper = joint.angularXDrive.positionDamper;
        angularXDriveSpring = joint.angularXDrive.positionSpring;
        maxForceX = joint.angularXDrive.maximumForce;

        angularYZDrive = joint.angularYZDrive;
        angularYZDriveDamper = joint.angularYZDrive.positionDamper;
        angularYZDriveSpring = joint.angularYZDrive.positionSpring;
        maxForceYZ = joint.angularYZDrive.maximumForce;

        angularLimitLowX = joint.lowAngularXLimit.limit;
        angularLimitHighX = joint.highAngularXLimit.limit;
        angularLimitY = joint.angularYLimit.limit;
        angularLimitZ = joint.angularZLimit.limit;

        primaryAxis = joint.axis;
        secondaryAxis = joint.secondaryAxis;

        Rigidbody rb = joint.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            gravity = rb.useGravity;
            mass = rb.mass;
            centerOfMass = rb.centerOfMass;
            inertiaTensor = rb.inertiaTensor;
        }
    }

    public static void SetSingleDrivesFromTuning(ConfigurableJoint joint, JointSettings[] jointSettingsTuning)
    {
        JointDrive drive = new JointDrive();
        JointDrive yzDrive = new JointDrive();
        float spring = 0, damper = 0, maxForce = 0;
        int numOfYZJointsTuning = 0;

        //SoftJointLimit limit = new SoftJointLimit();

        foreach (JointSettings settings in jointSettingsTuning)
        {
            //we have found the same primary axis, so the drives and angular limits are the same for the joint's x-axis
            if (settings.primaryAxis == joint.axis)
            {
                //x-drive
                drive.positionSpring = settings.angularXDriveSpring;
                drive.positionDamper = settings.angularXDriveDamper;
                drive.maximumForce = settings.maxForceX;
                joint.angularXDrive = drive;
                /*
                //low limit
                limit.limit = settings.angularLimitLowX;
                joint.lowAngularXLimit = limit;
                //high limit
                limit.limit = settings.angularLimitHighX;
                joint.highAngularXLimit = limit;
                */
            }
            else
            {
                //check whether drive from tuning has impact on the joint's yz drive
                if (settings.angularXDriveSpring != 0 && settings.maxForceX != 0)
                {
                    spring += settings.angularXDriveSpring;
                    damper += settings.angularXDriveDamper;
                    maxForce += settings.maxForceX;
                    numOfYZJointsTuning++;
                }
            }
        }


        //combined yz drive
        yzDrive.positionSpring = numOfYZJointsTuning != 0 ? spring / numOfYZJointsTuning : 0;
        yzDrive.positionDamper = numOfYZJointsTuning != 0 ? damper / numOfYZJointsTuning : 0;
        yzDrive.maximumForce = numOfYZJointsTuning != 0 ? maxForce / numOfYZJointsTuning : 0;
        joint.angularYZDrive = yzDrive;
    }


    #region Legacy
    public void SetAngularXDriveFromPD(float p, float d)
    {
        angularXDriveSpring = p;
        angularXDriveDamper = d;
    }

    public void SetAngularXDrive(JointDrive drive)
    {
        angularXDrive = drive;
        angularXDriveSpring = drive.positionSpring;
        angularXDriveDamper = drive.positionDamper;
        maxForceX = drive.maximumForce;
    }

    public void SetAngularYZDriveFromPD(float p, float d)
    {
        angularYZDriveSpring = p;
        angularYZDriveDamper = d;
    }

    public void SetAngularYZDrive(JointDrive drive)
    {
        angularYZDrive = drive;
        angularYZDriveSpring = drive.positionSpring;
        angularYZDriveDamper = drive.positionDamper;
        maxForceYZ = drive.maximumForce;
    }
    #endregion
}
