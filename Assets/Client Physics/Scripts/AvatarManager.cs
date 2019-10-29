﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarManager : MonoBehaviour {

    public bool usePIDController = true;
    public float PDKp = 1;
    public float PDKd = 1;
    public float Kp = 8;
    public float Ki = 0;
    public float Kd = .05f;

    Animator animatorAvatar;
    Animator animatorTarget;

    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneAvatar = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneTarget = new Dictionary<HumanBodyBones, GameObject>();

    // Use this for initialization
    void Start () {
        animatorAvatar = GetComponentInChildren<Animator>();
        animatorTarget = GameObject.FindGameObjectWithTag("Target").GetComponent<Animator>();
        InitializeBodyStructures();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //UpdatePDControllers();
        //UpdateVacuumBreatherPIDControllers();
        //UpdateJoints();
        //UpdateMerchVRPIDControllers();


	}
    /// <summary>
    ///     Maps all HumanBodyBones (assigned in the Avatar) to their GameObjects in the scene in order to get access to all components.
    ///     Adds Rigidbody to both bodies, adds PDController to the avatar.
    /// </summary>
    void InitializeBodyStructures()
    {
        foreach(HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone)
            {
                Transform boneTransformAvatar = animatorAvatar.GetBoneTransform(bone);
                Transform boneTransformTarget = animatorTarget.GetBoneTransform(bone);
                //We have to skip unassigned bodyparts.
                if (boneTransformAvatar != null && boneTransformTarget != null)
                {
                    //build Dictionaries
                    gameObjectPerBoneAvatar.Add(bone, boneTransformAvatar.gameObject);
                    gameObjectPerBoneTarget.Add(bone, boneTransformTarget.gameObject);

                    AssignRigidbodys(bone);
                    SetupJoints();
                    //AssignVacuumBreatherPIDController(bone);
                    AssignPDController(bone);
                    //BodyMass bm = new BodyMass(73, this);
                    //AssignMerchVRPIDController(bone);
                }
            }
        }
    }
    /// <summary>
    ///     A method to return the Rigidbody of the GameObject that corresponds to a certain bodypart. 
    ///     Use this to gain access to the velocity of the bodypart.
    /// </summary>
    Rigidbody GetRigidbodyFromBone(bool fromAvatar, HumanBodyBones boneID)
    {
        GameObject obj;
        if((fromAvatar ? gameObjectPerBoneAvatar : gameObjectPerBoneTarget).TryGetValue(boneID, out obj))
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if(rb != null)
            {
                return rb;
            } 
            else
            {
                Debug.Log("No rigidbody is assigned to the bone " + boneID + "\nMake sure to run AvatarManager.Initialize first.");
                return null;
            }
        }
        else
        {
            Debug.Log("No object is assigned to the bone " + boneID);
            return null;
        }
    }

    void AssignRigidbodys(HumanBodyBones bone)
    {
        gameObjectPerBoneAvatar[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneAvatar[bone].GetComponent<Rigidbody>().useGravity = false;

        gameObjectPerBoneTarget[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().useGravity = false;
    }

    void AssignMerchVRPIDController(HumanBodyBones bone)
    {
        gameObjectPerBoneAvatar[bone].AddComponent<PIDControllerCombined>();
        gameObjectPerBoneAvatar[bone].GetComponent<PIDControllerCombined>().pidPosition = new PIDControllerPos(gameObjectPerBoneAvatar[bone], gameObjectPerBoneAvatar[bone], 38, 5, 8, new Vector3(0, 1, 0));
        gameObjectPerBoneAvatar[bone].GetComponent<PIDControllerCombined>().pidRotation = new PIDControllerRot(gameObjectPerBoneAvatar[bone], 50, 5 ,10);
        gameObjectPerBoneAvatar[bone].GetComponent<PIDControllerCombined>().pidVelocity = new PIDControllerVel(gameObjectPerBoneAvatar[bone], 35, 0, 0.6f, new Vector3(1, 0, 1), 100);
    }

    void AssignPDController(HumanBodyBones bone)
    {
        gameObjectPerBoneAvatar[bone].AddComponent<PDController>();
        gameObjectPerBoneAvatar[bone].GetComponent<PDController>().rigidbody = gameObjectPerBoneAvatar[bone].GetComponent<Rigidbody>();
        gameObjectPerBoneAvatar[bone].GetComponent<PDController>().oldVelocity = gameObjectPerBoneAvatar[bone].GetComponent<PDController>().rigidbody.velocity;
        gameObjectPerBoneAvatar[bone].GetComponent<PDController>().proportionalGain = PDKp;
        gameObjectPerBoneAvatar[bone].GetComponent<PDController>().derivativeGain = PDKd;
    }    
    void AssignVacuumBreatherPIDController(HumanBodyBones bone)
    {
        gameObjectPerBoneAvatar[bone].AddComponent<VacuumBreather.ControlledObject>();
        gameObjectPerBoneAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Kp = Kp;
        gameObjectPerBoneAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Ki = Ki;
        gameObjectPerBoneAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Kd = Kd;
    }


    void AssignJoint(HumanBodyBones bone)
    {
        if (gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>() == null)
        {
            gameObjectPerBoneAvatar[bone].AddComponent<ConfigurableJoint>();
            gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>().anchor = gameObjectPerBoneAvatar[bone].transform.position;
        }
    }

    /// <summary>
    ///     Since we use rigidbodies, the child objects are moving independently from their parent objects. 
    ///     We have to link them together by using Joints to avoid limbs getting seperated.
    /// </summary>
    void SetupJoints()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneAvatar.Keys)
        {
            AssignJoint(bone);

                switch (bone)
                {
                    case HumanBodyBones.LeftUpperArm:
                        gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectPerBoneAvatar[HumanBodyBones.LeftShoulder].GetComponent<Rigidbody>();
                        break;
                    case HumanBodyBones.LeftLowerArm:
                        gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectPerBoneAvatar[HumanBodyBones.LeftUpperArm].GetComponent<Rigidbody>();
                        break;
                    case HumanBodyBones.LeftHand:
                        gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectPerBoneAvatar[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>();
                        break;
                    default: break;
                }
            }
        
    }

    void UpdatePDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneAvatar.Keys)
        {
            Rigidbody targetRb = GetRigidbodyFromBone(false, bone);
            if(targetRb != null)
            {
                gameObjectPerBoneAvatar[bone].GetComponent<PDController>().SetDestination(gameObjectPerBoneTarget[bone].transform, targetRb.velocity);
            }
        }
    }    
    
    void UpdateVacuumBreatherPIDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneAvatar.Keys)
        {
            gameObjectPerBoneAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().DesiredOrientation = gameObjectPerBoneTarget[bone].transform.rotation;
        }
    }

    void UpdateMerchVRPIDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneAvatar.Keys)
        {
            gameObjectPerBoneAvatar[bone].GetComponent<PIDControllerCombined>().pidPosition.UpdateTarget(gameObjectPerBoneTarget[bone].transform.position);
            gameObjectPerBoneAvatar[bone].GetComponent<PIDControllerCombined>().pidRotation.UpdateTarget(gameObjectPerBoneTarget[bone].transform.rotation);
            gameObjectPerBoneAvatar[bone].GetComponent<PIDControllerCombined>().pidVelocity.UpdateTarget(gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().velocity, 1);
        }
    }

    void UpdateJoints()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneAvatar.Keys)
        {
            GameObject tmp;
            if(gameObjectPerBoneAvatar.TryGetValue(bone, out tmp))
            {
                if(gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>() != null)
                {
                    gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>().targetPosition = gameObjectPerBoneTarget[bone].transform.position;
                    gameObjectPerBoneAvatar[bone].GetComponent<ConfigurableJoint>().targetRotation = gameObjectPerBoneTarget[bone].transform.rotation;
                }
            }
        }
    }
    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneAvatarDictionary()
    {
        return gameObjectPerBoneAvatar;
    }    
    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneTargetDictionary()
    {
        return gameObjectPerBoneTarget;
    }
}
