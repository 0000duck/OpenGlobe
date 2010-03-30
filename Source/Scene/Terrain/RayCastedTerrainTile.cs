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
using System.Collections.Generic;

namespace MiniGlobe.Terrain
{
    public sealed class RayCastedTerrainTile : IDisposable
    {
        public RayCastedTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 boxExit;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
                      boxExit = position.xyz;
                  }";
            string fs =
                @"#version 150
                 
                  in vec3 boxExit;

                  out vec3 fragmentColor;

                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform vec3 mg_cameraEye;

                  uniform vec3 u_aabbLowerLeft;
                  uniform vec3 u_aabbUpperRight;

                  struct Intersection
                  {
                      bool Intersects;
                      vec3 IntersectionPoint;
                  };

                  bool PointInsideAxisAlignedBoundingBox(vec3 point, vec3 lowerLeft, vec3 upperRight)
                  {
                      return all(greaterThanEqual(point, lowerLeft)) && all(lessThanEqual(point, upperRight));
                  }

                  void Swap(inout float left, inout float right)
                  {
                      float temp = left;
                      left = right;
                      right = temp;
                  }

                  bool PlanePairTest(
                      float origin, 
                      float direction, 
                      float aabbLowerLeft, 
                      float aabbUpperRight,
                      inout float tNear,
                      inout float tFar)
                  {
                      if (direction == 0)
                      {
                          //
                          // Ray is parallel to planes
                          //
                          if (origin < aabbLowerLeft || origin > aabbUpperRight)
                          {
                              return false;
                          }
                      }
                      else
                      {
                          //
                          // Compute the intersection distances of the planes
                          //
                          float oneOverDirection = 1.0 / direction;
                          float t1 = (aabbLowerLeft - origin) * oneOverDirection;
                          float t2 = (aabbUpperRight - origin) * oneOverDirection;

                          //
                          // Make t1 intersection with nearest plane
                          //
                          if (t1 > t2)
                          {
                              Swap(t1, t2);
                          }

                          //
                          // Track largest tNear and smallest tFar
                          //
                          tNear = max(t1, tNear);
                          tFar = min(t2, tFar);

                          //
                          // Missed box
                          //
                          if (tNear > tFar)
                          {
                              return false;
                          }

                          //
                          // Box is behind ray
                          //
                          if (tFar < 0)
                          {
                              return false;
                          }
                      }

                      return true;
                  }

                  Intersection RayIntersectsAABB(vec3 origin, vec3 direction, vec3 aabbLowerLeft, vec3 aabbUpperRight)
                  {
                      //
                      // Implementation of http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
                      //

                      float tNear = -100000.0;    // TODO:  How to get float max?
                      float tFar = 100000.0;

                      if (PlanePairTest(origin.x, direction.x, aabbLowerLeft.x, aabbUpperRight.x, tNear, tFar) &&
                          PlanePairTest(origin.y, direction.y, aabbLowerLeft.y, aabbUpperRight.y, tNear, tFar) &&
                          PlanePairTest(origin.z, direction.z, aabbLowerLeft.z, aabbUpperRight.z, tNear, tFar))
                      {
                          return Intersection(true, origin + (tNear * direction));
                      }

                      return Intersection(false, vec3(0.0));
                  }

                  void main()
                  {
                      vec3 direction = boxExit - mg_cameraEye;
                      vec2 oneOverDirectionXY = vec2(1.0) / direction.xy;

                      vec3 boxEntry;
                      if (PointInsideAxisAlignedBoundingBox(mg_cameraEye, u_aabbLowerLeft, u_aabbUpperRight))
                      {
                          boxEntry = mg_cameraEye;
                      }
                      else
                      {
                          Intersection i = RayIntersectsAABB(mg_cameraEye, direction, u_aabbLowerLeft, u_aabbUpperRight);
                          boxEntry = i.IntersectionPoint;
                      }

                      vec3 texEntry = boxEntry;

int i = 0;
                      vec3 intersectionPoint;
                      bool foundIntersection = false;

                      //while (!foundIntersection && all(lessThan(texEntry.xy, boxExit.xy)))
                      while (!foundIntersection && all(lessThan(texEntry.xy, boxExit.xy - vec2(0.0001))))   // TODO: need delta?
                      {
                          vec2 floorTexEntry = floor(texEntry.xy);
                          float height = texture(mg_texture0, floorTexEntry).r;

                          vec2 delta = ((floorTexEntry + vec2(1.0)) - texEntry.xy) * oneOverDirectionXY;
                          vec3 texExit = texEntry + (min(delta.x, delta.y) * direction);

                          //
                          // Explicitly set to avoid roundoff error
                          //
                          if (delta.x < delta.y)
                          {
                              texExit.x = floorTexEntry.x + 1.0;
                          }
                          else
                          {
                              texExit.y = floorTexEntry.y + 1.0;
                          }

                          //
                          // Check for intersection
                          //
                          if (direction.z >= 0.0)
                          {
                              if (texEntry.z <= height)
                              {
                                  foundIntersection = true;
                                  intersectionPoint = texEntry;
                              }
                          }
                          else
                          {
                              if (texExit.z <= height)
                              {
                                  foundIntersection = true;
                                  intersectionPoint = texEntry + (max((height - texEntry.z) / direction.z, 0.0) * direction);
                              }
                          }

                          texEntry = texExit;

if (i++ == 100)
{
fragmentColor = vec3(1.0, 1.0, 1.0);
return;
}
                      }


                      if (foundIntersection)
                      {
                          fragmentColor = vec3(intersectionPoint.z / 0.5, 0.0, 0.0);
                          // TODO:  set z
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            Vector3D radii = new Vector3D(
                tile.Extent.East - tile.Extent.West,
                tile.Extent.North - tile.Extent.South,
                tile.MaximumHeight - tile.MinimumHeight);
            Vector3D halfRadii = 0.5 * radii;

            (_sp.Uniforms["u_aabbLowerLeft"] as Uniform<Vector3S>).Value = Vector3S.Zero;
            (_sp.Uniforms["u_aabbUpperRight"] as Uniform<Vector3S>).Value = radii.ToVector3S();

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = BoxTessellator.Compute(radii);

            //
            // Translate box so it is not centered at the origin -
            // world space and texel space will match up.
            // TODO:  We don't always want this!
            //
            IList<Vector3D> positions = (mesh.Attributes["position"] as VertexAttributeDoubleVector3).Values;
            for (int i = 0; i < positions.Count; ++i)
            {
                positions[i] = positions[i] + halfRadii;
            }

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.Width, tile.Size.Height, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;
        }

        public void Render(SceneState sceneState)
        {
            _context.TextureUnits[0].Texture2DRectangle = _texture;
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
        private readonly RenderState _renderState;
    }
}