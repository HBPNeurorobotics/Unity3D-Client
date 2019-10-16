﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class AnimatorControl : MonoBehaviour
    {
        [SerializeField] private GameObject _localAvatar;

        [SerializeField] private float _timeStretchFactor = 1f;

        private Animator _animator = null;
        private UserAvatarVisualsIKControl _ikControl = null;

        public bool IsAnimationRunning
        {
            get { return 1f > _animator.GetCurrentAnimatorStateInfo(0).normalizedTime; }
        }

        public float TimeStretchFactor
        {
            get { return _timeStretchFactor; }
        }

        /// <summary>
        /// This code runs in Awake (instead of Start/OnEnable) since it is a dependency
        /// of multiple other components.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(_localAvatar);

            _animator = _localAvatar.GetComponent<Animator>();
            _ikControl = _localAvatar.GetComponent<UserAvatarVisualsIKControl>();

            Assert.IsNotNull(_animator);
            Assert.IsNotNull(_ikControl);

            PrepareRigForPlayback();

            Debug.LogWarning("The PIDTuningService GameObject is enabled. This will disable tracking and IK functionality.");
        }

        public void ResetUserAvatar()
        {
            PrepareRigForPlayback();
            _animator.Play("idle");
        }

        public IEnumerator RunAnimationPlayback(string state)
        {
            PrepareRigForPlayback();
            _animator.Play(state);

            yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName(state));
        }

        /// <summary>
        /// Validates that all animation states exist given the current animator
        /// </summary>
        public void ValidateStateList(List<string> states)
        {
            Assert.IsNotNull(_animator);

            foreach (var state in states)
            {
                Assert.IsTrue(_animator.HasState(0, Animator.StringToHash(state)));
            }
        }

        private void PrepareRigForPlayback()
        {
            _animator.enabled = true;

            // Disable IK in test environment
            _ikControl.ikActive = false;

            // Apply time stretch to account for low simulations speeds
            _animator.speed = 1f / _timeStretchFactor;
        }
    }
}