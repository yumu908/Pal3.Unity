﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Scene.SceneObjects
{
    using System.Collections;
    using Command;
    using Command.InternalCommands;
    using Common;
    using Core.Contracts;
    using Core.DataReader.Scn;
    using Data;
    using UnityEngine;

    [ScnSceneObject(SceneObjectType.SceneSfx)]
    public sealed class SceneSfxObject : SceneObject
    {
        public string SfxName { get; }

        private const string SCENE_SFX_AUDIO_SOURCE_NAME = nameof(SceneSfxObject);
        private const float SCENE_SFX_VOLUME = 0.4f;

        public SceneSfxObject(ScnObjectInfo objectInfo, ScnSceneInfo sceneInfo)
            : base(objectInfo, sceneInfo, hasModel: false)
        {
            SfxName = objectInfo.Name;
        }

        public override GameObject Activate(GameResourceProvider gameResourceProvider, Color tintColor)
        {
            if (IsActivated) return GetGameObject();

            GameObject sceneGameObject = base.Activate(gameResourceProvider, tintColor);

            // We want some random delay before playing the scene sfx
            // since there might be more than one audio source in the current scene
            // playing the exact same audio sfx, which could potentially cause unwanted
            // "Comb filter" effect.
            float startDelay = Random.Range(0f, 1f);

            float interval = ObjectInfo.Parameters[0] > 0 ? ObjectInfo.Parameters[0] / 1000f : 0f;

            CommandDispatcher<ICommand>.Instance.Dispatch(
                new AttachSfxToGameObjectRequest(sceneGameObject,
                    SfxName,
                    SCENE_SFX_AUDIO_SOURCE_NAME,
                    loopCount: -1,
                    SCENE_SFX_VOLUME,
                    startDelay,
                    interval));

            return sceneGameObject;
        }

        public override bool IsDirectlyInteractable(float distance) => false;

        public override bool ShouldGoToCutsceneWhenInteractionStarted() => false;

        public override IEnumerator InteractAsync(InteractionContext ctx)
        {
            yield break;
        }
    }
}