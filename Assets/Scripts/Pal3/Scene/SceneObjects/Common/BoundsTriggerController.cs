﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Scene.SceneObjects.Common
{
    using System;
    using Actor.Controllers;
    using Core.Extensions;
    using Core.Services;
    using GamePlay;
    using UnityEngine;

    public class BoundsTriggerController : MonoBehaviour
    {
        public event EventHandler<GameObject> OnPlayerActorEntered;
        public event EventHandler<GameObject> OnPlayerActorExited;

        private bool _hasCollided;
        private BoxCollider _collider;

        private PlayerActorManager _playerActorManager;

        public void SetBounds(Bounds bounds, bool isTrigger)
        {
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider>();
            }

            _collider.center = bounds.center;
            _collider.size = bounds.size;
            _collider.isTrigger = isTrigger;
        }

        private void OnEnable()
        {
            _playerActorManager = ServiceLocator.Instance.Get<PlayerActorManager>();
        }

        private void OnDisable()
        {
            if (_collider != null)
            {
                _collider.Destroy();
                _collider = null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<ActorController>() is {} actorController &&
                actorController.GetActor().Id == (int) _playerActorManager.GetPlayerActor())
            {
                OnPlayerActorEntered?.Invoke(this, collision.gameObject);
            }
        }

        private void OnTriggerEnter(Collider otherCollider)
        {
            if (otherCollider.gameObject.GetComponent<ActorController>() is {} actorController &&
                actorController.GetActor().Id == (int) _playerActorManager.GetPlayerActor())
            {
                OnPlayerActorEntered?.Invoke(this, otherCollider.gameObject);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.GetComponent<ActorController>() is {} actorController &&
                actorController.GetActor().Id == (int) _playerActorManager.GetPlayerActor())
            {
                OnPlayerActorExited?.Invoke(this, collision.gameObject);
            }
        }

        private void OnTriggerExit(Collider otherCollider)
        {
            if (otherCollider.gameObject.GetComponent<ActorController>() is {} actorController &&
                actorController.GetActor().Id == (int) _playerActorManager.GetPlayerActor())
            {
                OnPlayerActorExited?.Invoke(this, otherCollider.gameObject);
            }
        }
    }
}