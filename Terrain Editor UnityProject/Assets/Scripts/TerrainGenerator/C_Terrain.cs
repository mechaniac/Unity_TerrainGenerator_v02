using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_Terrain : MonoBehaviour
{
    public int chunkCountX;
    public int chunkCountZ;

    public int tilesPerChunkX;
    public int tilesPerChunkZ;

    public int tilesPerChunk;

    public float tileSize = 5f;

    public TerrainChunk chunkPrefab;
    TerrainChunk[] chunks;

    public C_Tile tilePrefab;
    public C_Tile[] tiles;

    public C_Tile borderTile;

    public float heightMultiplier;

    TerrainFromImage TFI;

    private void Awake()
    {
        tilesPerChunk = tilesPerChunkX * tilesPerChunkZ;

        borderTile = Instantiate(tilePrefab);
        borderTile.transform.parent = transform;
        borderTile.name = "Border Tile";
        borderTile.height = 0f;
        InitializeTileArray();
        InitializeChunks();

        SetTileNeighbours();



        TFI = GetComponent<TerrainFromImage>();
        TFI.SetHeightFromTexture();


        //TFI.LogPixels();
        //TestTilesFunction();

        UpdateAllTiles();
        //int testX = 6;
        //int testZ = 4;

        //Debug.Log($"index of tile {testX}/{testZ} = {GetTileFromCoordinates(testX, testZ)}");
        //TestTilesFunction2();


    }

    void UpdateAllTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].MyChunk.SetVerticesToTileHeight(tiles[i]);
        }

        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].UpdateVerticeGaps();
        }


        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].UpdateChunkMesh();
        }
    }


    void TestTilesFunction()
    {
        C_Tile tile = tiles[917];
        tile.height = 4f;
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i].IsTileInChunk(tile))
            {
                int index = chunks[i].GetFirstVertexIndexFromTile(tile);

                int[] indices = chunks[i].GetCenterVerticeIdsOfTile(tile);

                for (int y = 0; y < indices.Length; y++)
                {
                    chunks[i].SetVertexHeight(indices[y], tile.height);
                }

                //chunks[i].OffsetVertexHeight(index, 1f);
                chunks[i].UpdateChunkMesh();
                //Debug.Log($"upped vertex {index} in tile {tile.index}");
            }


        }
    }
    void TestTilesFunction2()
    {
        C_Tile tile = tiles[1005];
        tile.height = 4f;
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i].IsTileInChunk(tile))
            {
                int index = chunks[i].GetFirstVertexIndexFromTile(tile);
                Debug.Log($"yes! index: {index}");
                chunks[i].SetVertexHeight(index, 3f);
                chunks[i].UpdateChunkMesh();
            }


        }
    }



    void InitializeChunks()
    {

        chunks = new TerrainChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++, i++)
            {
                TerrainChunk c = chunks[i] = Instantiate(chunkPrefab);
                c.id = i;
                c.transform.position = new Vector3(x * tileSize * tilesPerChunkX, 0, z * tileSize * tilesPerChunkZ);
                c.transform.parent = transform;
                c.MyTerrain = this;
                c.ChunkCoordX = x;
                c.ChunkCoordZ = z;
                c.InitializeTerrainChunk();
            }
        }
    }

    void SetTileNeighbours()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].neighbourBottom = GetTileFromCoordinates(tiles[i].xCoord, tiles[i].zCoord - 1);
            tiles[i].neighbourRight = GetTileFromCoordinates(tiles[i].xCoord + 1, tiles[i].zCoord);
            tiles[i].neighbourTop = GetTileFromCoordinates(tiles[i].xCoord, tiles[i].zCoord + 1);
            tiles[i].neighbourLeft = GetTileFromCoordinates(tiles[i].xCoord - 1, tiles[i].zCoord);
        }
    }



    void InitializeTileArray()
    {
        tiles = new C_Tile[chunkCountX * tilesPerChunkX * chunkCountZ * tilesPerChunkZ];
    }



    public C_Tile GetTileFromCoordinates(int x, int z)
    {
        //Debug.Log($"x: {x} z:{z}");

        //Debug.Log($"tilesperZ = {tilesPerChunkZ * chunkCountZ}");
        if (x < 0 || z < 0 || z >= tilesPerChunkZ * chunkCountZ || x >= tilesPerChunkX * chunkCountX) return borderTile;

        //in which Chunk are we
        int chunkCoordX = x / tilesPerChunkX;
        int chunkCoordZ = z / tilesPerChunkZ;

        int relativeX = x - chunkCoordX * tilesPerChunkX;
        int relativeZ = z - chunkCoordZ * tilesPerChunkZ;

        int ChunkId = GetChunkIdFromChunkCoordinates(chunkCoordX, chunkCoordZ);
        //Debug.Log($"chunkID: {ChunkId}");
        int firstTileOfChunkIndex = chunks[ChunkId].firstTileIndex;

        //Debug.Log($"chunkX: {chunkCoordX} cunkZ: {chunkCoordZ}");
        //Debug.Log($"relativeX: {relativeX} relativeZ: {relativeZ}");

        return tiles[firstTileOfChunkIndex + relativeZ * tilesPerChunkX + relativeX];
    }

    int GetChunkIdFromChunkCoordinates(int x, int z)
    {
        return chunkCountX * z + x;
    }

}
