﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using SimpleJSON;

public class ROSAvatarRotSubscriber : ROSBridgeSubscriber {

    #region PUBLIC_MEMBER_VARIABLES
    #endregion //PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES
    #endregion //PRIVATE_MEMBER_VARIABLES

    #region MONOBEHAVIOR_METHODS
    #endregion //MONOBEHAVIOR_METHODS

    #region PUBLIC_METHODS

    public new static string GetMessageTopic()
    {
        return "/user_avatar_default_owner/cmd_rot";
    }

    public new static string GetMessageType()
    {
        return "geometry_msgs/Quaternion";
    }

    public new static ROSBridgeMsg ParseMessage(JSONNode msg)
    {
        return new QuaternionMsg(msg);
    }

    public new static void CallBack(ROSBridgeMsg msg)
    {
        ROSBridge.Instance.ReceiveMessage((QuaternionMsg)msg);
    }

    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS

    #endregion //PRIVATE_METHODS
}
