﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

#if PAL3

namespace Pal3.Scene.SceneObjects
{
    using System.Collections;
    using Command;
    using Command.InternalCommands;
    using Common;
    using Core.Contracts;
    using Core.DataReader.Scn;
    using Core.Services;
    using Data;
    using UnityEngine;

    [ScnSceneObject(SceneObjectType.EyeBall)]
    public sealed class EyeBallObject : SceneObject,
        ICommandExecutor<PlayerActorTilePositionUpdatedNotification>
    {
        private readonly Tilemap _tilemap;

        public EyeBallObject(ScnObjectInfo objectInfo, ScnSceneInfo sceneInfo)
            : base(objectInfo, sceneInfo)
        {
            _tilemap = ServiceLocator.Instance.Get<SceneManager>().GetCurrentScene().GetTilemap();
        }

        public override GameObject Activate(GameResourceProvider resourceProvider,
            Color tintColor)
        {
            if (IsActivated) return GetGameObject();
            GameObject sceneGameObject = base.Activate(resourceProvider, tintColor);
            CommandExecutorRegistry<ICommand>.Instance.Register(this);
            return sceneGameObject;
        }

        public void Execute(PlayerActorTilePositionUpdatedNotification command)
        {
            GameObject eyeBallGameObject = GetGameObject();
            Vector3 actorPosition = _tilemap.GetWorldPosition(command.Position, command.LayerIndex);
            actorPosition.y += 3f; // 3f is about the height of the player actor
            eyeBallGameObject.transform.LookAt(actorPosition);
        }

        public override bool IsDirectlyInteractable(float distance) => false;

        public override bool ShouldGoToCutsceneWhenInteractionStarted() => true;

        public override IEnumerator InteractAsync(InteractionContext ctx)
        {
            yield break;
        }

        public override void Deactivate()
        {
            CommandExecutorRegistry<ICommand>.Instance.UnRegister(this);
            base.Deactivate();
        }
    }
}

#endif