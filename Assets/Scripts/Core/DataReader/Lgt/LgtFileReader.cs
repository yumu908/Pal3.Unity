﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Core.DataReader.Lgt
{
    using System;
    using System.Collections.Generic;
    using GameBox;
    using Utils;

    public sealed class LgtFileReader : IFileReader<LgtFile>
    {
        public LgtFile Read(IBinaryReader reader)
        {
            var numOfLightNodes = reader.ReadInt32();
            var lightNodes = new List<LightNode>();

            for (var i = 0; i < numOfLightNodes; i++)
            {
                var lightNode = ReadLightNode(reader);

                if (Enum.IsDefined(typeof(GameBoxLightType), lightNode.LightType) &&
                    Enum.IsDefined(typeof(GameBoxLightShapeType), lightNode.LightShapeType))
                {
                    lightNodes.Add(lightNode);
                }
            }

            return new LgtFile(lightNodes.ToArray());
        }

        private static LightNode ReadLightNode(IBinaryReader reader)
        {
            var transformMatrix = new GameBoxMatrix4X4()
            {
                Xx = reader.ReadSingle(), Xy = reader.ReadSingle(), Xz = reader.ReadSingle(), Xw = reader.ReadSingle(),
                Yx = reader.ReadSingle(), Yy = reader.ReadSingle(), Yz = reader.ReadSingle(), Yw = reader.ReadSingle(),
                Zx = reader.ReadSingle(), Zy = reader.ReadSingle(), Zz = reader.ReadSingle(), Zw = reader.ReadSingle(),
                Tx = reader.ReadSingle(), Ty = reader.ReadSingle(), Tz = reader.ReadSingle(), Tw = reader.ReadSingle()
            };
            transformMatrix.Tw = 1f;

            var lightNode = new LightNode
            {
                WorldMatrix = GameBoxInterpreter.ToUnityMatrix4x4(transformMatrix),
                LightType = (GameBoxLightType)reader.ReadInt32(),
                LightColor = Utility.ToColor(reader.ReadSingles(4)),
                AmbientColor = Utility.ToColor(reader.ReadSingles(4)),
                UseDiffuse = reader.ReadBoolean(),
                UseSpecular = reader.ReadBoolean(),
                NearStart = reader.ReadSingle(),
                NearEnd = reader.ReadSingle(),
                FarStart = reader.ReadSingle(),
                FarEnd = reader.ReadSingle(),
                LightDecayType = (GameBoxLightDecayType)reader.ReadInt32(),
                DecayRadius = reader.ReadSingle(),
                LightShapeType = (GameBoxLightShapeType)reader.ReadInt32(),
                Size = reader.ReadSingle(),
                Falloff = reader.ReadSingle(),
                AspectRatio = reader.ReadSingle()
            };

            return lightNode;
        }
    }
}