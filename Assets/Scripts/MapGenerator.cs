using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;


	public int mapWidth;
	public int mapHeight;
	[Range(0.0001f,100)]
	public float noiseScale;
	[Range(1,10)]
	public int octaves;
	[Range(0,1)]
	public float persistance;
	[Range(1,10)]
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool useRandomSeed;
	public bool autoUpdate;

	public TerrainType[] regions;

	public void GenerateMap()
	{
		if(useRandomSeed)
		{
			seed = Random.Range(1,100000000);
		}
		float[,] noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colourMap = new Color[mapWidth * mapHeight];
		for (int y = 0; y < mapHeight; y++) 
		{
			for (int x = 0; x < mapWidth; x++) 
			{
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) 
				{
					if (currentHeight <= regions [i].height) 
					{
						colourMap [y * mapWidth + x] = regions [i].colour;
						break;
					}
				}
			}
		}

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) 
		{
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (noiseMap));
		} 
		else if (drawMode == DrawMode.ColourMap) 
		{
			display.DrawTexture (TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
		} 
		else if (drawMode == DrawMode.Mesh) 
		{
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (noiseMap), TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
		}
	}
}

[System.Serializable]
public struct TerrainType 
{
	public string name;
	public float height;
	public Color colour;
}