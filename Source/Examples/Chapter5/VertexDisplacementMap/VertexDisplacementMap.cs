﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter5
{
    sealed class VertexDisplacementMap : IDisposable
    {
        public VertexDisplacementMap()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 5:  Vertex Shader Displacement Mapping");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _window.Keyboard.KeyUp += OnKeyUp;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _sceneState.DiffuseIntensity = 0.9f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap(@"ps-e.lg.jpg"));
            _tile = new VertexDisplacementMapTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Size.X, terrainTile.Size.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Size.X * 0.5, terrainTile.Size.Y * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);
            
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\VertexDisplacementMap.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay(_window.Context);
            _hud.Color = Color.Black;
            UpdateHUD();
        }

        private static string TerrainNormalsToString(TerrainNormals normals)
        {
            switch(normals)
            {
                case TerrainNormals.None:
                    return "No lighting";
                case TerrainNormals.ThreeSamples:
                    return "Three Samples";
                case TerrainNormals.FourSamples:
                    return "Four Samples";
                case TerrainNormals.SobelFilter:
                    return "Sobel Filter";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Height Exaggeration: " + _tile.HeightExaggeration + " (up/down)\n";
            text += "Normals: " + TerrainNormalsToString(_tile.Normals) + " ('n' + left/right)";
                
            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.N)
            {
                _nKeyDown = true;
            }
            if ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down))
            {
                _tile.HeightExaggeration = Math.Max(1, _tile.HeightExaggeration + ((e.Key == KeyboardKey.Up) ? 1 : -1));
            }
            else if (_nKeyDown && ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right)))
            {
                _tile.Normals += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_tile.Normals < TerrainNormals.None)
                {
                    _tile.Normals = TerrainNormals.SobelFilter;
                }
                else if (_tile.Normals > TerrainNormals.SobelFilter)
                {
                    _tile.Normals = TerrainNormals.None;
                }
            }

            UpdateHUD();
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.N)
            {
                _nKeyDown = false;
            }
        }

        private void OnRenderFrame()
        {
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);

            _tile.Render(_sceneState);
            _hud.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _tile.Dispose();
            _hudFont.Dispose();
            _hud.Texture.Dispose();
            _hud.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (VertexDisplacementMap example = new VertexDisplacementMap())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly VertexDisplacementMapTerrainTile _tile;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private bool _nKeyDown;
    }
}