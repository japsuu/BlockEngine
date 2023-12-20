﻿using OpenTK.Mathematics;

namespace BlockEngine.Client.Framework.Chunks;

public class ChunkColumn
{
    private readonly Chunk?[] _chunks;

    public readonly Vector2i Position;
        
        
    public ChunkColumn(Vector2i position)
    {
        Position = position;
        _chunks = new Chunk[Constants.CHUNK_COLUMN_HEIGHT];
    }
        
        
    public bool ReadyToUnload() => true;
        
    
    public Chunk? GetChunkAtHeight(int y)
    {
        if (y < 0 || y >= Constants.CHUNK_COLUMN_HEIGHT_BLOCKS)
            return null;
        int arrayIndex = y / Constants.CHUNK_SIZE;
        return _chunks[arrayIndex];
    }
        
    
    public Chunk? GetChunk(int i)
    {
        return _chunks[i];
    }
        
        
    public void Tick()
    {
        foreach (Chunk? chunk in _chunks)
        {
            chunk?.Tick();
        }
    }


    public void Load()
    {
        for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
        {
            Chunk chunk = new Chunk(new Vector3i(Position.X, i * Constants.CHUNK_SIZE, Position.Y));
            ChunkGenerator.Enqueue(chunk.Position);
            _chunks[i] = chunk;
        }
    }
        
        
    public void Unload()
    {
        for (int i = 0; i < Constants.CHUNK_COLUMN_HEIGHT; i++)
        {
            Chunk chunk = new Chunk(new Vector3i(Position.X, i * Constants.CHUNK_SIZE, Position.Y));
            chunk.Unload();
        }
    }
}