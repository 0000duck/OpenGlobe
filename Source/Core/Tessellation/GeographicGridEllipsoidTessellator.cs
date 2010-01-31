﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using System;
using System.Collections.Generic;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Core.Tessellation
{
    [Flags]
    public enum GeographicGridEllipsoidVertexAttributes
    {
        Position = 1,
        Normal = 2,
        TextureCoordinate = 4,
        All = Position | Normal | TextureCoordinate
    }

    public static class GeographicGridEllipsoidTessellator
    {
        public static Mesh Compute(Ellipsoid ellipsoid, int numberOfSlicePartitions, int numberOfStackPartitions, GeographicGridEllipsoidVertexAttributes vertexAttributes)
        {
            if (numberOfSlicePartitions < 3)
            {
                throw new ArgumentOutOfRangeException("numberOfSlicePartitions");
            }

            if (numberOfStackPartitions < 2)
            {
                throw new ArgumentOutOfRangeException("numberOfStackPartitions");
            }

            if ((vertexAttributes & GeographicGridEllipsoidVertexAttributes.Position) != GeographicGridEllipsoidVertexAttributes.Position)
            {
                throw new ArgumentException("Positions must be provided.", "vertexAttributes");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfVertices = NumberOfVertices(numberOfSlicePartitions, numberOfStackPartitions);
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfVertices);
            mesh.Attributes.Add(positionsAttribute);

            IndicesInt indices = new IndicesInt(3 * NumberOfTriangles(numberOfSlicePartitions, numberOfStackPartitions));
            mesh.Indices = indices;

            IList<Vector3h> normals = null;
            if ((vertexAttributes & GeographicGridEllipsoidVertexAttributes.Normal) == GeographicGridEllipsoidVertexAttributes.Normal)
            {
                VertexAttributeHalfFloatVector3 normalsAttribute = new VertexAttributeHalfFloatVector3("normal", numberOfVertices);
                mesh.Attributes.Add(normalsAttribute);
                normals = normalsAttribute.Values;
            }

            IList<Vector2h> textureCoordinates = null;
            if ((vertexAttributes & GeographicGridEllipsoidVertexAttributes.TextureCoordinate) == GeographicGridEllipsoidVertexAttributes.TextureCoordinate)
            {
                VertexAttributeHalfFloatVector2 textureCoordinateAttribute = new VertexAttributeHalfFloatVector2("textureCoordinate", numberOfVertices);
                mesh.Attributes.Add(textureCoordinateAttribute);
                textureCoordinates = textureCoordinateAttribute.Values;
            }

            IList<Vector3d> positions = positionsAttribute.Values;
            positions.Add(new Vector3d(0, 0, ellipsoid.Radii.Z));

            // TODO:  lookup table
            for (int i = 1; i < numberOfStackPartitions; ++i)
            {
                double phi = Math.PI * (((double)i) / numberOfStackPartitions);
                double cosPhi = Math.Cos(phi);
                double sinPhi = Math.Sin(phi);

                for (int j = 0; j < numberOfSlicePartitions; ++j)
                {
                    double theta = (2.0 * Math.PI) * (((double)j) / numberOfSlicePartitions);
                    double cosTheta = Math.Cos(theta);
                    double sinTheta = Math.Sin(theta);

                    positions.Add(new Vector3d(
                        ellipsoid.Radii.X * cosTheta * sinPhi,
                        ellipsoid.Radii.Y * sinTheta * sinPhi,
                        ellipsoid.Radii.Z * cosPhi));
                }
            }
            positions.Add(new Vector3d(0, 0, -ellipsoid.Radii.Z));

            if ((normals != null) || (textureCoordinates != null))
            {
                for (int i = 0; i < positions.Count; ++i)
                {
                    Vector3d deticSurfaceNormal = ellipsoid.DeticSurfaceNormal(positions[i]);

                    if (normals != null)
                    {
                        normals.Add(new Vector3h(deticSurfaceNormal));
                    }

                    if (textureCoordinates != null)
                    {
                        textureCoordinates.Add(SubdivisionUtility.ComputeTextureCoordinate(deticSurfaceNormal));
                    }
                }
            }

            //
            // Triangle fan top row
            //
            for (int j = 1; j < numberOfSlicePartitions; ++j)
            {
                indices.AddTriangle(new TriangleIndices<int>(0, j, j + 1));
            }
            indices.AddTriangle(new TriangleIndices<int>(0, numberOfSlicePartitions, 1));

            //
            // Middle rows are triangle strips
            //
            for (int i = 0; i < numberOfStackPartitions - 2; ++i)
            {
                int topRowOffset = (i * numberOfSlicePartitions) + 1;
                int bottomRowOffset = ((i + 1) * numberOfSlicePartitions) + 1;

                for (int j = 0; j < numberOfSlicePartitions - 1; ++j)
                {
                    indices.AddTriangle(new TriangleIndices<int>(bottomRowOffset + j, bottomRowOffset + j + 1, topRowOffset + j + 1));
                    indices.AddTriangle(new TriangleIndices<int>(bottomRowOffset + j, topRowOffset + j + 1, topRowOffset + j));
                }
                indices.AddTriangle(new TriangleIndices<int>(bottomRowOffset + numberOfSlicePartitions - 1, bottomRowOffset, topRowOffset));
                indices.AddTriangle(new TriangleIndices<int>(bottomRowOffset + numberOfSlicePartitions - 1, topRowOffset, topRowOffset + numberOfSlicePartitions - 1));
            }

            //
            // Triangle fan bottom row
            //
            int lastPosition = positions.Count - 1;
            for (int j = lastPosition - 1; j > lastPosition - numberOfSlicePartitions; --j)
            {
                indices.AddTriangle(new TriangleIndices<int>(lastPosition, j, j - 1));
            }
            indices.AddTriangle(new TriangleIndices<int>(lastPosition, lastPosition - numberOfSlicePartitions, lastPosition - 1));

            return mesh;
        }

        private static int NumberOfTriangles(int numberOfSlicePartitions, int numberOfStackPartitions)
        {
            int numberOfTriangles = 2 * numberOfSlicePartitions;                                // Top and bottom fans
            numberOfTriangles += 2 * ((numberOfStackPartitions - 2) * numberOfSlicePartitions); // Middle triangle strips
            return numberOfTriangles;
        }

        private static int NumberOfVertices(int numberOfSlicePartitions, int numberOfStackPartitions)
        {
            return 2 + ((numberOfStackPartitions - 1) * numberOfSlicePartitions);
        }
    }
}