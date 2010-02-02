﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

//#define FBO

using System;
using System.Drawing;

using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;

namespace MiniGlobe.Examples.Chapter3.RayCasting
{
    sealed class RayCasting : IDisposable
    {
        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Middle)
            {
                _sceneState.Camera.SaveView(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.xml");
            }
        }

        public RayCasting()
        {
            Ellipsoid globeShape = Ellipsoid.UnitSphere;

            _window = Device.CreateWindow(800, 600, "Chapter 3:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Mouse.ButtonDown += MouseDown;
            _sceneState = new SceneState();
            _camera = new CameraGlobeCentered(_sceneState.Camera, _window, globeShape);

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 worldPosition;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 
                      worldPosition = position.xyz;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  out vec4 fragColor;

                  uniform vec4 mg_DiffuseSpecularAmbientShininess;
                  uniform sampler2D mg_Texture0;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;

                  uniform vec3 mg_LightPosition;
                  uniform vec3 mg_CameraEye;
                  uniform vec3 u_GlobeCenter;
                  uniform float u_GlobeRadius;

                  struct Intersection
                  {
                      bool  Intersects;
                      float Time;         // Along ray
                  };

                  Intersection rayIntersectSphere(vec3 rayOrigin, vec3 rayDirection, vec3 sphereCenter, float sphereRadius)
                  {
                      vec3 l = sphereCenter - rayOrigin;

                      float s = dot(l, rayDirection);
                      float lSquaredDistance  = dot(l, l);
                      float radiusSquared = sphereRadius * sphereRadius;

                      if (s < 0 && lSquaredDistance > radiusSquared)
                      {
                          return Intersection(false, 0);
                      }

                      float mSquared = lSquaredDistance - (s * s);
                      if (mSquared > radiusSquared)
                      {
                          return Intersection(false, 0);
                      }

                      float q = sqrt(radiusSquared - mSquared);
                      if (lSquaredDistance > radiusSquared)
                      {
                          return Intersection(true, s - q);
                      }
                      else
                      {
                          return Intersection(true, s + q);
                      }
                  }

                  float lightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, mg_DiffuseSpecularAmbientShininess.w);

                      return (mg_DiffuseSpecularAmbientShininess.x * diffuse) +
                             (mg_DiffuseSpecularAmbientShininess.y * specular) +
                              mg_DiffuseSpecularAmbientShininess.z;
                  }

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan2(normal.y, normal.x) / mg_TwoPi + 0.5, asin(normal.z) / mg_Pi + 0.5);
                  }

                  float ComputeWorldPositionDepth(vec3 position)
                  {
                      // Consider using mat4x2
                      vec4 v = mg_ModelViewPerspectiveProjectionMatrix * vec4(position, 1);   // clip coordinates
                      v.z /= v.w;                                                             // normalized device coordinates
                      v.z = (v.z + 1.0) * 0.5;
                      return v.z;
                  }

                  void main()
                  {
                      vec3 rayDirection = normalize(worldPosition - mg_CameraEye);
                      Intersection i = rayIntersectSphere(mg_CameraEye, rayDirection, u_GlobeCenter, u_GlobeRadius);
                      if (i.Intersects)
                      {
                          vec3 intersectionPosition = mg_CameraEye + (i.Time * rayDirection);
                          vec3 intersectionNormal = normalize(intersectionPosition);

                          vec3 toLight = normalize(mg_LightPosition - intersectionPosition);
                          vec3 toEye = normalize(mg_CameraEye - intersectionPosition);
                          float intensity = lightIntensity(intersectionNormal, toLight, toEye, mg_DiffuseSpecularAmbientShininess);

                          fragColor = vec4(intensity * texture2D(mg_Texture0, ComputeTextureCoordinates(intersectionNormal)).rgb, 1.0);
                          gl_FragDepth = ComputeWorldPositionDepth(intersectionPosition);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            (_sp.Uniforms["u_GlobeCenter"] as Uniform<Vector3>).Value = Vector3.Zero;
            (_sp.Uniforms["u_GlobeRadius"] as Uniform<float>).Value = (float)globeShape.Radii.X;   // TODO

            Mesh mesh = BoxTessellator.Compute(2 * globeShape.Radii);
            _va = _window.Context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            ///////////////////////////////////////////////////////////////////

            string vs2 =
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 
                  }";

            string fs2 =
                @"#version 150
                 
                  out vec4 fragColor;

                  void main()
                  {
                      fragColor = vec4(0.0, 0.0, 0.0, 1.0);
                  }";
            _boxSP = Device.CreateShaderProgram(vs2, fs2);

            _boxRenderState = new RenderState();
            _boxRenderState.RasterizationMode = RasterizationMode.Line;
            _boxRenderState.FacetCulling.Face = CullFace.Front;
            _boxRenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            ///////////////////////////////////////////////////////////////////

            _sceneState.Camera.ZoomToTarget(Math.Max(Math.Max(globeShape.Radii.X, globeShape.Radii.Y), globeShape.Radii.Z));
            //_sceneState.Camera.LoadView(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.xml");
        }

        public void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        public void OnRenderFrame()
        {
            Context context = _window.Context;

#if FBO
            HighResolutionSnapFrameBuffer snapBuffer = new HighResolutionSnapFrameBuffer(context, 3, 600, _sceneState.Camera.AspectRatio);
            _window.Context.Viewport = new Rectangle(0, 0, snapBuffer.WidthInPixels, snapBuffer.HeightInPixels);
            context.Bind(snapBuffer.FrameBuffer);
#endif

            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            context.Bind(_va);

            //
            // Ray Casting Pass
            //
            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Draw(_primitiveType, _sceneState);

            //
            // Wireframe Box Pass
            //
            context.Bind(_boxRenderState);
            context.Bind(_boxSP);
            context.Draw(_primitiveType, _sceneState);

#if FBO
            snapBuffer.SaveColorBuffer(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.png");
            //snapBuffer.SaveDepthBuffer(@"c:\depth.tif");
            Environment.Exit(0);
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            _boxSP.Dispose();
            _texture.Dispose();
            _va.Dispose();
            _sp.Dispose();
            _camera.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (RayCasting example = new RayCasting())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraGlobeCentered _camera;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;

        private readonly RenderState _boxRenderState;
        private readonly ShaderProgram _boxSP;
    }
}