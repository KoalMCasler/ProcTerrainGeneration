using UnityEngine;
using System.Collections;

public static class Noise 
{
	public enum NormalizeMode{ local, Global};
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode mode)
	{
		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random (seed);
		Vector2[] octaveOffsets = new Vector2[octaves];
		float amplitude = 1;
		float frequency = 1;
		float maxHeight = 0;

		for (int i = 0; i < octaves; i++) 
		{
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) - offset.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);
			maxHeight += amplitude;
			amplitude *= persistence;
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++) 
		{
			for (int x = 0; x < mapWidth; x++) 
			{
				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++) 
				{
					float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y-halfHeight + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1; //Corrects unity perlin
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistence;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxNoiseHeight) 
				{
					maxNoiseHeight = noiseHeight;
				} else if (noiseHeight < minNoiseHeight) 
				{
					minNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++) 
		{
			for (int x = 0; x < mapWidth; x++) 
			{
				if(mode == NormalizeMode.local)
				{
					noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
				}
				else if(mode == NormalizeMode.Global)
				{
					float normalizedHeight = (noiseMap[x,y]+1)/(maxHeight);
					noiseMap[x,y] = Mathf.Clamp(normalizedHeight,0,int.MaxValue);
				}
			}
		}

		return noiseMap;
	}

}
