﻿using BlockEngine.Client.Debugging;
using BlockEngine.Client.Threading;
using BlockEngine.Client.World;
using BlockEngine.Client.World.Regions.Chunks;
using OpenTK.Mathematics;

namespace BlockEngine.Client.Generation;

[Obsolete("This class is not used anymore, and will be removed in the future.")]
public class ChunkGeneratorManager : ChunkProcessorThreadManager<ChunkGeneratorThread, Vector3i>
{
    public ChunkGeneratorManager() : base(new ChunkGeneratorThread(), 64)
    {
        throw new NotImplementedException();
    }
    
    
    public override void ProcessQueues()
    {
// #if DEBUG
//         DebugStats.ChunksInGenerationQueue = (ulong)InputQueue.Count;
// #endif
//         base.ProcessQueues();
        throw new NotImplementedException();
    }


    protected override void OnChunkProcessed(Vector3i output)
    {
        // Chunk? chunk = GameWorld.CurrentGameWorld.RegionManager.GetChunkAt(output);
        //
        // chunk?.OnGenerated();
        throw new NotImplementedException();
    }
}