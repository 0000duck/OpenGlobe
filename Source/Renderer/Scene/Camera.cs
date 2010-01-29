﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.IO;
using System.Xml;
using OpenTK;

namespace MiniGlobe.Renderer
{
    public class Camera
    {
        public Camera()
        {
            Eye = -Vector3d.UnitY;
            Target = Vector3d.Zero;
            Up = Vector3d.UnitZ;

            FieldOfViewY = Math.PI / 6.0;
            AspectRatio = 1;

            NearPlaneDistance = 0.01;
            FarPlaneDistance = 64;
        }

        public Vector3d Eye { get; set; }
        public Vector3d Target { get; set; }
        public Vector3d Up { get; set; }

        public double FieldOfViewX
        {
            get { return (2.0 * Math.Atan(AspectRatio * Math.Tan(FieldOfViewY * 0.5))); }
        }
        public double FieldOfViewY { get; set; }
        public double AspectRatio { get; set; }

        public double NearPlaneDistance { get; set; }
        public double FarPlaneDistance { get; set; }

        public void ZoomToTarget(double radius)
        {
            Vector3d toEye = Vector3d.Normalize(Eye - Target);

            double sin = Math.Sin(Math.Min(FieldOfViewX, FieldOfViewY) * 0.5);
            double distance = (radius / sin);
            Eye = Target + (distance * toEye);
        }

        public void SaveView(string filename)
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);

            XmlElement cameraNode = xmlDocument.CreateElement("Camera");
            xmlDocument.AppendChild(cameraNode);

            SaveVector(xmlDocument, "Eye", Eye, cameraNode);
            SaveVector(xmlDocument, "Target", Target, cameraNode);
            SaveVector(xmlDocument, "Up", Up, cameraNode);

            xmlDocument.Save(filename);
        }

        static private void SaveVector(XmlDocument xmlDocument, string name, Vector3d value, XmlElement parentNode)
        {
            XmlElement node = xmlDocument.CreateElement(name);
            node.AppendChild(xmlDocument.CreateTextNode(value.X + " " + value.Y + " " + value.Z));
            parentNode.AppendChild(node);
        }

        public void LoadView(string filename)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Camera");

            string[] eye = nodeList[0].ChildNodes[0].InnerText.Split(new[] { ' ' });
            Eye = new Vector3d(Convert.ToDouble(eye[0]), Convert.ToDouble(eye[1]), Convert.ToDouble(eye[2]));
            string[] target = nodeList[0].ChildNodes[1].InnerText.Split(new[] { ' ' });
            Target = new Vector3d(Convert.ToDouble(target[0]), Convert.ToDouble(target[1]), Convert.ToDouble(target[2]));
            string[] up = nodeList[0].ChildNodes[2].InnerText.Split(new[] { ' ' });
            Up = new Vector3d(Convert.ToDouble(up[0]), Convert.ToDouble(up[1]), Convert.ToDouble(up[2]));
        }
    }
}
