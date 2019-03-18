﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using LibreLancer;
using LibreLancer.Utf.Mat;
using LibreLancer.Utf.Cmp;
using DF = LibreLancer.Utf.Dfm;
using ImGuiNET;
namespace LancerEdit
{
    public partial class ModelViewer
	{
        Viewport3D modelViewport;
        Viewport3D previewViewport;
        Viewport3D imageViewport;

        float gizmoScale;
        const float RADIUS_ONE = 21.916825f;
        void SetupViewport()
        {
            modelViewport = new Viewport3D(_window);
            modelViewport.MarginH = 60;
            modelViewport.DefaultOffset =
            modelViewport.CameraOffset = new Vector3(0, 0, drawable.GetRadius() * 2);
            modelViewport.ModelScale = drawable.GetRadius() / 2.6f;
            previewViewport = new Viewport3D(_window);
            imageViewport = new Viewport3D(_window);

            gizmoScale = drawable.GetRadius() / RADIUS_ONE;
            wireframeMaterial3db = new Material(res);
            wireframeMaterial3db.Dc = Color4.White;
            wireframeMaterial3db.DtName = ResourceManager.WhiteTextureName;
            normalsDebugMaterial = new Material(res);
            normalsDebugMaterial.Type = "NormalDebugMaterial";
            lighting = Lighting.Create();
            lighting.Enabled = true;
            lighting.Ambient = Color3f.Black;
            var src = new SystemLighting();
            src.Lights.Add(new DynamicLight()
            {
                Light = new RenderLight()
                {
                    Kind = LightKind.Directional,
                    Direction = new Vector3(0, -1, 0),
                    Color = Color3f.White
                }
            });
            src.Lights.Add(new DynamicLight()
            {
                Light = new RenderLight()
                {
                    Kind = LightKind.Directional,
                    Direction = new Vector3(0, 0, 1),
                    Color = Color3f.White
                }
            });
            lighting.Lights.SourceLighting = src;
            lighting.Lights.SourceEnabled[0] = true;
            lighting.Lights.SourceEnabled[1] = true;
            lighting.NumberOfTilesX = -1;
            GizmoRender.Init(res);
        }
        void DoViewport()
        {
            modelViewport.Background = background;
            modelViewport.Begin();
            DrawGL(modelViewport.RenderWidth, modelViewport.RenderHeight);
            modelViewport.End();
            rotation = modelViewport.Rotation;
        }

        void DoPreview(int width, int height)
        {
            previewViewport.Background = renderBackground ? background : Color4.Black;
            previewViewport.Begin(width, height);
            DrawGL(width, height);
            previewViewport.End();
        }

        unsafe void RenderImage(string output)
        {
            try
            {
                TextureImport.LoadLibraries();
            }
            catch (Exception ex)
            {
                FLLog.Error("Render", "Could not load FreeImage");
                FLLog.Info("Exception Info", ex.Message + "\n" + ex.StackTrace);
                return;
            }
            imageViewport.Background = renderBackground ? background : Color4.TransparentBlack;
            imageViewport.Begin(imageWidth, imageHeight);
            DrawGL(imageWidth, imageHeight);
            imageViewport.End(false);
            byte[] data = new byte[imageWidth * imageHeight * 4];
            imageViewport.RenderTarget.GetData(data);
            using (var sfc = new TeximpNet.Surface(imageWidth, imageHeight, true))
            {
                byte* sfcData = (byte*)sfc.DataPtr;
                for (int i = 0; i < data.Length; i++) sfcData[i] = data[i];
                sfc.SaveToFile(TeximpNet.ImageFormat.PNG, output);
            }

        }

