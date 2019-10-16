﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PIDTuning;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    /// <summary>
    /// Wraps the PidConfiguration class into a MonoBehaviour so we
    /// can draw a fancy editor for it.
    /// </summary>
    public class PidConfigurationStorage : MonoBehaviour
    {
        public PidConfiguration Configuration { get; private set; }

        [SerializeField]
        private RigAngleTracker _userAvatar;

        [SerializeField]
        private UserAvatarService _userAvatarService;

        private void Start()
        {
            Assert.IsNotNull(_userAvatar);
            Assert.IsNotNull(_userAvatarService);

            Configuration = new PidConfiguration(DateTime.UtcNow);

            Configuration.InitializeMapping(_userAvatar.GetJointToRadianMapping().Keys, PidParameters.FromParallelForm(1000f, 100f, 500f));
        }

        /// <summary>
        /// Transmit the given PID configuration to the simulation. If no config is provided as
        /// an argument, the current value of the Configuration property of this component
        /// will be transmitted.
        /// </summary>
        public void TransmitPidConfiguration(PidConfiguration config = null)
        {
            if (null == config)
            {
                config = Configuration;
            }

            Assert.IsNotNull(config);

            Assert.IsTrue(_userAvatarService.IsRemoteAvatarPresent, "Cannot transmit PID config when remote avatar is not present. Did you forget to spawn it?");

            foreach (var joint in config.Mapping)
            {
                string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + joint.Key + "/set_pid_params";

                // default was (100f, 50f, 10f)
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(joint.Value.Kp, joint.Value.Ki, joint.Value.Kd));
            }
        }

        public void ReplaceWithConfigFromJson(string json)
        {
            var newConfig = PidConfiguration.FromJson(json);

            if (!newConfig.Mapping.Keys.OrderBy(s => s).SequenceEqual(Configuration.Mapping.Keys.OrderBy(s => s)))
            {
                throw new ArgumentException("Joint mappings are not compatible.");
            }

            Configuration = newConfig;
        }

        public void ResetConfiguration(float kp, float ki, float kd)
        {
            // We need to enumerate the key collection here since lazy evaluation would fail during the call to
            // InitializeMapping, since it modifies the underlying collection
            var joinNames = Configuration.Mapping.Keys.ToArray();

            Configuration.InitializeMapping(joinNames, PidParameters.FromParallelForm(kp, ki, kd));
        }
    }
}