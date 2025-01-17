﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

#if PAL3A

namespace Pal3.Scene.SceneObjects
{
    using System.Collections;
    using Common;
    using Core.Animation;
    using Core.Contracts;
    using Core.DataReader.Scn;
    using Core.Extensions;
    using Core.GameBox;
    using Data;
    using Rendering.Renderer;
    using UnityEngine;

    [ScnSceneObject(SceneObjectType.RoadElevatorOrBlocker)]
    public sealed class RoadElevatorOrBlockerObject : SceneObject
    {
        private StandingPlatformController _platformController;
        private SceneObjectMeshCollider _meshCollider;

        public RoadElevatorOrBlockerObject(ScnObjectInfo objectInfo, ScnSceneInfo sceneInfo)
            : base(objectInfo, sceneInfo)
        {
        }

        public override GameObject Activate(GameResourceProvider resourceProvider, Color tintColor)
        {
            if (IsActivated) return GetGameObject();
            GameObject sceneGameObject = base.Activate(resourceProvider, tintColor);

            Bounds bounds = GetMeshBounds();
            bounds.size += new Vector3(0.6f, 0.2f, 0.6f); // Extend the bounds a little bit

            // This is a blocker object
            if (ObjectInfo.Parameters[4] == 1 &&
                ObjectInfo.Parameters[5] == 1)
            {
                if (ObjectInfo.SwitchState == 1 && ModelType == SceneObjectModelType.CvdModel)
                {
                    CvdModelRenderer cvdModelRenderer = GetCvdModelRenderer();
                    cvdModelRenderer.SetCurrentTime(cvdModelRenderer.GetDefaultAnimationDuration());
                    _meshCollider = sceneGameObject.AddComponent<SceneObjectMeshCollider>();
                }
            }
            else // This is a road elevator object
            {
                // Add a standing platform controller so that the player can stand on the floor
                _platformController = sceneGameObject.AddComponent<StandingPlatformController>();
                _platformController.Init(bounds, ObjectInfo.LayerIndex);

                // Add a mesh collider to block the player from walking into the object
                _meshCollider = sceneGameObject.AddComponent<SceneObjectMeshCollider>();
                _meshCollider.Init(new Vector3(-0.3f, -0.8f, -0.3f));
            }

            return sceneGameObject;
        }

        public override bool IsDirectlyInteractable(float distance) => false;

        public override bool ShouldGoToCutsceneWhenInteractionStarted() => true;

        public override IEnumerator InteractAsync(InteractionContext ctx)
        {
            PlaySfxIfAny();

            GameObject floorObject = GetGameObject();

            // This is a blocker object
            if (ObjectInfo.Parameters[4] == 1 &&
                ObjectInfo.Parameters[5] == 1)
            {
                FlipAndSaveSwitchState();

                yield return GetCvdModelRenderer().PlayOneTimeAnimationAsync(true);

                if (ObjectInfo.Parameters[2] != 0)
                {
                    Transform transform = floorObject.transform;
                    float yOffset = ((float)ObjectInfo.Parameters[2]).ToUnityDistance();
                    Vector3 finalPosition = transform.position + new Vector3(0f, -yOffset, 0f);
                    yield return floorObject.transform.MoveAsync(finalPosition, 0.5f);
                    SaveCurrentPosition();
                }

                if (_meshCollider == null)
                {
                    _meshCollider = floorObject.AddComponent<SceneObjectMeshCollider>();
                }
            }
            else // This is an elevator floor object
            {
                Vector3 currentPosition = floorObject.transform.position;

                float lowerYPosition =
                    ((float)ObjectInfo.Parameters[0] * ObjectInfo.Parameters[4]).ToUnityYPosition();
                float upperYPosition =
                    ((float)ObjectInfo.Parameters[2] * ObjectInfo.Parameters[5]).ToUnityYPosition();

                if (SceneInfo.Is("m05", "3"))
                {
                    lowerYPosition = 0f;
                }

                float finalYPosition = lowerYPosition;

                if (Mathf.Abs(currentPosition.y - lowerYPosition) < Mathf.Abs(currentPosition.y - upperYPosition))
                {
                    finalYPosition = upperYPosition;
                }

                Vector3 toPosition = new Vector3(currentPosition.x, finalYPosition, currentPosition.z);

                var duration = 2f;

                if (SceneInfo.Is("m06", "2"))
                {
                    duration = 0.3f;
                }

                yield return floorObject.transform.MoveAsync(toPosition, duration);

                SaveCurrentPosition();
            }
        }

        public override void Deactivate()
        {
            if (_platformController != null)
            {
                _platformController.Destroy();
                _platformController = null;
            }

            if (_meshCollider != null)
            {
                _meshCollider.Destroy();
                _meshCollider = null;
            }

            base.Deactivate();
        }
    }
}

#endif