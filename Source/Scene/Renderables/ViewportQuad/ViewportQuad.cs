﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class ViewportQuad : IDisposable
    {
        public ViewportQuad(Context context)
        {
            Verify.ThrowIfNull(context);

            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.DepthTest.Enabled = false;

            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.ViewportQuad.Shaders.ViewportQuadVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.ViewportQuad.Shaders.ViewportQuadFS.glsl"));

            _geometry = new ViewportQuadGeometry();
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            _geometry.Update(context, _sp);

            context.TextureUnits[0].Texture2D = Texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_geometry.VertexArray);
            context.Draw(PrimitiveType.TriangleStrip, sceneState);
        }

        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _geometry.Dispose();
        }

        #endregion

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly ViewportQuadGeometry _geometry;
    }
}