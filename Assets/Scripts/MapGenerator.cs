﻿using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour 
{
	public enum SceneMode{Menu, Game};
	public enum DrawMode {NoiseMap, ColorMap, Mesh};
	public DrawMode drawMode;
	public SceneMode sceneMode;
	public Noise.NormalizeMode normalizeMode;
	public const int mapChunkSize = 241; 
	[Range(0,6)]
	public int testLOD;
	[Range(1,100)]
	public float noiseScale;
	[Range(1,10)]
	public int octaves;
	[Range(0,1)]
	public float persistence = .5f;
	[Range(1,10)]
	public float lacunarity;
	public float meshHeightMulti;
	public AnimationCurve meshHeightCurve;
	public int seed;
	public Vector2 offset;

	public bool useRandomSeed;
	public bool autoUpdate;
	public float mapSpeed;

	public TerrainType[] regions;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	void Start()
	{
		if(sceneMode == SceneMode.Menu)
		{
			useRandomSeed = true;
			DrawMapInEditor();
		}
		if(useRandomSeed)
		{
			SetSeed();
		}

	}

	void Update()
	{
		if(mapDataThreadInfoQueue.Count > 0)
		{
			for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
		if(meshDataThreadInfoQueue.Count > 0)
		{
			for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
		if(sceneMode == SceneMode.Menu)
		{
			offset.x -= Time.deltaTime*mapSpeed;
			DrawMapInEditor();
		}
	}

	public void DrawMapInEditor()
	{
		SetSeed();
		MapData mapData = GenerateMapData(Vector2.zero);
		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) 
		{
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (mapData.heightMap));
		} 
		else if (drawMode == DrawMode.ColorMap) 
		{
			display.DrawTexture (TextureGenerator.TextureFromColorMap (mapData.colorMap, mapChunkSize, mapChunkSize));
		} 
		else if (drawMode == DrawMode.Mesh) 
		{
			display.DrawMesh (MeshGenerator.GenerateTerrainMesh (mapData.heightMap,meshHeightMulti,meshHeightCurve, testLOD), TextureGenerator.TextureFromColorMap (mapData.colorMap, mapChunkSize, mapChunkSize));
		}
	}

	public void RequestMapData(Vector2 center,Action<MapData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MapDataThread(center,callback);
		};
		new Thread(threadStart).Start();
	}

	void MapDataThread(Vector2 center,Action<MapData> callback)
	{
		MapData mapData = GenerateMapData(center);
		lock(mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback,mapData));
		}
	}
	
	public void RequestMeshData(MapData mapData,Action<MeshData> callback, int lod)
	{
		ThreadStart threadStart = delegate {
			MeshDataThread (mapData, callback, lod);
		};

		new Thread (threadStart).Start ();
	}

	void MeshDataThread(MapData mapData,Action<MeshData> callback,int lod)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap,meshHeightMulti,meshHeightCurve,lod);
		lock(meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback,meshData));
		}
	}
	public MapData GenerateMapData(Vector2 center)
	{
		float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, center + offset,normalizeMode);

		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) 
		{
			for (int x = 0; x < mapChunkSize; x++) 
			{
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) 
				{
					if (currentHeight >= regions [i].height) 
					{
						colorMap [y * mapChunkSize + x] = regions [i].colour;
						
					}
					else
					{
						break;
					}
				}
			}
		}
		return new MapData(noiseMap,colorMap);
	}

	void SetSeed()
	{
		if(useRandomSeed)
		{
			seed = UnityEngine.Random.Range(1,100000000);
			if(sceneMode == SceneMode.Menu)
			{
				useRandomSeed = false;
			}
		}
	}

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;
		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
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

public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;
	public MapData(float[,] heightMap, Color[] colorMap)
	{
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}
}