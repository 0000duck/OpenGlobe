﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class RayCastedGlobe : IDisposable
    {
        public RayCastedGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            _context = context;

            string vs = EmbeddedResources.GetText("MiniGlobe.Scene.Globes.RayCasted.Shaders.GlobeVS.glsl");
            string fs = EmbeddedResources.GetText("MiniGlobe.Scene.Globes.RayCasted.Shaders.GlobeFS.glsl");
            _sp = Device.CreateShaderProgram(vs, fs);
            _cameraEyeSquaredSP = _sp.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;

            _renderState = new RenderState();

            ///////////////////////////////////////////////////////////////////

            string solidFS = EmbeddedResources.GetText("MiniGlobe.Scene.Globes.RayCasted.Shaders.SolidShadedGlobeFS.glsl");
            _solidSP = Device.CreateShaderProgram(vs, solidFS);
            _cameraEyeSquaredSolidSP = _solidSP.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;

            ///////////////////////////////////////////////////////////////////

            Shape = Ellipsoid.UnitSphere;
            Shade = true;
            ShowGlobe = true;
        }

        private void Clean()
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                }

                Mesh mesh = BoxTessellator.Compute(2 * _shape.Radii);
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _renderState.FacetCulling.Face = CullFace.Front;
                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                (_sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();
                (_solidSP.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
                }
                _wireframe = new Wireframe(_context, mesh);
                _wireframe.FacetCullingFace = CullFace.Front;
                _wireframe.Width = 3;

                _dirty = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Clean();

            if (ShowGlobe || ShowWireframeBoundingBox)
            {
                _context.Bind(_va);
            }

            if (ShowGlobe)
            {
                Vector3D eye = sceneState.Camera.Eye;
                Vector3S cameraEyeSquared = eye.MultiplyComponents(eye).ToVector3S();

                if (Shade)
                {
                    _context.TextureUnits[0].Texture2D = Texture;
                    _context.Bind(_sp);
                    _cameraEyeSquaredSP.Value = cameraEyeSquared;
                }
                else
                {
                    _context.Bind(_solidSP);
                    _cameraEyeSquaredSolidSP.Value = cameraEyeSquared;
                }
                _context.Bind(_renderState);
                _context.Draw(_primitiveType, sceneState);
            }

            if (ShowWireframeBoundingBox)
            {
                _wireframe.Render(sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
        }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                _dirty = true;
                _shape = value;
            }
        }

        public bool Shade { get; set; }
        public bool ShowGlobe { get; set; }
        public bool ShowWireframeBoundingBox { get; set; }
        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _solidSP.Dispose();

            if (_va != null)
            {
                _va.Dispose();
            }

            if (_wireframe != null)
            {
                _wireframe.Dispose();
            }
        }

        #endregion

        private readonly Context _context;

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector3S> _cameraEyeSquaredSP;
        private readonly ShaderProgram _solidSP;
        private readonly Uniform<Vector3S> _cameraEyeSquaredSolidSP;

        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Wireframe _wireframe;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}