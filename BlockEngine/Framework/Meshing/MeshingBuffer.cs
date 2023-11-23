﻿using BlockEngine.Framework.Bitpacking;
using BlockEngine.Framework.Blocks;
using BlockEngine.Utils;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Meshing;

/// <summary>
/// Buffer in to which meshes are generated.
/// </summary>
public class MeshingBuffer
{
    private const int ELEMENTS_PER_VERTEX = 2;
    private const int FACES_PER_BLOCK = 6;
    private const int VERTS_PER_FACE = 4;
    private const int INDICES_PER_FACE = 6;
    // Since we cull internal faces, the worst case is half of the faces (every other block needs to be meshed).
    private const int MAX_VISIBLE_FACES = Constants.CHUNK_SIZE_CUBED * FACES_PER_BLOCK / 2;
    private const int MAX_VERTS_PER_CHUNK = MAX_VISIBLE_FACES * VERTS_PER_FACE;
    private const int MAX_INDICES_PER_CHUNK = MAX_VISIBLE_FACES * INDICES_PER_FACE;
    private const int MAX_VERTEX_DATA_PER_CHUNK = MAX_VERTS_PER_CHUNK * ELEMENTS_PER_VERTEX;
    
    /// <summary>
    /// Array of bytes containing the vertex data. 2 uints (64 bits) per vertex.
    /// </summary>
    private readonly uint[] _vertexData = new uint[MAX_VERTEX_DATA_PER_CHUNK];
    private readonly uint[] _indices = new uint[MAX_INDICES_PER_CHUNK];
    
    public uint AddedVertexDataCount { get; private set; }
    public uint AddedIndicesCount { get; private set; }
    public uint AddedFacesCount { get; private set; }


