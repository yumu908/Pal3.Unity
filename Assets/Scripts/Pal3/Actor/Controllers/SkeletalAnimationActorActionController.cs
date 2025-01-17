﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Actor.Controllers
{
    using System;
    using Command;
    using Command.SceCommands;
    using Core.DataLoader;
    using Core.DataReader.Cpk;
    using Core.DataReader.Mov;
    using Core.DataReader.Msh;
    using Core.DataReader.Mtl;
    using Core.Extensions;
    using Core.Utils;
    using Data;
    using Rendering.Material;
    using Rendering.Renderer;
    using Script.Waiter;
    using UnityEngine;

    public sealed class SkeletalAnimationActorActionController : ActorActionController,
        ICommandExecutor<ActorStopActionCommand>
    {
        private GameResourceProvider _resourceProvider;
        private IMaterialFactory _materialFactory;
        private ActorBase _actor;
        private Color _tintColor;

        private SkeletalModelRenderer _skeletalModelRenderer;

        private Bounds _rendererBounds;
        private Bounds _meshBounds;

        public void Init(GameResourceProvider resourceProvider,
            ActorBase actor,
            bool hasColliderAndRigidBody,
            bool isDropShadowEnabled,
            Color tintColor)
        {
            base.Init(resourceProvider, actor, hasColliderAndRigidBody, isDropShadowEnabled);

            _resourceProvider = resourceProvider;
            _actor = actor;
            _tintColor = tintColor;
            _materialFactory = resourceProvider.GetMaterialFactory();
        }

        private void OnEnable()
        {
            CommandExecutorRegistry<ICommand>.Instance.Register(this);
        }

        private void OnDisable()
        {
            CommandExecutorRegistry<ICommand>.Instance.UnRegister(this);
            DeActivate();
            base.DeActivate();
        }

        public override void PerformAction(string actionName,
            bool overwrite = false,
            int loopCount = -1,
            WaitUntilCanceled waiter = null)
        {
            if (!overwrite && string.Equals(GetCurrentAction(), actionName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!_actor.HasAction(actionName))
            {
                Debug.LogError($"[{nameof(SkeletalAnimationActorActionController)}] Action {actionName} not found for actor {_actor.Name}.");
                waiter?.CancelWait();
                return;
            }

            MshFile mshFile;
            MtlFile mtlFile;
            MovFile movFile;
            ITextureResourceProvider textureProvider;
            try
            {
                string mshFilePath = _actor.GetMeshFilePath(actionName);
                mshFile = _resourceProvider.GetGameResourceFile<MshFile>(mshFilePath);

                string mtlFilePath = _actor.GetMaterialFilePath(actionName);
                mtlFile = _resourceProvider.GetGameResourceFile<MtlFile>(mtlFilePath);

                string movFilePath = _actor.GetActionFilePath(actionName);
                movFile = _resourceProvider.GetGameResourceFile<MovFile>(movFilePath);

                textureProvider = _resourceProvider.CreateTextureResourceProvider(
                    Utility.GetDirectoryName(mtlFilePath, CpkConstants.DirectorySeparatorChar));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(SkeletalAnimationActorActionController)}] Exception: {ex}");
                waiter?.CancelWait();
                return;
            }

            DisposeCurrentAction();

            _skeletalModelRenderer = gameObject.GetOrAddComponent<SkeletalModelRenderer>();

            _skeletalModelRenderer.Init(mshFile,
                mtlFile,
                _materialFactory,
                textureProvider,
                _tintColor);

            _skeletalModelRenderer.StartAnimation(movFile, loopCount);

            _rendererBounds = _skeletalModelRenderer.GetRendererBounds();
            _meshBounds = _skeletalModelRenderer.GetMeshBounds();

            base.PerformAction(actionName, overwrite, loopCount, waiter);
        }

        public override void PauseAnimation()
        {
            if (_skeletalModelRenderer != null)
            {
                _skeletalModelRenderer.PauseAnimation();
            }
        }

        public override float GetActorHeight()
        {
            if (_skeletalModelRenderer == null || !_skeletalModelRenderer.IsVisible())
            {
                return _meshBounds.size.y;
            }

            return _skeletalModelRenderer.GetMeshBounds().size.y;
        }

        public override Bounds GetRendererBounds()
        {
            return (_skeletalModelRenderer == null || !_skeletalModelRenderer.IsVisible()) ? _rendererBounds :
                _skeletalModelRenderer.GetRendererBounds();
        }

        public override Bounds GetMeshBounds()
        {
            return (_skeletalModelRenderer == null || !_skeletalModelRenderer.IsVisible()) ? _meshBounds :
                _skeletalModelRenderer.GetMeshBounds();
        }

        internal override void DisposeCurrentAction()
        {
            if (_skeletalModelRenderer != null)
            {
                _skeletalModelRenderer.Dispose();
                _skeletalModelRenderer = null;
            }

            base.DisposeCurrentAction();
        }

        internal override void DeActivate()
        {
            DisposeCurrentAction();
            base.DeActivate();
        }

        public void Execute(ActorStopActionCommand command)
        {
            if (command.ActorId != _actor.Id ||
                _skeletalModelRenderer == null ||
                !_skeletalModelRenderer.IsVisible()) return;

            if (_skeletalModelRenderer.IsVisible())
            {
                PerformAction(_actor.GetIdleAction());
            }
        }
    }
}