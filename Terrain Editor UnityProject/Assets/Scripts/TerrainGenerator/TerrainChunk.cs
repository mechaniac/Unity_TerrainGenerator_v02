using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    Mesh mesh;

    public C_Terrain MyTerrain;

    public int id;

    Vector3[] vertices;
    Vector2[] uv;

    int cutCount = 2;

    int verticesX;
    int verticesZ;
    int vertexCount;

    C_Tile[] tiles;
    MeshCollider collider;

    public int firstTileIndex;

    public int ChunkCoordX;
    public int ChunkCoordZ;


    private void Awake()
    {

    }

    public void InitializeTerrainChunk()
    {
        name = $"Chunk{id}: {ChunkCoordX}/{ChunkCoordZ}";
        CalculateVertexCount();

        vertices = new Vector3[vertexCount];


        mesh = GetComponent<MeshFilter>().mesh;
        collider = GetComponent<MeshCollider>();

        SetChunkVertices();

        AdjustGridOffsets(.6f);
        ApplyUVCoordinates();





        SetChunkTriangles();

        GenerateTilesPerChunk();

        //PrintChunkIndices(GetAllTilesOfChunk());
    }

    void CalculateVertexCount()
    {
        verticesX = MyTerrain.tilesPerChunkX * cutCount + MyTerrain.tilesPerChunkX + 1;
        verticesZ = MyTerrain.tilesPerChunkZ * cutCount + MyTerrain.tilesPerChunkZ + 1;

        firstTileIndex = MyTerrain.tilesPerChunk * id;

        vertexCount = verticesX * verticesZ;
    }

    void SetChunkVertices()
    {
        float vertexOffset = MyTerrain.tileSize / (cutCount + 1);

        for (int z = 0, i = 0; z < verticesZ; z++)
        {
            for (int x = 0; x < verticesX; x++, i++)
            {
                vertices[i] = new Vector3(x * vertexOffset, 0, z * vertexOffset);
            }
        }
        mesh.vertices = vertices;


    }

    void AdjustGridOffsets(float offset)    //incorporate innerOffset
    {
        for (int z = 0, _z = 0; z < verticesZ; z++)
        {
            if (z % 3 != 0)
            {
                _z++;
                float o = offset;
                if (_z % 2 != 0) o = -o;

                for (int x = 0; x < verticesX; x++)
                {
                    vertices[verticesX * z + x] += new Vector3(0, 0, o);
                }
            }

        }

        for (int x = 0, _x = 0; x < verticesX; x++)
        {
            if (x % 3 != 0)
            {
                _x++;
                float o = offset;
                if (_x % 2 != 0) o = -o;

                for (int z = 0; z < verticesZ; z++)
                {
                    vertices[x + verticesX * z] += new Vector3(o, 0, 0);
                }
            }
        }
        mesh.vertices = vertices;

    }

    void ApplyUVCoordinates()
    {
        uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = new Vector2((float)vertices[i].x / (MyTerrain.tilesPerChunkX * MyTerrain.tileSize), (float)vertices[i].z / (MyTerrain.tilesPerChunkZ * MyTerrain.tileSize));
        }
        mesh.uv = uv;

    }

    void SetChunkTriangles()
    {
        int[] triangles = new int[6 * verticesX * verticesZ];

        for (int z = 0, ti = 0; z < verticesZ - 1; z++)
        {
            for (int x = 0; x < verticesX - 1; x++, ti += 6)
            {
                int offsetFRomZ = z * verticesX;
                if ((((ti / 6) + 1) % 3 == 0 && z % 3 == 0) || ((ti / 6) % 3 == 0 && (z + 1) % 3 == 0))     //Set CornerTriangles CounterClockwise
                {

                    triangles[ti + 3] = triangles[ti] = offsetFRomZ + x;
                    triangles[ti + 4] = offsetFRomZ + x + verticesX;
                    triangles[ti + 2] = offsetFRomZ + x + 1;
                    triangles[ti + 1] = triangles[ti + 5] = offsetFRomZ + x + verticesX + 1;
                }
                else    //Set Regular Triangles
                {

                    triangles[ti] = offsetFRomZ + x;
                    triangles[ti + 1] = triangles[ti + 4] = offsetFRomZ + x + verticesX;
                    triangles[ti + 2] = triangles[ti + 3] = offsetFRomZ + x + 1;
                    triangles[ti + 5] = offsetFRomZ + x + verticesX + 1;
                }

            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void GenerateTilesPerChunk()
    {
        tiles = new C_Tile[MyTerrain.tilesPerChunkX * MyTerrain.tilesPerChunkZ];
        Vector3 startPosition = this.transform.position;
        float offset = MyTerrain.tileSize / 2;
        float ts = MyTerrain.tileSize;

        for (int z = 0, i = 0; z < MyTerrain.tilesPerChunkZ; z++)
        {
            for (int x = 0; x < MyTerrain.tilesPerChunkX; x++, i++)
            {
                C_Tile c = MyTerrain.tiles[firstTileIndex + i] = tiles[i] = Instantiate(MyTerrain.tilePrefab);
                c.transform.parent = transform;
                c.transform.position = startPosition + new Vector3(ts * x + offset, 0, ts * z + offset);
                c.MyChunk = this;

                int globalCoordX = x + ChunkCoordX * MyTerrain.tilesPerChunkX;
                int globalCoordZ = z + ChunkCoordZ * MyTerrain.tilesPerChunkZ;

                c.InitializeTile(globalCoordX, globalCoordZ, firstTileIndex + i);

            }
        }
    }

    public void UpdateVerticeGaps()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            AlignTopVerticesWithNeighbor(tiles[i]);
            AlignBottomVerticesWithNeighbor(tiles[i]);
            AlignRightVerticesWithNeighbor(tiles[i]);

            AlignLeftVerticesWithNeighbor(tiles[i]);
            AlignCornerVerticesWithNeighbours(tiles[i]);

            SetBorderVerticesHeight(Direction.Bottom, 0f);

        }
    }


    C_Tile[] GetAllTilesOfChunk()
    {
        C_Tile[] cT = new C_Tile[MyTerrain.tilesPerChunk];

        for (int i = 0; i < MyTerrain.tilesPerChunk; i++)
        {
            cT[i] = MyTerrain.tiles[firstTileIndex + i];
        }
        return cT;
    }

    void PrintTilesIndices(C_Tile[] t)
    {
        Debug.Log($"Chunk {id} holds :");
        for (int i = 0; i < t.Length; i++)
        {
            Debug.Log($" Tile: {t[i].index}");
        }
    }

    public bool IsTileInChunk(C_Tile t)
    {
        if (t.index >= firstTileIndex && t.index < firstTileIndex + MyTerrain.tilesPerChunk) return true;

        return false;
    }

    public int GetFirstVertexIndexFromTile(C_Tile t)
    {
        int relativeX = t.xCoord - ChunkCoordX * MyTerrain.tilesPerChunkX;
        int relativeZ = t.zCoord - ChunkCoordZ * MyTerrain.tilesPerChunkZ;

        int offsetFromX = relativeX * (cutCount + 1);
        int offsetFromZ = relativeZ * (cutCount + 1) * verticesX;

        //Debug.Log($"tile {t.index} has xCoord: {t.xCoord} and zCoord: {t.zCoord}");
        //Debug.Log($"tile {t.index} has offsetFromX: {offsetFromX} and offsetFromZ: {offsetFromZ}");
        //Debug.Log($"tile {t.index} has relativeX: {relativeX} and relativeZ: {relativeZ}");
        //Debug.Log($"chunk {this} has ChunkCoordX: {ChunkCoordX} and ChunkCoordZ: {ChunkCoordZ}");

        //Debug.Log($"index: {offsetFromX + offsetFromZ}");
        return offsetFromX + offsetFromZ;
    }

    public int[] GetCenterVerticeIdsOfTile(C_Tile t)
    {
        int[] v = new int[4];

        int startVertex = GetFirstVertexIndexFromTile(t);

        v[0] = startVertex + verticesX + 1;
        v[1] = v[0] + 1;

        v[2] = startVertex + 2 * verticesX + 1;
        v[3] = v[2] + 1;

        return v;
    }

    public int[] GetRightVerticeIdsOfTile(C_Tile t)
    {
        int[] v = new int[2];

        int startVertex = GetFirstVertexIndexFromTile(t);

        v[0] = startVertex + 3 + verticesX;
        v[1] = v[0] + verticesX;

        return v;
    }

    public int[] GetLeftVerticeIdsOfTile(C_Tile t)
    {
        int[] v = new int[2];

        int startVertex = GetFirstVertexIndexFromTile(t);

        v[0] = startVertex + verticesX;
        v[1] = v[0] + verticesX;


        return v;
    }

    public int[] GetBottomVerticesOfTile(C_Tile t)
    {
        int[] v = new int[2];

        int startVertex = GetFirstVertexIndexFromTile(t);


        v[0] = startVertex + 1;
        v[1] = v[0] + 1;

        return v;
    }


    public int[] GetTopVerticesOfTile(C_Tile t)
    {
        int[] v = new int[2];

        int startVertex = GetFirstVertexIndexFromTile(t);

        v[0] = startVertex + verticesX * 3 + 1;
        v[1] = v[0] + 1;

        return v;
    }

    public int[] GetCornerVerticesOfTile(C_Tile t)
    {
        int[] v = new int[4];

        int startVertex = GetFirstVertexIndexFromTile(t);

        v[0] = startVertex;                         //lowerLeft
        v[1] = startVertex + verticesX * 3 + 3;     //upperRight
        v[2] = startVertex + 3;                     //lowerRight
        v[3] = startVertex + verticesX * 3;           //upperLeft

        return v;
    }

    void AlignCornerVerticesWithNeighbours(C_Tile tile)
    {
        int[] v = GetCornerVerticesOfTile(tile);

        // Lower Left
        float lowerLeftCornerHeight = tile.height + tile.neighbourBottom.height + tile.neighbourLeft.height;
        if (tile.neighbourBottom.name == "Border Tile")
        {
            lowerLeftCornerHeight = lowerLeftCornerHeight / 3f;
        }
        else
        {
            lowerLeftCornerHeight += tile.neighbourBottom.neighbourLeft.height;
            lowerLeftCornerHeight = lowerLeftCornerHeight / 4f;
        }
        SetVertexHeight(v[0], lowerLeftCornerHeight);


        // Upper Right
        float upperRightCornerHeight = tile.height + tile.neighbourTop.height + tile.neighbourRight.height;
        if (tile.neighbourTop.name == "Border Tile")
        {
            upperRightCornerHeight = upperRightCornerHeight / 3f;
        }
        else
        {
            upperRightCornerHeight += tile.neighbourTop.neighbourRight.height;
            upperRightCornerHeight = upperRightCornerHeight / 4f;
        }
        SetVertexHeight(v[1], upperRightCornerHeight);


        // Lower Right
        float lowerRightCornerHeight = tile.height + tile.neighbourBottom.height + tile.neighbourRight.height;
        if (tile.neighbourRight.name == "Border Tile")
        {
            lowerRightCornerHeight = lowerRightCornerHeight / 3f;
        }
        else
        {
            lowerRightCornerHeight += tile.neighbourRight.neighbourBottom.height;
            lowerRightCornerHeight = lowerRightCornerHeight / 4f;
        }
        SetVertexHeight(v[2], lowerRightCornerHeight);

        // Upper Left
        float upperLeftCornerHeight = tile.height + tile.neighbourTop.height + tile.neighbourLeft.height;
        if (tile.neighbourTop.name == "Border Tile")
        {
            upperLeftCornerHeight = upperLeftCornerHeight / 3f;
        }
        else
        {
            upperLeftCornerHeight += tile.neighbourTop.neighbourLeft.height;
            upperLeftCornerHeight = upperLeftCornerHeight / 4f;
        }
        SetVertexHeight(v[3], upperLeftCornerHeight);
    }

    void AlignLeftVerticesWithNeighbor(C_Tile tile)
    {
        float setHeight = (tile.neighbourLeft.height + tile.height) / 2f;

        int[] v = GetLeftVerticeIdsOfTile(tile);

        for (int i = 0; i < v.Length; i++)
        {
            SetVertexHeight(v[i], setHeight);
        }
    }

    void AlignRightVerticesWithNeighbor(C_Tile tile)
    {
        float setHeight = (tile.neighbourRight.height + tile.height) / 2f;

        int[] v = GetRightVerticeIdsOfTile(tile);

        for (int i = 0; i < v.Length; i++)
        {
            SetVertexHeight(v[i], setHeight);
        }
    }

    void AlignTopVerticesWithNeighbor(C_Tile tile)
    {
        float setHeight = (tile.neighbourTop.height + tile.height) / 2f;

        int[] v = GetTopVerticesOfTile(tile);

        for (int i = 0; i < v.Length; i++)
        {
            SetVertexHeight(v[i], setHeight);
        }
    }

    void AlignBottomVerticesWithNeighbor(C_Tile tile)
    {
        float setHeight = (tile.neighbourBottom.height + tile.height) / 2f;

        int[] v = GetBottomVerticesOfTile(tile);

        for (int i = 0; i < v.Length; i++)
        {
            SetVertexHeight(v[i], setHeight);
        }
    }


    void SetBorderVerticesHeight(Direction dir, float height)
    {
        if (ChunkCoordZ == 0) { SetBottomBorderHeight(height); }

        if (ChunkCoordZ == MyTerrain.chunkCountZ - 1) { SetTopBorderHeight(height); }

        if (ChunkCoordX == 0) { SetLeftBorderHeight(height); }

        if (ChunkCoordX == MyTerrain.chunkCountX - 1) { SetRightBorderHeight(height); }
    }

    void SetBottomBorderHeight(float height)
    {
        for (int i = 0; i < verticesX; i++)
        {
            SetVertexHeight(i, height);

        }
    }

    void SetTopBorderHeight(float height)
    {
        for (int i = (verticesX * verticesZ) - verticesX; i < verticesX * verticesZ; i++)
        {
            SetVertexHeight(i, height);

        }
    }

    void SetLeftBorderHeight(float height)
    {
        for (int i = 0; i <= verticesX * verticesZ - verticesX; i += verticesX)
        {
            SetVertexHeight(i, height);
        }
    }

    void SetRightBorderHeight(float height)
    {
        for (int i = verticesX -1; i < verticesX * verticesZ; i += verticesX)
        {
            SetVertexHeight(i, height);
        }
    }

    public void OffsetVertexHeight(int i, float height)
    {
        vertices[i] += new Vector3(0, height, 0);
    }

    public void SetVertexHeight(int i, float height)
    {
        vertices[i] = new Vector3(vertices[i].x, height * MyTerrain.heightMultiplier, vertices[i].z);
    }

    public void SetVerticesToTileHeight(C_Tile t)
    {
        int[] centerVertices = GetCenterVerticeIdsOfTile(t);

        for (int i = 0; i < centerVertices.Length; i++)
        {
            SetVertexHeight(centerVertices[i], t.height);
        }
    }



    public void UpdateChunkMesh()
    {
        mesh.vertices = vertices;
        collider.sharedMesh = mesh;
        mesh.RecalculateNormals();
    }

}

public enum Direction
{
    Left,
    Top,
    Right,
    Bottom
}