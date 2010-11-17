﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ShaderObjectGL3x : Disposable
    {
        public ShaderObjectGL3x(ShaderType shaderType, string source)
        {
            string builtinConstants =
                "#version 330 \n" +

                "#define og_positionVertexLocation          " + VertexLocations.Position.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_normalVertexLocation            " + VertexLocations.Normal.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_textureCoordinateVertexLocation " + VertexLocations.TextureCoordinate.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_colorVertexLocation             " + VertexLocations.Color.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
               
                "const float og_E =                " + Math.E.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_pi =               " + Math.PI.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_oneOverPi =        " + Trig.OneOverPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverTwo =        " + Trig.PiOverTwo.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverThree =      " + Trig.PiOverThree.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverFour =       " + Trig.PiOverFour.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverSix =        " + Trig.PiOverSix.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_threePiOver2 =     " + Trig.ThreePiOver2.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_twoPi =            " + Trig.TwoPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_oneOverTwoPi =     " + Trig.OneOverTwoPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_halfPi =           " + Trig.HalfPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_radiansPerDegree = " + Trig.RadiansPerDegree.ToString(NumberFormatInfo.InvariantInfo) + "; \n";

            string builtinFunctions = EmbeddedResources.GetText("OpenGlobe.Renderer.GL3x.GLSL.BuiltinFunctions.glsl");

            string modifiedSource;

            //
            // This requires that #version be the first line in the shader.  This
            // doesn't follow the spec exactly, which allows whitespace and
            // comments to come beforehand.
            //
            if (source.StartsWith("#version", StringComparison.InvariantCulture))
            {
                Debug.Assert(source.StartsWith("#version 330", StringComparison.InvariantCulture));
                modifiedSource = "//" + source;
            }
            else
            {
                modifiedSource = source;
            }

            string[] sources = new[] { builtinConstants, builtinFunctions, modifiedSource };
            int[] lengths = new[] { builtinConstants.Length, builtinFunctions.Length, modifiedSource.Length };

            _shaderObject = GL.CreateShader(shaderType);
            unsafe
            {
                fixed (int *lengthPointer = lengths)
                {
                    GL.ShaderSource(_shaderObject, sources.Length, sources, lengthPointer);
                }
            }
            GL.CompileShader(_shaderObject);

            int compileStatus;
            GL.GetShader(_shaderObject, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus == 0)
            {
                Console.WriteLine(sources[0]);
                Console.WriteLine(sources[1]);
                throw new CouldNotCreateVideoCardResourceException("Could not compile shader object.  Compile Log:  \n\n" + CompileLog);
            }
        }

        ~ShaderObjectGL3x()
        {
            FinalizerThreadContextGL3x.RunFinalizer(Dispose);
        }

        public int Handle
        {
            get { return _shaderObject; }
        }

        public string CompileLog
        {
            get { return GL.GetShaderInfoLog(_shaderObject); }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            // Always delete the shader, even in the finalizer.
            if (_shaderObject != 0)
            {
                GL.DeleteShader(_shaderObject);
                _shaderObject = 0;
            }
            base.Dispose(disposing);
        }

        #endregion

        private int _shaderObject;
    }
}
