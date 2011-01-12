﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class ClipmapTerrainOnGlobe : IDisposable
    {
        public ClipmapTerrainOnGlobe()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 11:  Clipmap Terrain");

            //_ellipsoid = Ellipsoid.ScaledWgs84;
            _ellipsoid = Ellipsoid.Wgs84;

            //SimpleTerrainSource terrainSource = new SimpleTerrainSource(@"..\..\..\..\..\..\Data\Terrain\ps_height_16k");
            WorldWindTerrainSource terrainSource = new WorldWindTerrainSource();
            _clipmap = new GlobeClipmapTerrain(_window.Context, terrainSource, _ellipsoid, 511);
            _clipmap.HeightExaggeration = 1.0f; // (float)(1.0 / Ellipsoid.Wgs84.MaximumRadius);

            _sceneState = new SceneState();
            _sceneState.DiffuseIntensity = 0.90f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;
            _sceneState.Camera.FieldOfViewY = Math.PI / 3.0;

            _clearState = new ClearState();
            _clearState.Color = Color.LightSkyBlue;
            //_clearState.Color = Color.White;

            _sceneState.Camera.PerspectiveNearPlaneDistance = 0.000001 * _ellipsoid.MaximumRadius;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 10.0 * _ellipsoid.MaximumRadius;
            _sceneState.SunPosition = new Vector3D(200000, 300000, 200000) * _ellipsoid.MaximumRadius;

             _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _ellipsoid);
             _camera.Range = 1.5 * _ellipsoid.MaximumRadius;

             _globe = new RayCastedGlobe(_window.Context);
             _globe.Shape = _ellipsoid;
             Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
             _globe.Texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

             _clearDepth = new ClearState();
             _clearDepth.Buffers = ClearBuffers.DepthBuffer | ClearBuffers.StencilBuffer;

            _window.Keyboard.KeyDown += OnKeyDown;

            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.PreRenderFrame += OnPreRenderFrame;

            PersistentView.Execute(@"C:\Users\Kevin Ring\Documents\Book\svn\GeometryClipmapping\Figures\ClipmapLevelsNearPole.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"C:\Users\Kevin Ring\Documents\Book\svn\GeometryClipmapping\Figures\ClipmapLevelsNearPole.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay();
            _hud.Color = Color.Blue;
            UpdateHUD();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.S)
            {
                _sceneState.SunPosition = _sceneState.Camera.Eye;
            }
            else if (e.Key == KeyboardKey.W)
            {
                _clipmap.Wireframe = !_clipmap.Wireframe;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.B)
            {
                if (!_clipmap.BlendRegionsEnabled)
                {
                    _clipmap.BlendRegionsEnabled = true;
                    _clipmap.ShowBlendRegions = false;
                }
                else if (_clipmap.ShowBlendRegions)
                {
                    _clipmap.BlendRegionsEnabled = false;
                }
                else
                {
                    _clipmap.ShowBlendRegions = true;
                }
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.L)
            {
                _clipmap.LodUpdateEnabled = !_clipmap.LodUpdateEnabled;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.C)
            {
                _clipmap.ColorClipmapLevels = !_clipmap.ColorClipmapLevels;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.Z)
            {
                double longitude = -119.5326056;
                double latitude = 37.74451389;
                Geodetic3D halfDome = new Geodetic3D(Trig.ToRadians(longitude), Trig.ToRadians(latitude), 2700.0);
                _camera.ViewPoint(_ellipsoid, halfDome);
                _camera.Azimuth = 0.0;
                _camera.Elevation = Trig.ToRadians(30.0);
                _camera.Range = 10000.0;
                _camera.RotateRateRangeAdjustment = 0.0;
                _camera.RotateFactor *= 1000.0;
            }
            else if (e.Key == KeyboardKey.F)
            {
                _camera.Dispose();
                CameraFly fly = new CameraFly(_sceneState.Camera, _window);
                //fly.MovementRate *= 10;
            }
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);

            _globe.Render(context, _sceneState);

            context.Clear(_clearDepth);

            _clipmap.Render(context, _sceneState);

            if (_hud != null)
            {
                _hud.Render(context, _sceneState);
            }
        }

        private void OnPreRenderFrame()
        {
            Context context = _window.Context;
            _clipmap.PreRender(context, _sceneState);
        }

        private void UpdateHUD()
        {
            if (_hud == null)
                return;

            string text;

            text = "Blending: " + GetBlendingString() + " (B)\n";
            text += "Wireframe: " + (_clipmap.Wireframe ? "Enabled" : "Disabled") + " (W)\n";
            text += "LOD Update: " + (_clipmap.LodUpdateEnabled ? "Enabled" : "Disabled") + " (L)\n";
            text += "Color Clipmap Levels: " + (_clipmap.ColorClipmapLevels ? "Enabled" : "Disabled") + " (C)\n";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private string GetBlendingString()
        {
            if (!_clipmap.BlendRegionsEnabled)
                return "Disabled";
            else if (_clipmap.ShowBlendRegions)
                return "Enabled and Shown";
            else
                return "Enabled";
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_camera != null)
                _camera.Dispose();
            //if (_cameraFly != null)
            //    _cameraFly.Dispose();
            _clipmap.Dispose();
            if (_hudFont != null)
                _hudFont.Dispose();
            if (_hud != null)
            {
                _hud.Texture.Dispose();
                _hud.Dispose();
            }
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (ClipmapTerrainOnGlobe example = new ClipmapTerrainOnGlobe())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        //private CameraFly _cameraFly;
        private readonly ClearState _clearState;
        private readonly GlobeClipmapTerrain _clipmap;
        private HeadsUpDisplay _hud;
        private Font _hudFont;
        private RayCastedGlobe _globe;
        private ClearState _clearDepth;
        private Ellipsoid _ellipsoid;
    }
}