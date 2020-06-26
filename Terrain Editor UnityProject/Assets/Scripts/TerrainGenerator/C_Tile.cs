using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class C_Tile : MonoBehaviour
{
    public int xCoord;
    public int zCoord;
    public float height = 0;

    public TerrainChunk MyChunk;
    public int index;

    public C_Tile neighbourBottom;
    public C_Tile neighbourRight;
    public C_Tile neighbourTop;
    public C_Tile neighbourLeft;

    Text text;
    void Start()
    {
        
    }

    public void InitializeTile(int x, int z, int i)
    {
        xCoord = x;
        zCoord = z;
        index = i;
        height = 0;
        SetText();
        name = $"Tile{index}: {xCoord}/{zCoord}";
    }

    void SetText()
    {
        if (text == null) text = GetComponentInChildren<Text>();

        text.text = $" {xCoord} / {zCoord} \n {index} \n {height}";

    }

    public void SetHeight(int height)
    {
        this.height = height;
        SetText();
    }
}
