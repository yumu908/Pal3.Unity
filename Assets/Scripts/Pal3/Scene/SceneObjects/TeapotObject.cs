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
    using Data;
    using UnityEngine;

    [ScnSceneObject(SceneObjectType.Teapot)]
    public sealed class TeapotObject : SceneObject
    {
        public TeapotObject(ScnObjectInfo objectInfo, ScnSceneInfo sceneInfo)
            : base(objectInfo, sceneInfo)
        {
        }

        public override GameObject Activate(GameResourceProvider resourceProvider, Color tintColor)
        {
            if (IsActivated) return GetGameObject();
            GameObject sceneGameObject = base.Activate(resourceProvider, tintColor);

            if (ObjectInfo.SwitchState == 1)
            {
                sceneGameObject.transform.rotation *= Quaternion.Euler(0, 0, -ObjectInfo.Parameters[2]);
            }

            return sceneGameObject;
        }

        public override bool IsDirectlyInteractable(float distance) => false;

        public override bool ShouldGoToCutsceneWhenInteractionStarted() => true;

        public override IEnumerator InteractAsync(InteractionContext ctx)
        {
            if (ObjectInfo.SwitchState == 1) yield break;

            FlipAndSaveSwitchState();

            PlaySfxIfAny();

            Transform objectTransform = GetGameObject().transform;
            Quaternion rotation = objectTransform.rotation;
            Quaternion targetRotation = rotation * Quaternion.Euler(0, 0, -ObjectInfo.Parameters[2]);

            yield return objectTransform.RotateAsync(targetRotation, 1f, AnimationCurveType.Sine);

            yield return ExecuteScriptAndWaitForFinishIfAnyAsync();
        }
    }
}

#endif