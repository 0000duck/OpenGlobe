﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainTile
    {
        public RasterTerrainTile(RasterTerrainSource terrainSource, RasterTerrainTileIdentifier identifier)
        {
            _terrainSource = terrainSource;
            _identifier = identifier;
        }

        public RasterTerrainSource Source
        {
            get { return _terrainSource; }
        }

        public RasterTerrainLevel Level
        {
            get { return _terrainSource.Levels[_identifier.Level]; }
        }

        public RasterTerrainTileIdentifier Identifier
        {
            get { return _identifier; }
        }

        public RasterTerrainTileStatus Status
        {
            get
            {
                if (_isLoading)
                {
                    return RasterTerrainTileStatus.Loading;
                }
                else if (_posts != null)
                {
                    return RasterTerrainTileStatus.Loaded;
                }
                else
                {
                    return RasterTerrainTileStatus.Unloaded;
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            internal set
            {
                _isLoading = value;
                UpdateActivation();
            }
        }

        public abstract RasterTerrainTile SouthwestChild { get; }
        public abstract RasterTerrainTile SoutheastChild { get; }
        public abstract RasterTerrainTile NorthwestChild { get; }
        public abstract RasterTerrainTile NortheastChild { get; }

        /// <summary>
        /// Gets the index in the overall level of the westernmost post in this tile.
        /// </summary>
        public abstract int West { get; }

        /// <summary>
        /// Gets the index in the overall level of the southernmost post in this tile.
        /// </summary>
        public abstract int South { get; }

        /// <summary>
        /// Gets the index in the overall level of the easternmost post in this tile.
        /// </summary>
        public abstract int East { get; }

        /// <summary>
        /// Gets the index in the overall level of the northernmost post in this tile.
        /// </summary>
        public abstract int North { get; }

        /// <summary>
        /// Loads the tile into system memory, reading it from disk or a network server as necessary.
        /// This method does not return until the tile is loaded.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Gets a subset of posts from the tile and copies them into destination array.  This method should
        /// only be called when <see cref="Status"/> is <see cref="RasterTerrainTileStatus.Loaded"/>.
        /// </summary>
        /// <param name="west">The westernmost post to copy.  0 is the westernmost post in the tile.</param>
        /// <param name="south">The southernmost post to copy.  0 is the southernmost post in the tile.</param>
        /// <param name="east">The easternmost post to copy.  0 is the westernmost post in the tile.</param>
        /// <param name="north">The northernmost post to copy.  0 is the southernmost post in the tile.</param>
        /// <param name="destination">The destination array to receive the post data.</param>
        /// <param name="startIndex">The index at which to begin writing; the index in <paramref name="destination"/> to write the southwesternmost requested post.</param>
        /// <param name="stride">The number of posts in a row of the destination array.</param>
        /// <exception cref="InvalidOperationException">
        /// The tile is not loaded.
        /// </exception>
        public void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride)
        {
            if (Posts == null)
            {
                throw new InvalidOperationException("Tile is not loaded.");
            }

            Level.GetPosts(West + west, South + south, West + east, South + north, destination, startIndex, stride);
        }

        protected float[] Posts
        {
            get { return _posts; }
            set
            {
                _posts = value;
                UpdateActivation();
            }
        }

        private void UpdateActivation()
        {
            if (_posts != null || _isLoading == true)
            {
                Level.Source.ActivateTile(this);
            }
            else
            {
                Level.Source.DeactivateTile(this);
            }
        }

        private RasterTerrainTileIdentifier _identifier;
        private RasterTerrainSource _terrainSource;
        private float[] _posts;
        private bool _isLoading;
    }
}
