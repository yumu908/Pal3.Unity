// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.GameSystems.Minimap
{
    using Core.DataReader.Nav;
    using UnityEngine;

    public class MinimapTextureCreator
    {
        private readonly Color _obstacleColor;
        private readonly Color _wallColor;
        private readonly Color _floorColor;

        public MinimapTextureCreator(Color obstacleColor,
            Color wallColor,
            Color floorColor)
        {
            _obstacleColor = obstacleColor;
            _wallColor = wallColor;
            _floorColor = floorColor;
        }

        /// <summary>
        /// Creates a Texture2D for the given NavTileLayer, representing a minimap of the layer.
        /// </summary>
        /// <param name="layer">The NavTileLayer to create the minimap for.</param>
        /// <returns>A Texture2D representing the minimap of the NavTileLayer.</returns>
        public Texture2D CreateMinimapTexture(NavTileLayer layer)
        {
            Color[] colors = new Color[layer.Width * layer.Height];

            for (var i = 0; i < layer.Width; i++)
            {
                for (int j = 0; j < layer.Height; j++)
                {
                    var index = i + j * layer.Width;
                    NavTile tile = layer.Tiles[index];

                    // NOTE: the texture is flipped vertically compared to the tilemap space
                    var colorIndex = i + (layer.Height - j - 1) * layer.Width;
                    colors[colorIndex] = tile.DistanceToNearestObstacle switch
                    {
                        0 => _obstacleColor,
                        1 => _wallColor,
                        _ => _floorColor
                    };
                }
            }

            Texture2D texture = new Texture2D(layer.Width, layer.Height, TextureFormat.RGBA32, mipChain: false);
            texture.SetPixels(colors);
            texture.Apply(updateMipmaps: false);
            return texture;
        }
    }
}