﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    public sealed class TextureAtlas : IDisposable
    {
        public TextureAtlas(IEnumerable<Bitmap> bitmaps)
            : this(bitmaps, 1)
        {
        }

        public TextureAtlas(IEnumerable<Bitmap> bitmaps, int borderWidthInPixels)
        {
            if (bitmaps == null)
            {
                throw new ArgumentNullException("bitmaps");
            }

            int numberOfBitmaps = CollectionAlgorithms.EnumerableCount(bitmaps);

            if (numberOfBitmaps == 0)
            {
                throw new ArgumentException("bitmaps does not contain any items.", "bitmaps");
            }

            List<AnnotatedBitmap> annotatedBitmaps = new List<AnnotatedBitmap>(numberOfBitmaps);

            PixelFormat pixelFormat = PixelFormat.Undefined;

            int j = 0;
            foreach (Bitmap b in bitmaps)
            {
                if (b == null)
                {
                    throw new ArgumentNullException("bitmaps", "An item in bitmaps is null.");
                }

                if (pixelFormat == PixelFormat.Undefined)
                {
                    pixelFormat = b.PixelFormat;
                }
                else if (b.PixelFormat != pixelFormat)
                {
                    throw new ArgumentException("All bitmaps must have the same PixelFormat.", "bitmaps");
                }

                annotatedBitmaps.Add(new AnnotatedBitmap(b, j++));
            }

            if (pixelFormat == PixelFormat.Undefined)
            {
                throw new ArgumentException("All bitmaps have PixelFormat.Undefined.", "bitmaps");
            }

            if (borderWidthInPixels < 0)
            {
                throw new ArgumentOutOfRangeException("borderWidthInPixels");
            }

            ///////////////////////////////////////////////////////////////////

            annotatedBitmaps.Sort(new BitmapMaximumToMinimumHeight());

            IList<Point> offsets = new List<Point>(annotatedBitmaps.Count);
            int width = ComputeAtlasWidth(annotatedBitmaps, borderWidthInPixels);
            int xOffset = 0;
            int yOffset = 0;
            int rowHeight = 0;

            //
            // TODO:  Pack more tightly based on algorithm in
            //
            //     http://www-ui.is.s.u-tokyo.ac.jp/~takeo/papers/i3dg2001.pdf
            //
            for (int i = 0; i < numberOfBitmaps; ++i)
            {
                Bitmap b = annotatedBitmaps[i].Bitmap;

                int widthIncrement = b.Width + borderWidthInPixels;

                if (xOffset + widthIncrement > width)
                {
                    xOffset = 0;
                    yOffset += rowHeight + borderWidthInPixels;
                }

                if (xOffset == 0)
                {
                    //
                    // The first bitmap of the row determines the row height.
                    // This is worst case since bitmaps are sorted by height.
                    //
                    rowHeight = b.Height;
                }

                offsets.Add(new Point(xOffset, yOffset));
                xOffset += widthIncrement;
            }
            int height = yOffset + rowHeight;

            ///////////////////////////////////////////////////////////////////

            RectangleH[] textureCoordinates = new RectangleH[annotatedBitmaps.Count];
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            Graphics graphics = Graphics.FromImage(bitmap);

            double widthD = width;
            double heightD = height;

            for (int i = 0; i < numberOfBitmaps; ++i)
            {
                Point upperLeft = offsets[i];
                AnnotatedBitmap b = annotatedBitmaps[i];

                textureCoordinates[b.Index] = new RectangleH(
                    new Vector2H(                                                       // Lower Left
                        (double)upperLeft.X / widthD,
                        (heightD - (double)(upperLeft.Y + b.Bitmap.Height)) / heightD),
                    new Vector2H(                                                       // Upper Right
                        (double)(upperLeft.X + b.Bitmap.Width) / widthD,
                        (heightD - (double)upperLeft.Y) / heightD));

                graphics.DrawImageUnscaled(b.Bitmap, upperLeft);
            }
            graphics.Dispose();

            _bitmap = bitmap;
            _textureCoordinates = new TextureCoordinateCollection(textureCoordinates);
            _borderWidth = borderWidthInPixels;
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }

        public TextureCoordinateCollection TextureCoordinates 
        { 
            get { return _textureCoordinates; } 
        }

        public int BorderWidth 
        { 
            get { return _borderWidth; }
        }

        private static int ComputeAtlasWidth(IList<AnnotatedBitmap> bitmaps, int borderWidthInPixels)
        {
            int maxWidth = 0;
            int area = 0;
            for (int i = 0; i < bitmaps.Count; ++i)
            {
                Bitmap b = bitmaps[i].Bitmap;
                area += (b.Width + borderWidthInPixels) * (b.Height + borderWidthInPixels);
                maxWidth = Math.Max(maxWidth, b.Width);
            }

            return Math.Max((int)Math.Sqrt((double)area), maxWidth + borderWidthInPixels);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _bitmap.Dispose();
        }

        #endregion

        private readonly Bitmap _bitmap;
        private readonly TextureCoordinateCollection _textureCoordinates;
        private readonly int _borderWidth;

        private class AnnotatedBitmap
        {
            public AnnotatedBitmap(Bitmap bitmap, int index)
            {
                _bitmap = bitmap;
                _index = index;
            }

            public Bitmap Bitmap { get { return _bitmap; } }
            public int Index { get { return _index; } }

            private Bitmap _bitmap;
            private int _index;
        }

        private class BitmapMaximumToMinimumHeight : IComparer<AnnotatedBitmap>
        {
            public int Compare(AnnotatedBitmap left, AnnotatedBitmap right)
            {
                return right.Bitmap.Height - left.Bitmap.Height;
            }
        }
    }
}
