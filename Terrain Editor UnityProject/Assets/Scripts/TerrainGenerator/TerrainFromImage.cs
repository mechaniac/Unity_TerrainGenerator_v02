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

    public void SetHeightFromTexture()
    {
        if (tileCountX == 0) InitializeFields();
        SetHeightFromSmallerTexture();
    }

    public void SetHeightFromBiggerTexture()
    {
        

        int xStop = TerrainMap.width > tileCountX ? tileCountX : TerrainMap.width;
        int zStop = TerrainMap.height > tileCountZ ? tileCountZ : TerrainMap.height;

        for (int z = 0, i = 0; z < zStop; z++)
        {

            for (int x = 0; x < xStop; x++, i++)
            {

                MyTerrain.tiles[i].height =  (TerrainMap.GetPixel(MyTerrain.tiles[i].xCoord, MyTerrain.tiles[i].zCoord).b);
            }
        }
    }

    public void SetHeightFromSmallerTexture()
    {
        Debug.Log($"Terrainmap width: {TerrainMap.width}, height: {TerrainMap.height}");
        for (int i = 0; i < MyTerrain.tiles.Length; i++)
        {
            int _x = MyTerrain.tiles[i].xCoord;
            int _z = MyTerrain.tiles[i].zCoord;
            if (_x < TerrainMap.width && _z < TerrainMap.height)
            {
                MyTerrain.tiles[i].height = (TerrainMap.GetPixel(MyTerrain.tiles[i].xCoord, MyTerrain.tiles[i].zCoord).b);
            }
        }
    }


}
