﻿using BlockEngine.Client.Debugging;
using BlockEngine.Client.Rendering.Chunks;
using BlockEngine.Client.Threading;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;

namespace BlockEngine.Client.Meshing;

public class ChunkMesherManager : ChunkProcessorThreadManager<ChunkMesherThread, ChunkMesh>
{
    public ChunkMesherManager() : base(new ChunkMesherThread(), 64)
    {
    }
    
    
    public override void ProcessQueues()
    {
        DebugStats.ChunksInMeshingQueue = (ulong)InputQueue.Count;
        
        base.ProcessQueues();
    }


    protected override void OnChunkProcessed(ChunkMesh output)
    {
        // Check if the chunk is still loaded
        Chunk? chunk = GameWorld.CurrentGameWorld.RegionManager.GetChunkAt(output.ChunkPos);
        if (chunk == null)
            return;
            
        ChunkRendererStorage.AddOrUpdateChunkMesh(output);
        chunk.OnMeshed();
    }
}