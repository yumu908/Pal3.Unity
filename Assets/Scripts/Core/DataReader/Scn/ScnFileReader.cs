﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Core.DataReader.Scn
{
    using System;
    using System.IO;
    using Contracts;
    using GameBox;
    using UnityEngine;

    public sealed class ScnFileReader : IFileReader<ScnFile>
    {
        public ScnFile Read(IBinaryReader reader, int codepage)
        {
            var header = reader.ReadChars(4);
            var headerStr = new string(header[..^1]);

            if (headerStr != "SCN")
            {
                throw new InvalidDataException("Invalid SCN(.scn) file: header != SCN");
            }

            var version = reader.ReadInt16();
            var numberOfNpc = reader.ReadInt16();
            var npcDataOffset = reader.ReadUInt32();
            var numberOfObjects = reader.ReadInt16();
            var objectDataOffset = reader.ReadUInt32();

            var sceneInfo = new ScnSceneInfo()
            {
                CityName = reader.ReadString(32, codepage),
                SceneName = reader.ReadString(32, codepage),
                Model = reader.ReadString(32, codepage),
                SceneType = (SceneType) reader.ReadInt32(),
                LightMap = reader.ReadInt32(),
                SkyBox = reader.ReadUInt32(),
                Reserved = reader.ReadUInt32s(6),
            };

            reader.Seek(npcDataOffset, SeekOrigin.Begin);
            var npcInfos = new ScnNpcInfo[numberOfNpc];
            for (var i = 0; i < numberOfNpc; i++)
            {
                npcInfos[i] = ReadNpcInfo(reader, codepage);
            }

            reader.Seek(objectDataOffset, SeekOrigin.Begin);
            var objInfos = new ScnObjectInfo[numberOfObjects];
            for (var i = 0; i < numberOfObjects; i++)
            {
                ScnObjectInfo sceneObject = ReadObjectInfo(reader, codepage);
                objInfos[i] = sceneObject;
            }

            return new ScnFile(sceneInfo, npcInfos, objInfos);
        }

        private static ScnNpcInfo ReadNpcInfo(IBinaryReader reader, int codepage)
        {
            return new ScnNpcInfo()
            {
                Id = reader.ReadByte(),
                Type = (ActorType)reader.ReadByte(),
                Name = reader.ReadString(32, codepage),
                Texture = reader.ReadString(34, codepage),
                FacingDirection = reader.ReadSingle(),
                LayerIndex = reader.ReadInt32(),
                GameBoxXPosition = reader.ReadSingle(),
                GameBoxZPosition = reader.ReadSingle(),
                InitActive = reader.ReadInt32(),
                InitBehaviour = (ActorBehaviourType) reader.ReadInt32(),
                ScriptId = reader.ReadUInt32(),
                GameBoxYPosition = reader.ReadSingle(),
                InitAction = reader.ReadString(16, codepage),
                MonsterIds = reader.ReadUInt32s(3),
                NumberOfMonsters = reader.ReadByte(),
                MonsterCanRespawn = reader.ReadByte(),
                PaddingBytes = reader.ReadBytes(2), // Empty padding bytes to complete 4-byte alignment
                Path = new ScnPath()
                {
                    NumberOfWaypoints = reader.ReadInt32(),
                    GameBoxWaypoints = reader.ReadVector3s(16)
                },
                NoTurn = reader.ReadUInt32(),
                LoopAction = reader.ReadUInt32(),
                GameBoxMoveSpeed = reader.ReadUInt32(),
                Reserved = reader.ReadUInt32s(29),
            };
        }

        private static ScnObjectInfo ReadObjectInfo(IBinaryReader reader, int codepage)
        {
            var id = reader.ReadByte();
            var initActive = reader.ReadByte();
            var times = reader.ReadByte();
            var switchState = reader.ReadByte();

            var name = reader.ReadString(32, codepage);

            var triggerType = reader.ReadByte();
            var isNonBlocking = reader.ReadByte();
            _ = reader.ReadBytes(2); // Empty padding bytes to complete 4-byte alignment

            var gameBoxPosition = reader.ReadVector3();
            var yRotation = reader.ReadSingle();

            var tileMapTriggerRect = new GameBoxRect()
            {
                Left = reader.ReadInt32(),
                Top = reader.ReadInt32(),
                Right = reader.ReadInt32(),
                Bottom = reader.ReadInt32(),
            };

            var type = (SceneObjectType)reader.ReadByte();
            var saveState = reader.ReadByte();
            var layerIndex = reader.ReadByte();
            var elementType = (ObjectElementType)reader.ReadByte();

            #if PAL3
            var parameters = reader.ReadInt32s(6);
            #elif PAL3A
            var parameters = Array.ConvertAll(reader.ReadSingles(6), Convert.ToInt32);
            #endif

            #if PAL3A
            var notUsed = reader.ReadUInt32();
            #endif

            var requireSpecialAction = reader.ReadByte();
            var requireItem = reader.ReadUInt16();
            _ = reader.ReadByte(); // Empty padding byte to complete 4-byte alignment

            var requireMoney = reader.ReadUInt16();
            var requireLevel = reader.ReadUInt16();

            var requireAttackValue = reader.ReadUInt16();
            var requireAllMechanismsSolved = reader.ReadByte();
            var failedMessage = reader.ReadString(16, codepage);
            _ = reader.ReadByte(); // Empty padding byte to complete 4-byte alignment

            #if PAL3A
            var linkedObjectGroupId = reader.ReadUInt16();
            _ = reader.ReadBytes(2); // Empty padding bytes to complete 4-byte alignment
            #endif

            var scriptId = reader.ReadUInt32();

            var path = new ScnPath()
            {
                NumberOfWaypoints = reader.ReadInt32(),
                GameBoxWaypoints = reader.ReadVector3s(16)
            };

            var linkedObjectId = reader.ReadUInt16();
            var dependentSceneName = reader.ReadString(32, codepage);
            var dependentObjectId = reader.ReadByte();
            _ = reader.ReadByte(); // Empty padding byte to complete 4-byte alignment

            var bounds = new Bounds();
            bounds.SetMinMax(
                reader.ReadVector3().ToUnityPosition(),
                reader.ReadVector3().ToUnityPosition());

            var xRotation = reader.ReadSingle();

            #if PAL3A
            var zRotation = reader.ReadSingle();
            #endif

            var sfxName = reader.ReadString(8, codepage);

            var effectModelType = reader.ReadUInt32();

            var scriptActivated = reader.ReadUInt32();
            var scriptMoved = reader.ReadUInt32();

            #if PAL3A
            var canOnlyBeTriggeredOnce = reader.ReadUInt32();
            #endif

            #if PAL3
            var reserved = reader.ReadUInt32s(52);
            #elif PAL3A
            var reserved = reader.ReadUInt32s(44);
            #endif

            return new ScnObjectInfo()
            {
                Id = id,
                InitActive = initActive,
                Times = times,
                SwitchState = switchState,

                Name = name,

                TriggerType = triggerType,
                IsNonBlocking = isNonBlocking,

                GameBoxPosition = gameBoxPosition,
                GameBoxYRotation = yRotation,
                TileMapTriggerRect = tileMapTriggerRect,

                Type = type,
                SaveState = saveState,
                LayerIndex = layerIndex,
                ElementType = elementType,

                Parameters = parameters,

                #if PAL3A
                NotUsed = notUsed,
                #endif

                RequireSpecialAction = requireSpecialAction,
                RequireItem = requireItem,
                RequireMoney = requireMoney,
                RequireLevel = requireLevel,
                RequireAttackValue = requireAttackValue,
                RequireAllMechanismsSolved = requireAllMechanismsSolved,

                FailedMessage = failedMessage,

                #if PAL3A
                LinkedObjectGroupId = linkedObjectGroupId,
                #endif

                ScriptId = scriptId,

                Path = path,

                LinkedObjectId = linkedObjectId,
                DependentSceneName = dependentSceneName,
                DependentObjectId = dependentObjectId,

                Bounds = bounds,

                GameBoxXRotation = xRotation,

                #if PAL3A
                GameBoxZRotation = zRotation,
                #endif

                SfxName = sfxName,
                EffectModelType = effectModelType,

                ScriptActivated = scriptActivated,
                ScriptMoved = scriptMoved,

                #if PAL3A
                CanOnlyBeTriggeredOnce = canOnlyBeTriggeredOnce,
                #endif

                Reserved = reserved,
            };
        }
    }
}