    /// <summary>
    /// Adds a block face to the mesh.
    /// </summary>
    /// <param name="blockPos">Position of the block in the chunk (0-31 on all axis)</param>
    /// <param name="faceNormal">Which face we are adding</param>
    /// <param name="textureIndex">Index to the texture of this face (0-4095)</param>
    /// <param name="lightColor">Color of the light hitting this face</param>
    /// <param name="lightLevel">Amount of light that hits this face (0-31)</param>
    /// <param name="skyLightLevel">Amount of skylight hitting this face (0-31)</param>
    public void AddFace(Vector3i blockPos, BlockFaceNormal faceNormal, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        Vector3i vertPos1;
        Vector3i vertPos2;
        Vector3i vertPos3;
        Vector3i vertPos4;
        int normal = (int)faceNormal;
        switch (faceNormal)
        {
            case BlockFaceNormal.XPositive:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                break;
            case BlockFaceNormal.YPositive:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                break;
            case BlockFaceNormal.ZPositive:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                break;
            case BlockFaceNormal.XNegative:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                break;
            case BlockFaceNormal.YNegative:
                vertPos1 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z + 1);
                vertPos4 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z + 1);
                break;
            case BlockFaceNormal.ZNegative:
                vertPos1 = new Vector3i(blockPos.X + 1, blockPos.Y, blockPos.Z);
                vertPos2 = new Vector3i(blockPos.X, blockPos.Y, blockPos.Z);
                vertPos3 = new Vector3i(blockPos.X, blockPos.Y + 1, blockPos.Z);
                vertPos4 = new Vector3i(blockPos.X + 1, blockPos.Y + 1, blockPos.Z);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(faceNormal), faceNormal, "What face is THAT?!");
        }
        AddVertex(vertPos1, normal, 0, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos2, normal, 1, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos3, normal, 2, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddVertex(vertPos4, normal, 3, textureIndex, lightColor, lightLevel, skyLightLevel);
        AddIndices();
        AddedFacesCount++;
    }


    private void AddVertex(Vector3i vertexPos, int normal, int textureUvIndex, int textureIndex, Color9 lightColor, int lightLevel, int skyLightLevel)
    {
        // PositionIndex    =   0-133152. Calculated with x + Size * (y + Size * z).             = 18 bits.  18 bits.
        // TextureIndex     =   0-4095.                                                         = 12 bits.  30 bits.
        // LightColor       =   0-511.                                                          = 9 bits.   39 bits.
        // LightLevel       =   0-31.                                                           = 5 bits.   44 bits.
        // SkyLightLevel    =   0-31.                                                           = 5 bits.   49 bits.
        // Normal           =   0-5.                                                            = 3 bits.   52 bits.
        // UVIndex          =   0-3. Could be calculated dynamically based on gl_VertexID.      = 2 bits.   54 bits.
        // 10 bits leftover.
        
        int positionIndex = (vertexPos.X << 12) | (vertexPos.Y << 6) | vertexPos.Z;
        
        int lightColorValue = lightColor.Value;
        
        Logger.Debug($"Request add vert @ {vertexPos} -> {positionIndex} with normal {normal}");

        if (positionIndex < 0 || positionIndex > 133152)
            throw new ArgumentOutOfRangeException(nameof(positionIndex), positionIndex, "Position index out of range");
        
        if (lightColorValue < 0 || lightColorValue > 511)
            throw new ArgumentOutOfRangeException(nameof(lightColorValue), lightColorValue, "Light color value out of range");
        
        if (lightLevel < 0 || lightLevel > 31)
            throw new ArgumentOutOfRangeException(nameof(lightLevel), lightLevel, "Light level out of range");
        
        if (skyLightLevel < 0 || skyLightLevel > 31)
            throw new ArgumentOutOfRangeException(nameof(skyLightLevel), skyLightLevel, "Skylight level out of range");
        
        if (textureIndex < 0 || textureIndex > 4095)
            throw new ArgumentOutOfRangeException(nameof(textureIndex), textureIndex, "Texture index out of range");
        
        if (normal < 0 || normal > 5)
            throw new ArgumentOutOfRangeException(nameof(normal), normal, "Normal out of range");
        
        if (textureUvIndex < 0 || textureUvIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(textureUvIndex), textureUvIndex, "Texture UV index out of range");

        // NOTE: According to the OpenGL spec, vertex data should be 4-byte aligned. This means that since we cannot fit our vertex in 4 bytes, we use the full 8 bytes.
        // Compress all data to two 32-bit uints...
        uint data1 = 0b_00000000_00000000_00000000_00000000;
        uint data2 = 0b_00000000_00000000_00000000_00000000;
        int bitIndex1 = 0;
        int bitIndex2 = 0;
        
        positionIndex       .InjectUnsigned(ref data1, ref bitIndex1, 18);
        lightColorValue     .InjectUnsigned(ref data1, ref bitIndex1, 9);
        lightLevel          .InjectUnsigned(ref data1, ref bitIndex1, 5);
        textureIndex        .InjectUnsigned(ref data2, ref bitIndex2, 12);
        skyLightLevel       .InjectUnsigned(ref data2, ref bitIndex2, 5);
        normal              .InjectUnsigned(ref data2, ref bitIndex2, 3);
        textureUvIndex      .InjectUnsigned(ref data2, ref bitIndex2, 2);
        _vertexData[AddedVertexDataCount] = data1;
        _vertexData[AddedVertexDataCount + 1] = data2;
        AddedVertexDataCount += 2;
    }
    
    
    private void AddIndices()
    {
        uint offset = 4 * AddedFacesCount;
        _indices[AddedIndicesCount] = offset + 0;
        _indices[AddedIndicesCount + 1] = offset + 1;
        _indices[AddedIndicesCount + 2] = offset + 2;
        _indices[AddedIndicesCount + 3] = offset + 0;
        _indices[AddedIndicesCount + 4] = offset + 2;
        _indices[AddedIndicesCount + 5] = offset + 3;
        AddedIndicesCount += 6;
    }


    public ChunkMesh CreateMesh(Vector3i chunkPos)
    {
        // Create new arrays with the correct size, to avoid sending unused data to the GPU.
        uint[] vertices = new uint[AddedVertexDataCount];
        uint[] indices = new uint[AddedIndicesCount];
        Array.Copy(_vertexData, vertices, AddedVertexDataCount);
        Array.Copy(_indices, indices, AddedIndicesCount);
        
        return new ChunkMesh(chunkPos, vertices, indices);
    }


    public void Clear()
    {
        Array.Clear(_vertexData, 0, _vertexData.Length);
        Array.Clear(_indices, 0, _indices.Length);
        AddedVertexDataCount = 0;
        AddedIndicesCount = 0;
        AddedFacesCount = 0;
    }
}