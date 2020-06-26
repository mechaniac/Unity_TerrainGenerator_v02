using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(C_Terrain))]
public class TerrainFromImage : MonoBehaviour
{
    C_Terrain MyTerrain;

    public Texture2D TerrainMap;

    int tileCountX;
    int tileCountZ;

    private void Awake()
    {
        


    }

    void InitializeFields()
    {
        if(MyTerrain == null) MyTerrain = GetComponent<C_Terrain>();
        tileCountX = MyTerrain.chunkCountX * MyTerrain.tilesPerChunkX;
        tileCountZ = MyTerrain.chunkCountZ * MyTerrain.tilesPerChunkZ;
    }


    public void LogPixels()
    {
        for (int x = 0; x < TerrainMap.width; x++)
        {
            for (int z = 0; z < TerrainMap.height; z++)
            {
                Debug.Log($"pixel: {TerrainMap.GetPixel(x, z).b}");
            }
        }
    }


    public void SetHeightFromMap()
    {
        if (tileCountX == 0) InitializeFields();

        int xStop = TerrainMap.width > tileCountX ? tileCountX : TerrainMap.width;
        int zStop = TerrainMap.height > tileCountZ ? tileCountZ : TerrainMap.height;

        for (int z = 0, i = 0; z < zStop; z++)
        {

            for (int x = 0; x < xStop; x++, i++)
            {

                MyTerrain.tiles[i].height = 3 * (TerrainMap.GetPixel(MyTerrain.tiles[i].xCoord, MyTerrain.tiles[i].zCoord).b);
            }
        }
    }


}