        long fR = 0;
        void DrawGL(int renderWidth, int renderHeight)
        {
            ICamera cam;
            //Draw Model
            var lookAtCam = new LookAtCamera();
            Matrix4 rot = Matrix4.CreateRotationX(modelViewport.CameraRotation.Y) *
                Matrix4.CreateRotationY(modelViewport.CameraRotation.X);
            var dir = rot.Transform(Vector3.Forward);
            var to = modelViewport.CameraOffset + (dir * 10);
            lookAtCam.Update(renderWidth, renderHeight, modelViewport.CameraOffset, to, rot);
            ThnCamera tcam = null;
            float znear = 0;
            float zfar = 0;
            if(doCockpitCam) {
                var vp = new Viewport(0, 0, renderWidth, renderHeight);
                tcam = new ThnCamera(vp);
                tcam.Transform.AspectRatio = renderWidth / (float)renderHeight;
                var tr = cameraPart.GetTransform(Matrix4.Identity);
                tcam.Transform.Orientation = Matrix4.CreateFromQuaternion(tr.ExtractRotation());
                tcam.Transform.Position = tr.Transform(Vector3.Zero);
                znear = cameraPart.Camera.Znear;
                zfar = cameraPart.Camera.Zfar;
                tcam.Transform.Znear = 0.001f;
                tcam.Transform.Zfar = 1000;
                tcam.Transform.FovH = MathHelper.RadiansToDegrees(cameraPart.Camera.Fovx);
                tcam.frameNo = fR++;
                tcam.Update();
                cam = tcam;
            }
            else {
                cam = lookAtCam;
            }
            _window.DebugRender.StartFrame(cam, rstate);

            drawable.Update(cam, TimeSpan.Zero, TimeSpan.FromSeconds(_window.TotalTime));
            if (viewMode != M_NONE)
            {
                int drawCount = doCockpitCam ? 2 : 1;
                for (int i = 0; i < drawCount; i++)
                {
                    buffer.StartFrame(rstate);
                    if (i == 1) {
                        rstate.ClearDepth();
                        tcam.Transform.Zfar = zfar;
                        tcam.Transform.Znear = znear;
                        tcam.frameNo = fR++;
                        tcam.Update();
                    }
                    if (drawable is CmpFile)
                        DrawCmp(cam, false);
                    else
                        DrawSimple(cam, false);
                    buffer.DrawOpaque(rstate);
                    rstate.DepthWrite = false;
                    buffer.DrawTransparent(rstate);
                    rstate.DepthWrite = true;
                }
            }
            if (doWireframe)
            {
                buffer.StartFrame(rstate);
                GL.PolygonOffset(1, 1);
                rstate.Wireframe = true;
                if (drawable is CmpFile)
                    DrawCmp(cam, true);
                else
                    DrawSimple(cam, true);
                GL.PolygonOffset(0, 0);
                buffer.DrawOpaque(rstate);
                rstate.Wireframe = false;
            }
            if(drawVMeshWire)
            {
                if (drawable is CmpFile)
                    WireCmp();
                else if (drawable is ModelFile)
                    Wire3db();
            }
            //Draw VMeshWire (if used)
            _window.DebugRender.Render();
            //Draw hardpoints
            DrawHardpoints(cam);
            if (drawSkeleton) DrawSkeleton(cam);
        }

        void DrawSkeleton(ICamera cam)
        {
            var matrix = GetModelMatrix();
            GizmoRender.Scale = gizmoScale;
            GizmoRender.Begin();
            var df = (DF.DfmFile)drawable;
            foreach(var c in df.Constructs.Constructs)
            {
                var b1 = c.BoneA;
                var b2 = c.BoneB;
                if (string.IsNullOrEmpty(c.BoneB))
                {
                    b2 = c.ParentName;
                    b1 = c.BoneA;
                }
                var conMat = c.Rotation * Matrix4.CreateTranslation(c.Origin);
                DF.Bone bone1 = null;
                DF.Bone bone2 = null;
                foreach(var k in df.Parts.Values)
                {
                    if (k.objectName == b1) bone1 = k.Bone;
                    if (k.objectName == b2) bone2 = k.Bone;
                }
                if (bone1 == null || bone2 == null) continue;
                GizmoRender.AddGizmo(bone2.BoneToRoot * bone1.BoneToRoot * conMat * matrix);
            }
            /*foreach(var b in df.Bones.Values)
            {
                var tr = b.BoneToRoot;
                tr.Transpose();
                if (b.Construct != null)
                    tr *= b.Construct.Transform;
                tr *= matrix;
               
                GizmoRender.AddGizmo(tr, true, true);
            }*/
            GizmoRender.RenderGizmos(cam, rstate);
        }

        Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateRotationX(rotation.Y) * Matrix4.CreateRotationY(rotation.X);
        }
                
        void ResetViewport()
        {

        }
        void WireCmp()
        {
            var cmp = (CmpFile)drawable;
            var matrix = GetModelMatrix();
            foreach (var part in cmp.Parts)
            {
                if(part.Model.VMeshWire != null) DrawVMeshWire(part.Model.VMeshWire, part.GetTransform(matrix));
            }
        }
        void Wire3db()
        {
            var model = (ModelFile)drawable;
            if (model.VMeshWire == null) return;
            var matrix = GetModelMatrix();
            DrawVMeshWire(model.VMeshWire, matrix);
        }
        void DrawVMeshWire(VMeshWire wires, Matrix4 mat)
        {
            var c = _window.DebugRender.Color;
            _window.DebugRender.Color = Color4.White;
            for (int i = 0; i < wires.Lines.Length / 2; i++)
            {
                _window.DebugRender.DrawLine(
                    mat.Transform(wires.Lines[i * 2]),
                    mat.Transform(wires.Lines[i * 2 + 1])
                );
            }
            _window.DebugRender.Color = c;
        }

