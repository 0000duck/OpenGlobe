﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene.Terrain
{
    internal class ClipmapLevel
    {
        public RasterTerrainLevel Terrain;

        public Texture2D HeightTexture;
        public Texture2D NormalTexture;

        public Vector2I OriginInTextures = new Vector2I(0, 0);

        public bool OffsetStripOnNorth;
        public bool OffsetStripOnEast;

        public class Extent
        {
            public Extent()
            {
            }

            public Extent(int west, int south, int east, int north)
            {
                West = west;
                South = south;
                East = east;
                North = north;
            }

            public int West;
            public int South;
            public int East;
            public int North;
        }

        public Extent CurrentExtent = new Extent(1, 1, 0, 0);
        public Extent NextExtent = new Extent();

        public ClipmapLevel FinerLevel;
        public ClipmapLevel CoarserLevel;
    }
}
