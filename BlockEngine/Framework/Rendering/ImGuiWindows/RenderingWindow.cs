﻿using System.Diagnostics;
using System.Globalization;
using BlockEngine.Framework.Debugging;
using BlockEngine.Framework.Meshing;
using BlockEngine.Utils;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public class RenderingWindow : ImGuiWindow
{
    public static class RenderingStats
    {
        public static ulong LoadedColumnCount;
        public static ulong ChunksInMeshingQueue;
        public static float MeshingTime { get; private set; }
        
        private static readonly Stopwatch MeshingTimer = new();
        
        
        public static void StartMeshing()
        {
            MeshingTimer.Restart();
        }
        
        
        public static void StopMeshing(int chunksMeshed)
        {
            MeshingTimer.Stop();
            MeshingTime = MeshingTimer.ElapsedMilliseconds / (float)chunksMeshed;
        }
    }
    
    public override string Title => "Rendering Settings";

    private readonly NumberFormatInfo _numberFormat;


    public RenderingWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
        _numberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _numberFormat.NumberGroupSeparator = " ";
    }


    protected override void UpdateContent()
    {
        ImGui.Checkbox("Wireframe rendering", ref DebugSettings.RenderWireframe);

        if (ImGui.Checkbox("Render chunk borders", ref DebugSettings.RenderChunkBorders))
        {
            if (DebugSettings.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Render column borders", ref DebugSettings.RenderChunkColumnBorders))
        {
            if (DebugSettings.RenderChunkColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        
        ImGui.Checkbox("Render skybox", ref DebugSettings.RenderSkybox);

        ulong loadedChunks = RenderingStats.LoadedColumnCount * Constants.CHUNK_COLUMN_HEIGHT;
        ImGui.Text($"Loaded Blocks = {(loadedChunks * Constants.CHUNK_SIZE_CUBED).ToString("#,0", _numberFormat)}");
        ImGui.Text($"Loaded Chunks = {(loadedChunks).ToString("#,0", _numberFormat)}");
        ImGui.Text($"Loaded Columns = {RenderingStats.LoadedColumnCount}");
        ImGui.Text($"Cached chunk meshes = {ChunkRendererStorage.GeneratedRendererCount}");
        ImGui.Text($"Chunks in meshing queue = {RenderingStats.ChunksInMeshingQueue}");
        ImGui.Text($"Meshing time = {RenderingStats.MeshingTime:F1}ms");
    }
}