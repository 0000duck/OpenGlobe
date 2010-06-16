﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using System.Drawing;
using System.Diagnostics;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public class SceneState
    {
        public SceneState()
	    {
            DiffuseIntensity = 0.65f;
            SpecularIntensity = 0.25f;
            AmbientIntensity = 0.10f;
            Shininess = 12;
            Camera = new Camera();
            SunPosition = new Vector3D(200000, 0, 0);
            ModelMatrix = Matrix4d.Identity;
            HighResolutionSnapScale = 1;
        }

        public float DiffuseIntensity { get; set; }
        public float SpecularIntensity { get; set; }
        public float AmbientIntensity { get; set; }
        public float Shininess { get; set; }
        public Camera Camera { get; set; }

        public Vector3D SunPosition { get; set; }

        public Vector3D CameraLightPosition
        {
            get { return Camera.Eye; }
        }

        public Matrix4d ComputeViewportTransformationMatrix(Rectangle viewport)
        {
            double halfWidth = viewport.Width * 0.5;
            double halfHeight = viewport.Height * 0.5;
            double halfDepth = Camera.OrthographicDepth * 0.5;

            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return new Matrix4d(
                halfWidth, 0, 0, 0,
                0, halfHeight, 0, 0,
                0, 0, -halfDepth, 0,
                viewport.Left, viewport.Top, Camera.OrthographicNearPlaneDistance, 1);
        }

        public static Matrix4d ComputeViewportOrthographicProjectionMatrix(Rectangle viewport)
        {
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return Matrix4d.CreateOrthographicOffCenter(viewport.Left, viewport.Right, viewport.Top,
                viewport.Bottom, 0, 1);
        }

        public Matrix4d OrthographicProjectionMatrix
        {
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            get
            {
                return Matrix4d.CreateOrthographicOffCenter(Camera.OrthographicLeft, Camera.OrthographicRight, 
                    Camera.OrthographicTop, Camera.OrthographicBottom,
                    Camera.OrthographicNearPlaneDistance, Camera.OrthographicFarPlaneDistance);
            }
        }

        public Matrix4d PerspectiveProjectionMatrix 
        {
            get
            {
                //
                // Workaround because the following throws for zNear >= zFar
                //
                //return Matrix4d.CreatePerspectiveFieldOfView(Camera.FieldOfViewY, Camera.AspectRatio,
                //    Camera.PerspectiveNearPlaneDistance, Camera.PerspectiveFarPlaneDistance);

                double fovy = Camera.FieldOfViewY;
                double aspect = Camera.AspectRatio;
                double zNear = Camera.PerspectiveNearPlaneDistance;
                double zFar = Camera.PerspectiveFarPlaneDistance;

                Debug.Assert(fovy > 0 && fovy <= System.Math.PI);
                Debug.Assert(aspect > 0);
                Debug.Assert(zNear > 0);
                Debug.Assert(zFar > 0);

                double yMax = zNear * System.Math.Tan(0.5 * fovy);
                double yMin = -yMax;
                double xMin = yMin * aspect;
                double xMax = yMax * aspect;

                double left = xMin;
                double right = xMax;
                double bottom = yMin;
                double top = yMax;

                double x = (2.0 * zNear) / (right - left);
                double y = (2.0 * zNear) / (top - bottom);
                double a = (right + left) / (right - left);
                double b = (top + bottom) / (top - bottom);
                double c = -(zFar + zNear) / (zFar - zNear);
                double d = -(2.0 * zFar * zNear) / (zFar - zNear);

                return new Matrix4d(x, 0, 0, 0,
                                    0, y, 0, 0,
                                    a, b, c, -1,
                                    0, 0, d, 0);
            }
        }

        public Matrix4d ViewMatrix
        {
            get { return Matrix4d.LookAt(Camera.Eye.X, Camera.Eye.Y, Camera.Eye.Z, Camera.Target.X, Camera.Target.Y, Camera.Target.Z, Camera.Up.X, Camera.Up.Y, Camera.Up.Z); }
        }

        public Matrix4d ModelMatrix { get; set; }
        
        public Matrix4d ModelViewMatrix
        {
            get { return ModelMatrix * ViewMatrix; }
        }

        public Matrix4d ModelViewPerspectiveProjectionMatrix
        {
            get { return ModelViewMatrix * PerspectiveProjectionMatrix; }
        }

        public Matrix4d ModelViewOrthographicProjectionMatrix
        {
            get { return ModelViewMatrix * OrthographicProjectionMatrix; }
        }

        // TODO:  Should return matrix in double precision
        public Matrix42 ModelZToClipCoordinates
        {
            get
            {
                //
                // Bottom two rows of model-view-projection matrix
                //
                Matrix4d m = ModelViewPerspectiveProjectionMatrix;
                return new Matrix42(
                    (float)m.M13, (float)m.M23, (float)m.M33, (float)m.M43,
                    (float)m.M14, (float)m.M24, (float)m.M34, (float)m.M44);
            }
        }

        public double HighResolutionSnapScale { get; set; }
    }
}
