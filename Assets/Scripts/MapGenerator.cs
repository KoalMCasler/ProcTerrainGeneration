using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	public const int mapChunkSize = 241; 
	[Range(0,6)]
	public int levelOfDetail;
	[Range(1,100)]
	public float noiseScale;
	[Range(1,10)]
	public int octaves;
	[Range(0,1)]
	public float persistance = .5f;
	[Range(1,10)]
	public float lacunarity;
	public float meshHeightMulti;
	public AnimationCurve meshHeightCurve;
	public int seed;
	public Vector2 offset;

	public bool useRandomSeed;
	public bool autoUpdate;

	public TerrainType[] regions;

	void Start()
	{
		GenerateMap();
	}

	public void GenerateMap()
	{
		if(useRandomSeed)
		{
			seed = Random.Range(1,100000000);
		}
		float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) 
		{
			for (int x = 0; x < mapChunkSize; x++) 
			{
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) 
				{
					if (currentHeight <= regions [i].height) 
					{
						colourMap [y * mapChunkSize + x] = regions [i].colour;
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
			display.DrawTexture (TextureGenerator.TextureFromColourMap (colourMap, mapChunkSize, mapChunkSize));
		} 
		else if (drawMode == DrawMode.Mesh) 
		{
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (noiseMap,meshHeightMulti,meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap (colourMap, mapChunkSize, mapChunkSize));
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