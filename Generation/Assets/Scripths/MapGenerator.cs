﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode {NoiseMap, ColourMap, Mesh};
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    [Range(1,29)]
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;

    public int mapChunkSize = 420;

    public bool autoUpdate;

    public bool useFallOffMap;

    public TerrainType[] regions;

    float[,] falloffMap;

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize); 
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++) {
                if(useFallOffMap) {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x,y]);
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colourMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
    }

    void OnValidate()
    {
        if (mapWidth < 1) {
            mapWidth = 1;
        }
        if (mapHeight < 1) {
            mapHeight = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

}

[System.Serializable]
public struct TerrainType {
    public float height;
    public Color color;
    public string name;
}