        void DrawHardpoints(ICamera cam)
        {
            var matrix = GetModelMatrix();
            GizmoRender.Scale = gizmoScale;
            GizmoRender.Begin();
          
            foreach (var tr in gizmos)
            {
                if (tr.Enabled || tr.Override != null)
                {
                    var transform = tr.Override ?? tr.Definition.Transform;
                    //highlight edited cube
                    if(tr.Override != null) {
                        GizmoRender.CubeColor = Color4.CornflowerBlue;
                        GizmoRender.CubeAlpha = 0.5f;
                    } else {
                        GizmoRender.CubeColor = Color4.Purple;
                        GizmoRender.CubeAlpha = 0.3f;
                    }
                    //arc
                    if(tr.Definition is RevoluteHardpointDefinition) {
                        var rev = (RevoluteHardpointDefinition)tr.Definition;
                        var min = tr.Override == null ? rev.Min : tr.EditingMin;
                        var max = tr.Override == null ? rev.Max : tr.EditingMax;
                        GizmoRender.AddGizmoArc(transform * (tr.Parent == null ? Matrix4.Identity : tr.Parent.Transform) * matrix, min,max);
                    }
                    //
                    GizmoRender.AddGizmo(transform * (tr.Parent == null ? Matrix4.Identity : tr.Parent.Transform) * matrix);
                }
            }
            GizmoRender.RenderGizmos(cam, rstate);
        }

        void DrawSimple(ICamera cam, bool wireFrame)
        {
            Material mat = null;
            var matrix = Matrix4.Identity;
            if (isStarsphere)
                matrix = Matrix4.CreateTranslation(cam.Position);
            else
                matrix = GetModelMatrix();
            if (wireFrame || viewMode == M_FLAT)
            {
                mat = wireframeMaterial3db;
                mat.Update(cam);
            }
            else if (viewMode == M_NORMALS)
            {
                mat = normalsDebugMaterial;
                mat.Update(cam);
            }
                       
            ModelFile mdl = drawable as ModelFile;
            if (viewMode == M_LIT)
            {
                if (mdl != null)
                    mdl.DrawBufferLevel(mdl.Levels[GetLevel(mdl.Switch2, mdl.Levels.Length - 1)], buffer, matrix, ref lighting);
                else
                    drawable.DrawBuffer(buffer, matrix, ref lighting, mat);
            }
            else
            {
                if (mdl != null)
                    mdl.DrawBufferLevel(mdl.Levels[GetLevel(mdl.Switch2, mdl.Levels.Length - 1)], buffer, matrix, ref Lighting.Empty, mat);
                else
                    drawable.DrawBuffer(buffer, matrix, ref Lighting.Empty, mat);
            }
        }

        int jColors = 0;
        void DrawCmp(ICamera cam, bool wireFrame)
        {
            var matrix = GetModelMatrix();
            if (isStarsphere)
                matrix = Matrix4.CreateTranslation(cam.Position);
            var cmp = (CmpFile)drawable;
            if (wireFrame || viewMode == M_FLAT)
            {
                for (int i = 0; i < cmp.Parts.Count; i++)
                {
                    var part = cmp.Parts[i];
                    if (part.Camera != null) continue;
                    Material mat;
                    if (!partMaterials.TryGetValue(i, out mat))
                    {
                        mat = new Material(res);
                        mat.DtName = ResourceManager.WhiteTextureName;
                        mat.Dc = initialCmpColors[jColors++];
                        if (jColors >= initialCmpColors.Length) jColors = 0;
                        partMaterials.Add(i, mat);
                    }
                    mat.Update(cam);
                    part.DrawBufferLevel(
                        GetLevel(part.Model.Switch2, part.Model.Levels.Length - 1),
                        buffer, matrix, ref Lighting.Empty, mat
                    );
                }
            }
            else if (viewMode == M_TEXTURED || viewMode == M_LIT)
            {
                if (viewMode == M_LIT)
                {
                    foreach (var part in cmp.Parts)
                    {
                        if (part.Camera == null)
                            part.DrawBufferLevel(
                                GetLevel(part.Model.Switch2, part.Model.Levels.Length - 1),
                                buffer, matrix, ref lighting
                            );
                    }
                }
                else
                {
                    foreach (var part in cmp.Parts)
                    {
                        if(part.Camera == null)
                        part.DrawBufferLevel(
                                GetLevel(part.Model.Switch2, part.Model.Levels.Length - 1),
                                buffer, matrix, ref Lighting.Empty
                        );
                    }
                }
            }
            else
            {
                normalsDebugMaterial.Update(cam);
                foreach (var part in cmp.Parts)
                {
                    if (part.Camera == null)
                        part.DrawBufferLevel(
                            GetLevel(part.Model.Switch2, part.Model.Levels.Length - 1),
                            buffer, matrix, ref Lighting.Empty, normalsDebugMaterial
                        );
                }
            }
        }


    }
}
