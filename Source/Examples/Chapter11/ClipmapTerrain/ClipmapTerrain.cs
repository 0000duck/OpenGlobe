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

using OpenGlobe.Renderer;
using OpenGlobe.Scene;

using OpenGlobe.Core;
using OpenGlobe.Terrain;
using OpenGlobe.Scene.Terrain;

namespace OpenGlobe.Examples
{
    sealed class ClipmapTerrain : IDisposable
    {
        public ClipmapTerrain()
        {
            _window = Device.CreateWindow(640, 480, "Chapter 11:  Clipmap Terrain");

            SimpleTerrainSource terrainSource = new SimpleTerrainSource(@"..\..\..\..\..\..\Data\Terrain\ps_height_16k");
            _clipmap = new PlaneClipmapTerrain(_window.Context, terrainSource, 255);
            _clipmap.HeightExaggeration = 0.01f;

            _sceneState = new SceneState();
            _sceneState.DiffuseIntensity = 0.90f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;
            _sceneState.Camera.FieldOfViewY = Math.PI / 3.0;

            _clearState = new ClearState();
            _clearState.Color = Color.LightSkyBlue;

            Ellipsoid ellipsoid = Ellipsoid.Wgs84;
            _sceneState.Camera.PerspectiveNearPlaneDistance = 0.1;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 20000.0;
            _sceneState.SunPosition = new Vector3D(200000, 300000, 200000);

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(0.0, 0.0, _clipmap.HeightExaggeration * 2700.0);
            _camera.ZoomRateRangeAdjustment = 0.0;
            _camera.Azimuth = 0.0;
            _camera.Elevation = Trig.ToRadians(30.0);
            _camera.Range = 0.1;

            _camera.Dispose();
            _sceneState.Camera.Eye = new Vector3D(0.0, 0.0, _clipmap.HeightExaggeration * 2700.0);
            _sceneState.Camera.Target = _sceneState.Camera.Eye + Vector3D.UnitZ;
            _cameraFly = new CameraFly(_sceneState.Camera, _window);
            _cameraFly.UpdateParametersFromCamera();
            _cameraFly.MovementRate = 100.0;

            _window.Keyboard.KeyDown += OnKeyDown;

            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.PreRenderFrame += OnPreRenderFrame;

            PersistentView.Execute(@"C:\Users\Kevin Ring\Documents\Book\svn\TerrainLevelOfDetail\Figures\HalfDome.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"C:\Users\Kevin Ring\Documents\Book\svn\TerrainLevelOfDetail\Figures\HalfDome.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
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
            }
            else if (e.Key == KeyboardKey.B)
            {
                _clipmap.ShowBlendRegions = !_clipmap.ShowBlendRegions;
            }
            else if (e.Key == KeyboardKey.L)
            {
                _update = !_update;
            }
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);
            _clipmap.Render(context, _sceneState);
        }

        private void OnPreRenderFrame()
        {
            if (_update)
            {
                Context context = _window.Context;
                _clipmap.PreRender(context, _sceneState);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_camera != null)
                _camera.Dispose();
            if (_cameraFly != null)
                _cameraFly.Dispose();
            _clipmap.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (ClipmapTerrain example = new ClipmapTerrain())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly CameraFly _cameraFly;
        private readonly ClearState _clearState;
        private readonly PlaneClipmapTerrain _clipmap;
        private bool _update = true;
    }
}