#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    [Flags]
    public enum TerrainRenderBuffers
    {
        Color = 1,
        Depth = 2,
        Silhouette = 4,
        All = Color | Depth | Silhouette
    }
    
    public class GlobeClipmapTerrain : IDisposable
    {
        public GlobeClipmapTerrain(Context context, RasterTerrainSource terrainSource, Ellipsoid ellipsoid, int clipmapPosts)
        {
            _terrainSource = terrainSource;
            _ellipsoid = ellipsoid;

            _clipmapPosts = clipmapPosts;
            _clipmapSegments = _clipmapPosts - 1;

            int clipmapLevels = _terrainSource.Levels.Count;
            _clipmapLevels = new ClipmapLevel[clipmapLevels];

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                _clipmapLevels[i] = new ClipmapLevel();
            }

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                RasterTerrainLevel terrainLevel = _terrainSource.Levels[i];
                _clipmapLevels[i].Terrain = terrainLevel;
                _clipmapLevels[i].HeightTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.Red32f));
                _clipmapLevels[i].NormalTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.RedGreenBlue32f));
                _clipmapLevels[i].CoarserLevel = i == 0 ? null : _clipmapLevels[i - 1];
                _clipmapLevels[i].FinerLevel = i == _clipmapLevels.Length - 1 ? null : _clipmapLevels[i + 1];
            }

            _clearColor = new ClearState();
            _clearColor.Buffers = ClearBuffers.ColorBuffer;

            _shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.GlobeClipmapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.GlobeClipmapFS.glsl"));

            _fillPatchPosts = (clipmapPosts + 1) / 4; // M
            _fillPatchSegments = _fillPatchPosts - 1;

            // Create the MxM block used to fill the ring and the field.
            Mesh fieldBlockMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fillPatchSegments, _fillPatchSegments)),
                _fillPatchSegments, _fillPatchSegments);
            _fillPatch = context.CreateVertexArray(fieldBlockMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the Mx3 block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fillPatchSegments, 2.0)),
                _fillPatchSegments, 2);
            _horizontalFixupPatch = context.CreateVertexArray(ringFixupHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the 3xM block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, _fillPatchSegments)),
                2, _fillPatchSegments);
            _verticalFixupPatch = context.CreateVertexArray(ringFixupVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fillPatchPosts, 1.0)),
                2 * _fillPatchPosts, 1);
            _horizontalOffsetPatch = context.CreateVertexArray(offsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 2 * _fillPatchPosts - 1)),
                1, 2 * _fillPatchPosts - 1);
            _verticalOffsetPatch = context.CreateVertexArray(offsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh centerMesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, 2.0)), 2, 2);
            _centerPatch = context.CreateVertexArray(centerMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh degenerateTriangleMesh = CreateDegenerateTriangleMesh();
            _degenerateTrianglePatch = context.CreateVertexArray(degenerateTriangleMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _patchOriginInClippedLevel = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_patchOriginInClippedLevel"];
            _levelScaleFactor = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelScaleFactor"];
            _levelZeroWorldScaleFactor = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelZeroWorldScaleFactor"];
            _levelOffsetFromWorldOrigin = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelOffsetFromWorldOrigin"];
            _heightExaggeration = (Uniform<float>)_shaderProgram.Uniforms["u_heightExaggeration"];
            _viewPosInClippedLevel = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_viewPosInClippedLevel"];
            _fineLevelOriginInCoarse = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_fineLevelOriginInCoarse"];
            _fineTextureOrigin = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_fineTextureOrigin"];
            _showBlendRegions = (Uniform<bool>)_shaderProgram.Uniforms["u_showBlendRegions"];
            _useBlendRegions = (Uniform<bool>)_shaderProgram.Uniforms["u_useBlendRegions"];
            _color = (Uniform<Vector3F>)_shaderProgram.Uniforms["u_color"];
            _blendRegionColor = (Uniform<Vector3F>)_shaderProgram.Uniforms["u_blendRegionColor"];

            ((Uniform<Vector3F>)_shaderProgram.Uniforms["u_globeRadiiSquared"]).Value = ellipsoid.RadiiSquared.ToVector3F();
            ((Uniform<float>)_shaderProgram.Uniforms["u_oneOverClipmapSize"]).Value = 1.0f / clipmapPosts;
            ((Uniform<Vector2F>)_shaderProgram.Uniforms["u_unblendedRegionSize"]).Value = new Vector2F((float)(_clipmapSegments / 2 - _clipmapPosts / 10.0 - 1));
            ((Uniform<Vector2F>)_shaderProgram.Uniforms["u_oneOverBlendedRegionSize"]).Value = new Vector2F((float)(10.0 / _clipmapPosts));

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;

            _useBlendRegions.Value = true;

            ///////////////////////////////////////////////////////////////////

            _shaderProgramSilhouette = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.GlobeClipmapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.SilhouetteGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.SilhouetteFS.glsl"));

            ((Uniform<float>)_shaderProgramSilhouette.Uniforms["u_fillDistance"]).Value = 1.5f;
            ((Uniform<Vector3F>)_shaderProgramSilhouette.Uniforms["u_globeRadiiSquared"]).Value = ellipsoid.RadiiSquared.ToVector3F();
            ((Uniform<float>)_shaderProgramSilhouette.Uniforms["u_oneOverClipmapSize"]).Value = 1.0f / clipmapPosts;
            ((Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_unblendedRegionSize"]).Value = new Vector2F((float)(_clipmapSegments / 2 - _clipmapPosts / 10.0 - 1));
            ((Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_oneOverBlendedRegionSize"]).Value = new Vector2F((float)(10.0 / _clipmapPosts));
            _patchOriginInClippedLevelSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_patchOriginInClippedLevel"];
            _levelScaleFactorSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_levelScaleFactor"];
            _levelZeroWorldScaleFactorSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_levelZeroWorldScaleFactor"];
            _levelOffsetFromWorldOriginSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_levelOffsetFromWorldOrigin"];
            _heightExaggerationSilhouette = (Uniform<float>)_shaderProgramSilhouette.Uniforms["u_heightExaggeration"];
            _viewPosInClippedLevelSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_viewPosInClippedLevel"];
            _fineLevelOriginInCoarseSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_fineLevelOriginInCoarse"];
            _fineTextureOriginSilhouette = (Uniform<Vector2F>)_shaderProgramSilhouette.Uniforms["u_fineTextureOrigin"];
            _useBlendRegionsSilhouette = (Uniform<bool>)_shaderProgramSilhouette.Uniforms["u_useBlendRegions"];
            
            _useBlendRegionsSilhouette.Value = true;

            _renderStateSilhouette = new RenderState();
            _renderStateSilhouette.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _renderStateSilhouette.FacetCulling.Face = CullFace.Front;
            _renderStateSilhouette.DepthMask = false;

            ///////////////////////////////////////////////////////////////////

            _updater = new ClipmapUpdater(context, _clipmapLevels);

            HeightExaggeration = 0.00001f;
        }

        public bool Wireframe
        {
            get { return _wireframe; }
            set { _wireframe = value; }
        }

        public bool BlendRegionsEnabled
        {
            get { return _blendRegionsEnabled; }
            set { _blendRegionsEnabled = value; }
        }

        public bool ShowBlendRegions
        {
            get { return _showBlendRegions.Value; }
            set { _showBlendRegions.Value = value; }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set
            {
                _heightExaggeration.Value = value;
                _heightExaggerationSilhouette.Value = _heightExaggeration.Value;
                _updater.HeightExaggeration = 0.00001f; // value;
            }
        }

        public bool LodUpdateEnabled
        {
            get { return _lodUpdateEnabled; }
            set { _lodUpdateEnabled = value; }
        }

        public bool ColorClipmapLevels
        {
            get { return _colorClipmapLevels; }
            set { _colorClipmapLevels = value; }
        }

        public Texture2D SilhouetteTexture 
        { 
            get { return _silhouetteTexture; } 
        }

        public Texture2D DepthTexture 
        { 
            get { return _depthTexture; } 
        }

        private void CreateDepthTexture(Context context)
        {
            if ((_depthTexture == null) ||
                (_depthTexture.Description.Width != context.Viewport.Width) ||
                (_depthTexture.Description.Height != context.Viewport.Height))
            {
                DisposeDepth();
                _depthTexture = Device.CreateTexture2D(new Texture2DDescription(context.Viewport.Width, context.Viewport.Height, TextureFormat.Depth24Stencil8));
            }
        }

        private void CreateSilhouetteFramebuffer(Context context)
        {
            if ((_silhouetteTexture == null) ||
                (_silhouetteTexture.Description.Width != context.Viewport.Width) ||
                (_silhouetteTexture.Description.Height != context.Viewport.Height))
            {
                DisposeSilhouette();
                _silhouetteTexture = Device.CreateTexture2D(new Texture2DDescription(context.Viewport.Width, context.Viewport.Height, TextureFormat.Red8));

                _silhouetteFrameBuffer = context.CreateFramebuffer();
                _silhouetteFrameBuffer.DepthAttachment = _depthTexture;
                _silhouetteFrameBuffer.ColorAttachments[0] = _silhouetteTexture;
            }
        }

        public void PreRender(Context context, SceneState sceneState)
        {
            if (!_lodUpdateEnabled)
                return;

            _clipmapCenter = _ellipsoid.ToGeodetic3D(sceneState.Camera.Eye);

            //Geodetic2D center = Ellipsoid.ScaledWgs84.ToGeodetic2D(sceneState.Camera.Target / Ellipsoid.Wgs84.MaximumRadius);
            Geodetic2D center = new Geodetic2D(_clipmapCenter.Longitude, _clipmapCenter.Latitude);
            double centerLongitude = Trig.ToDegrees(center.Longitude);
            double centerLatitude = Trig.ToDegrees(center.Latitude);

            ClipmapLevel level = _clipmapLevels[_clipmapLevels.Length - 1];
            double longitudeIndex = level.Terrain.LongitudeToIndex(centerLongitude);
            double latitudeIndex = level.Terrain.LatitudeToIndex(centerLatitude);

            int west = (int)(longitudeIndex - _clipmapPosts / 2);
            if ((west % 2) != 0)
            {
                ++west;
            }
            int south = (int)(latitudeIndex - _clipmapPosts / 2);
            if ((south % 2) != 0)
            {
                ++south;
            }

            level.NextExtent.West = west;
            level.NextExtent.East = west + _clipmapSegments;
            level.NextExtent.South = south;
            level.NextExtent.North = south + _clipmapSegments;

            UpdateOriginInTextures(level);

            for (int i = _clipmapLevels.Length - 2; i >= 0; --i)
            {
                level = _clipmapLevels[i];
                ClipmapLevel finerLevel = _clipmapLevels[i + 1];

                level.NextExtent.West = finerLevel.NextExtent.West / 2 - _fillPatchSegments;
                level.OffsetStripOnEast = (level.NextExtent.West % 2) == 0;
                if (!level.OffsetStripOnEast)
                {
                    --level.NextExtent.West;
                }
                level.NextExtent.East = level.NextExtent.West + _clipmapSegments;

                level.NextExtent.South = finerLevel.NextExtent.South / 2 - _fillPatchSegments;
                level.OffsetStripOnNorth = (level.NextExtent.South % 2) == 0;
                if (!level.OffsetStripOnNorth)
                {
                    --level.NextExtent.South;
                }
                level.NextExtent.North = level.NextExtent.South + _clipmapSegments;

                UpdateOriginInTextures(level);
            }

            _updater.ApplyNewData(context);

            for (int i = 0; i <_clipmapLevels.Length; ++i)
            {
                ClipmapLevel thisLevel = _clipmapLevels[i];
                ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                _updater.RequestTileResidency(context, thisLevel);
                PreRenderLevel(thisLevel, coarserLevel, context, sceneState);
            }
        }

        private void UpdateOriginInTextures(ClipmapLevel level)
        {
            int deltaX = level.NextExtent.West - level.CurrentExtent.West;
            int deltaY = level.NextExtent.South - level.CurrentExtent.South;
            if (deltaX == 0 && deltaY == 0)
                return;

            if (level.CurrentExtent.West > level.CurrentExtent.East ||  // initial update
                Math.Abs(deltaX) >= _clipmapPosts || Math.Abs(deltaY) >= _clipmapPosts)      // complete update
            {
                level.OriginInTextures = new Vector2I(0, 0);
            }
            else
            {
                int newOriginX = (level.OriginInTextures.X + deltaX) % _clipmapPosts;
                if (newOriginX < 0)
                    newOriginX += _clipmapPosts;
                int newOriginY = (level.OriginInTextures.Y + deltaY) % _clipmapPosts;
                if (newOriginY < 0)
                    newOriginY += _clipmapPosts;
                level.OriginInTextures = new Vector2I(newOriginX, newOriginY);
            }
        }

        private void PreRenderLevel(ClipmapLevel level, ClipmapLevel coarserLevel, Context context, SceneState sceneState)
        {
            int deltaX = level.NextExtent.West - level.CurrentExtent.West;
            int deltaY = level.NextExtent.South - level.CurrentExtent.South;
            if (deltaX == 0 && deltaY == 0)
                return;

            int minLongitude = deltaX > 0 ? level.CurrentExtent.East + 1 : level.NextExtent.West;
            int maxLongitude = deltaX > 0 ? level.NextExtent.East : level.CurrentExtent.West - 1;
            int minLatitude = deltaY > 0 ? level.CurrentExtent.North + 1 : level.NextExtent.South;
            int maxLatitude = deltaY > 0 ? level.NextExtent.North : level.CurrentExtent.South - 1;

            int width = maxLongitude - minLongitude + 1;
            int height = maxLatitude - minLatitude + 1;

            if (level.CurrentExtent.West > level.CurrentExtent.East || // initial update
                width >= _clipmapPosts || height >= _clipmapPosts) // complete update
            {
                // Initial or complete update.
                width = _clipmapPosts;
                height = _clipmapPosts;
                deltaX = _clipmapPosts;
                deltaY = _clipmapPosts;
                minLongitude = level.NextExtent.West;
                maxLongitude = level.NextExtent.East;
                minLatitude = level.NextExtent.South;
                maxLatitude = level.NextExtent.North;
            }

            if (height > 0)
            {
                ClipmapUpdate horizontalUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West,
                    minLatitude,
                    level.NextExtent.East,
                    maxLatitude);
                _updater.Update(context, horizontalUpdate);
            }

            if (width > 0)
            {
                ClipmapUpdate verticalUpdate = new ClipmapUpdate(
                    level,
                    minLongitude,
                    level.NextExtent.South,
                    maxLongitude,
                    level.NextExtent.North);
                _updater.Update(context, verticalUpdate);
            }

            //_updater.VerifyHeights(level);

            level.CurrentExtent.West = level.NextExtent.West;
            level.CurrentExtent.South = level.NextExtent.South;
            level.CurrentExtent.East = level.NextExtent.East;
            level.CurrentExtent.North = level.NextExtent.North;
        }

        public void Render(Context context, SceneState sceneState, TerrainRenderBuffers buffers)
        {
            if (_wireframe)
            {
                _renderState.RasterizationMode = RasterizationMode.Line;
            }
            else
            {
                _renderState.RasterizationMode = RasterizationMode.Fill;
            }

            Vector3D previousTarget = sceneState.Camera.Target;
            Vector3D previousEye = sceneState.Camera.Eye;
            Vector3D previousSun = sceneState.SunPosition;

            //Vector3D toSubtract = new Vector3D(_clipmapCenter.X, _clipmapCenter.Y, 0.0);
            //sceneState.Camera.Target -= toSubtract;
            //sceneState.Camera.Eye -= toSubtract;
            //sceneState.SunPosition -= toSubtract;

            _levelZeroWorldScaleFactor.Value = new Vector2F((float)_clipmapLevels[0].Terrain.PostDeltaLongitude, (float)_clipmapLevels[0].Terrain.PostDeltaLatitude);
            _levelZeroWorldScaleFactorSilhouette.Value = _levelZeroWorldScaleFactor.Value;

            int maxLevel = _clipmapLevels.Length - 1;

            int longitudeIndex = (int)_clipmapLevels[0].Terrain.LongitudeToIndex(_clipmapCenter.Longitude);
            int latitudeIndex = (int)_clipmapLevels[0].Terrain.LatitudeToIndex(_clipmapCenter.Latitude);

            float[] heightSample = new float[1];
            _clipmapLevels[0].Terrain.GetPosts(longitudeIndex, latitudeIndex, longitudeIndex, latitudeIndex, heightSample, 0, 1);

            /*while (maxLevel > 0)
            {
                double terrainHeight = heightSample[0] * _heightExaggeration.Value; // TODO: get the real terrain height
                double viewerHeight = clipmapCenter.Z;
                double h = viewerHeight - terrainHeight;
                double gridExtent = _clipmapLevels[maxLevel].Terrain.PostDeltaLongitude * _clipmapPosts;
                if (h <= 0.4 * gridExtent)
                {
                    break;
                }
                --maxLevel;
            }*/

            //Vector2D center = toSubtract.XY;
            Vector2D center = Vector2D.Zero;

            _renderState.ColorMask = ((buffers & TerrainRenderBuffers.Color) == TerrainRenderBuffers.Color) ? ColorMask.True : ColorMask.False;

            for (int i = maxLevel; i >= 0; --i)
            {
                ClipmapLevel thisLevel = _clipmapLevels[i];
                ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];
                RenderLevel(i, thisLevel, coarserLevel, (i == maxLevel), false, center, context, sceneState);
            }

            if (((buffers & TerrainRenderBuffers.Depth) == TerrainRenderBuffers.Depth) ||
                ((buffers & TerrainRenderBuffers.Silhouette) == TerrainRenderBuffers.Silhouette))
            {
                CreateDepthTexture(context);
                _depthTexture.CopyFromFramebuffer();
            }
            else
            {
                DisposeDepth();
            }

            if ((buffers & TerrainRenderBuffers.Silhouette) == TerrainRenderBuffers.Silhouette)
            {
                CreateSilhouetteFramebuffer(context);

                Framebuffer oldFramebuffer = context.Framebuffer;
                context.Framebuffer = _silhouetteFrameBuffer;
                context.Clear(_clearColor);

                Device.Hack();
                _renderState.RasterizationMode = RasterizationMode.Line;
                _renderState.FacetCulling.Face = CullFace.Front;

                for (int i = maxLevel; i >= 0; --i)
                {
                    ClipmapLevel thisLevel = _clipmapLevels[i];
                    ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];
                    RenderLevel(i, thisLevel, coarserLevel, (i == maxLevel), true, center, context, sceneState);
                }

                _renderState.FacetCulling.Face = CullFace.Back;
                _renderState.RasterizationMode = RasterizationMode.Fill;

                context.Framebuffer = oldFramebuffer;
            }
            else
            {
                DisposeSilhouette();
            }

            sceneState.Camera.Target = previousTarget;
            sceneState.Camera.Eye = previousEye;
            sceneState.SunPosition = previousSun;
        }

        private void RenderLevel(int levelIndex, ClipmapLevel level, ClipmapLevel coarserLevel, bool fillRing, bool renderSilhouette, Vector2D center, Context context, SceneState sceneState)
        {
            context.TextureUnits[0].Texture = level.HeightTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestRepeat;
            context.TextureUnits[1].Texture = coarserLevel.HeightTexture;
            context.TextureUnits[1].TextureSampler = Device.TextureSamplers.LinearRepeat;
            context.TextureUnits[2].Texture = level.NormalTexture;
            context.TextureUnits[2].TextureSampler = Device.TextureSamplers.LinearRepeat;
            context.TextureUnits[3].Texture = coarserLevel.NormalTexture;
            context.TextureUnits[3].TextureSampler = Device.TextureSamplers.LinearRepeat;

            if (_colorClipmapLevels)
            {
                _color.Value = _colors[levelIndex];
                _blendRegionColor.Value = levelIndex == 0 ? _colors[levelIndex] : _colors[levelIndex - 1];
            }
            else
            {
                _color.Value = new Vector3F(0.0f, 1.0f, 0.0f);
                _blendRegionColor.Value = new Vector3F(0.0f, 0.0f, 1.0f);
            }

            int west = level.CurrentExtent.West;
            int south = level.CurrentExtent.South;
            int east = level.CurrentExtent.East;
            int north = level.CurrentExtent.North;

            float levelScaleFactor = (float)Math.Pow(2.0, -levelIndex);
            _levelScaleFactor.Value = new Vector2F(levelScaleFactor, levelScaleFactor);
            _levelScaleFactorSilhouette.Value = _levelScaleFactor.Value;

            _levelOffsetFromWorldOrigin.Value = new Vector2F((float)((double)level.CurrentExtent.West - level.Terrain.LongitudeToIndex(0.0)),
                                                             (float)((double)level.CurrentExtent.South - level.Terrain.LatitudeToIndex(0.0)));
            _levelOffsetFromWorldOriginSilhouette.Value = _levelOffsetFromWorldOrigin.Value;

            int coarserWest = coarserLevel.CurrentExtent.West;
            int coarserSouth = coarserLevel.CurrentExtent.South;
            _fineLevelOriginInCoarse.Value = coarserLevel.OriginInTextures.ToVector2F() +
                                             new Vector2F(west / 2 - coarserWest + 0.5f,
                                                          south / 2 - coarserSouth + 0.5f);
            _fineLevelOriginInCoarseSilhouette.Value = _fineLevelOriginInCoarse.Value;

            _viewPosInClippedLevel.Value = new Vector2F((float)(level.Terrain.LongitudeToIndex(Trig.ToDegrees(_clipmapCenter.Longitude)) - level.CurrentExtent.West),
                                                        (float)(level.Terrain.LatitudeToIndex(Trig.ToDegrees(_clipmapCenter.Latitude)) - level.CurrentExtent.South));
            _viewPosInClippedLevelSilhouette.Value = _viewPosInClippedLevel.Value;

            _fineTextureOrigin.Value = level.OriginInTextures.ToVector2F() + new Vector2F(0.5f, 0.5f);
            _fineTextureOriginSilhouette.Value = _fineTextureOrigin.Value;

            _useBlendRegions.Value = _blendRegionsEnabled && level != coarserLevel;
            _useBlendRegionsSilhouette.Value = _useBlendRegions.Value;

            DrawBlock(_fillPatch, renderSilhouette, west, south, west, south, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, west + _fillPatchSegments, south, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, east - 2 * _fillPatchSegments, south, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, east - _fillPatchSegments, south, context, sceneState);

            DrawBlock(_fillPatch, renderSilhouette, west, south, west, south + _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, east - _fillPatchSegments, south + _fillPatchSegments, context, sceneState);

            DrawBlock(_fillPatch, renderSilhouette, west, south, west, north - 2 * _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, east - _fillPatchSegments, north - 2 * _fillPatchSegments, context, sceneState);

            DrawBlock(_fillPatch, renderSilhouette, west, south, west, north - _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, west + _fillPatchSegments, north - _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, east - 2 * _fillPatchSegments, north - _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, renderSilhouette, west, south, east - _fillPatchSegments, north - _fillPatchSegments, context, sceneState);

            DrawBlock(_horizontalFixupPatch, renderSilhouette, west, south, west, south + 2 * _fillPatchSegments, context, sceneState);
            DrawBlock(_horizontalFixupPatch, renderSilhouette, west, south, east - _fillPatchSegments, south + 2 * _fillPatchSegments, context, sceneState);

            DrawBlock(_verticalFixupPatch, renderSilhouette, west, south, west + 2 * _fillPatchSegments, south, context, sceneState);
            DrawBlock(_verticalFixupPatch, renderSilhouette, west, south, west + 2 * _fillPatchSegments, north - _fillPatchSegments, context, sceneState);

            if (!renderSilhouette)
            {
                DrawBlock(_degenerateTrianglePatch, renderSilhouette, west, south, west, south, context, sceneState);
            }

            // Fill the center of the highest-detail ring
            if (fillRing)
            {
                DrawBlock(_fillPatch, renderSilhouette, west, south, west + _fillPatchSegments, south + _fillPatchSegments, context, sceneState);
                DrawBlock(_fillPatch, renderSilhouette, west, south, west + 2 * _fillPatchPosts, south + _fillPatchSegments, context, sceneState);
                DrawBlock(_fillPatch, renderSilhouette, west, south, west + _fillPatchSegments, south + 2 * _fillPatchPosts, context, sceneState);
                DrawBlock(_fillPatch, renderSilhouette, west, south, west + 2 * _fillPatchPosts, south + 2 * _fillPatchPosts, context, sceneState);

                DrawBlock(_horizontalFixupPatch, renderSilhouette, west, south, west + _fillPatchSegments, south + 2 * _fillPatchSegments, context, sceneState);
                DrawBlock(_horizontalFixupPatch, renderSilhouette, west, south, west + 2 * _fillPatchPosts, south + 2 * _fillPatchSegments, context, sceneState);

                DrawBlock(_verticalFixupPatch, renderSilhouette, west, south, west + 2 * _fillPatchSegments, south + _fillPatchSegments, context, sceneState);
                DrawBlock(_verticalFixupPatch, renderSilhouette, west, south, west + 2 * _fillPatchSegments, south + 2 * _fillPatchPosts, context, sceneState);

                DrawBlock(_centerPatch, renderSilhouette, west, south, west + 2 * _fillPatchSegments, south + 2 * _fillPatchSegments, context, sceneState);
            }
            else
            {
                int offset = level.OffsetStripOnNorth
                                ? north - _fillPatchPosts
                                : south + _fillPatchSegments;
                DrawBlock(_horizontalOffsetPatch, renderSilhouette, west, south, west + _fillPatchSegments, offset, context, sceneState);

                int southOffset = level.OffsetStripOnNorth ? 0 : 1;
                offset = level.OffsetStripOnEast
                                ? east - _fillPatchPosts
                                : west + _fillPatchSegments;
                DrawBlock(_verticalOffsetPatch, renderSilhouette, west, south, offset, south + _fillPatchSegments + southOffset, context, sceneState);
            }
        }

        private void DrawBlock(VertexArray block, bool renderSilhouette, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            _patchOriginInClippedLevel.Value = new Vector2F(textureWest, textureSouth);
            _patchOriginInClippedLevelSilhouette.Value = _patchOriginInClippedLevel.Value;

            DrawState drawState = null;
            //if (!renderSilhouette)
            //{
                drawState = new DrawState(_renderState, _shaderProgram, block);
            //}
            //else
            //{
            //    drawState = new DrawState(_renderStateSilhouette, _shaderProgramSilhouette, block);
            //}
            context.Draw(_primitiveType, drawState, sceneState);
        }

        public void Dispose()
        {
            _shaderProgramSilhouette.Dispose();

            DisposeDepth();
            DisposeSilhouette();
        }

        private void DisposeDepth()
        {
            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
                _depthTexture = null;
            }
        }

        private void DisposeSilhouette()
        {
            if (_silhouetteTexture != null)
            {
                _silhouetteTexture.Dispose();
                _silhouetteTexture = null;
            }

            if (_silhouetteFrameBuffer != null)
            {
                _silhouetteFrameBuffer.Dispose();
                _silhouetteFrameBuffer = null;
            }
        }

        private Mesh CreateDegenerateTriangleMesh()
        {
            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = _clipmapSegments * 4;
            VertexAttributeFloatVector2 positionsAttribute = new VertexAttributeFloatVector2("position", numberOfPositions);
            IList<Vector2F> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (_clipmapSegments / 2) * 3 * 4;
            IndicesUnsignedShort indices = new IndicesUnsignedShort(numberOfIndices);
            mesh.Indices = indices;

            for (int i = 0; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2F(0.0f, i));
            }

            for (int i = 1; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2F(i, _clipmapSegments));
            }

            for (int i = _clipmapSegments - 1; i >= 0; --i)
            {
                positions.Add(new Vector2F(_clipmapSegments, i));
            }

            for (int i = _clipmapSegments - 1; i > 0; --i)
            {
                positions.Add(new Vector2F(i, 0.0f));
            }

            for (int i = 0; i < numberOfIndices; i += 2)
            {
                indices.AddTriangle(new TriangleIndicesUnsignedShort((ushort)i, (ushort)(i + 1), (ushort)(i + 2)));
            }

            return mesh;
        }

        /*
        private double EstimateLevelExtent(ClipmapLevel level)
        {
            int east = level.CurrentExtent.West + _clipmapSegments;
            int north = level.CurrentExtent.South + _clipmapSegments;

            Geodetic2D southwest = new Geodetic2D(
                                    Trig.ToRadians(level.Terrain.IndexToLongitude(level.CurrentExtent.West)),
                                    Trig.ToRadians(level.Terrain.IndexToLatitude(level.CurrentExtent.South)));
            Geodetic2D northeast = new Geodetic2D(
                                    Trig.ToRadians(level.Terrain.IndexToLongitude(east)),
                                    Trig.ToRadians(level.Terrain.IndexToLatitude(north)));

            Vector3D southwestCartesian = Ellipsoid.ScaledWgs84.ToVector3D(southwest);
            Vector3D northeastCartesian = Ellipsoid.ScaledWgs84.ToVector3D(northeast);

            return (northeastCartesian - southwestCartesian).Magnitude;
        }
        */

        private static Vector3F[] CreateColors()
        {
            int i = 0;
            Vector3F[] colors = new Vector3F[20];
            colors[i++] = new Vector3F(1.0f, 0.42f, 0.0f);
            colors[i++] = new Vector3F(0.0f, 0.58f, 1.0f);
            colors[i++] = new Vector3F(0.0f, 0.5f, 0.05f);
            colors[i++] = new Vector3F(0.7f, 0.0f, 1.0f);
            colors[i++] = new Vector3F(0.0f, 0.78f, 0.78f);
            colors[i++] = new Vector3F(1.0f, 0.85f, 0.0f);

            Random random = new Random();
            for (; i < colors.Length; ++i)
            {
                colors[i] = new Vector3F((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            }
            return colors;
        }

        private RasterTerrainSource _terrainSource;
        private int _clipmapPosts;
        private int _clipmapSegments;
        private ClipmapLevel[] _clipmapLevels;

        private readonly ClearState _clearColor;

        private ShaderProgram _shaderProgram;
        private RenderState _renderState;
        private PrimitiveType _primitiveType;

        private int _fillPatchPosts;
        private int _fillPatchSegments;

        private VertexArray _fillPatch;
        private VertexArray _horizontalFixupPatch;
        private VertexArray _verticalFixupPatch;
        private VertexArray _horizontalOffsetPatch;
        private VertexArray _verticalOffsetPatch;
        private VertexArray _centerPatch;
        private VertexArray _degenerateTrianglePatch;

        private readonly Uniform<Vector2F> _patchOriginInClippedLevel;
        private readonly Uniform<Vector2F> _levelScaleFactor;
        private readonly Uniform<Vector2F> _levelZeroWorldScaleFactor;
        private readonly Uniform<Vector2F> _levelOffsetFromWorldOrigin;
        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<Vector2F> _fineLevelOriginInCoarse;
        private readonly Uniform<Vector2F> _viewPosInClippedLevel;
        private readonly Uniform<Vector2F> _fineTextureOrigin;
        private readonly Uniform<bool> _showBlendRegions;
        private readonly Uniform<bool> _useBlendRegions;
        private readonly Uniform<Vector3F> _color;
        private readonly Uniform<Vector3F> _blendRegionColor;

        private readonly ShaderProgram _shaderProgramSilhouette;
        private readonly Uniform<Vector2F> _patchOriginInClippedLevelSilhouette;
        private readonly Uniform<Vector2F> _levelScaleFactorSilhouette;
        private readonly Uniform<Vector2F> _levelZeroWorldScaleFactorSilhouette;
        private readonly Uniform<Vector2F> _levelOffsetFromWorldOriginSilhouette;
        private readonly Uniform<float> _heightExaggerationSilhouette;
        private readonly Uniform<Vector2F> _fineLevelOriginInCoarseSilhouette;
        private readonly Uniform<Vector2F> _viewPosInClippedLevelSilhouette;
        private readonly Uniform<Vector2F> _fineTextureOriginSilhouette;
        private readonly Uniform<bool> _useBlendRegionsSilhouette;
        private readonly RenderState _renderStateSilhouette;

        private Texture2D _silhouetteTexture;
        private Texture2D _depthTexture;
        private Framebuffer _silhouetteFrameBuffer;

        private bool _wireframe;
        private bool _blendRegionsEnabled = true;
        private bool _lodUpdateEnabled = true;
        private bool _colorClipmapLevels;

        private ClipmapUpdater _updater;
        private Geodetic3D _clipmapCenter;

        private static readonly Vector3F[] _colors = CreateColors();
        private Ellipsoid _ellipsoid;
    }
}
