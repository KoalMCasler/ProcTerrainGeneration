using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.Rendering.VirtualTexturing;

public class EndlessTerrain : MonoBehaviour 
{
	const float updateThreshold = 25f;
	const float sqrUpdateThreshold	= updateThreshold * updateThreshold;
	public LODInfo[] detailLevels;
	public static float maxViewDst;
	public Transform viewer;
	static MapGenerator mapGenerator;
	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	int chunkSize;
	int chunksVisibleInViewDst;
	public Material mapMat;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() 
	{
		mapGenerator = FindObjectOfType<MapGenerator>();
		maxViewDst = detailLevels[detailLevels.Length-1].visibleDistance;
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
		UpdateVisibleChunks();
	}

	void Update() 
	{
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);
		if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrUpdateThreshold)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}
		
	void UpdateVisibleChunks() 
	{

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) 
		{
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();
			
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) 
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) 
			{
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					if (terrainChunkDictionary [viewedChunkCoord].IsVisible ()) {
						terrainChunksVisibleLastUpdate.Add (terrainChunkDictionary [viewedChunkCoord]);
					}
				} else {
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform,mapMat,detailLevels));
				}

			}
		}
	}

	public class TerrainChunk 
	{
		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		LODInfo[] detailLevels;
		LODMesh[] lODMeshes;
		MapData mapData;
		bool hasMapData;
		int prevLodIndex = -1;
		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material, LODInfo[] detailLevels) 
		{
			position = coord * size;
			bounds = new Bounds(position,Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);
			this.detailLevels = detailLevels;
			meshObject = new GameObject("TerrainChunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			meshRenderer.material = material;
			SetVisible(false);
			lODMeshes = new LODMesh[detailLevels.Length];
			for(int i = 0; i< detailLevels.Length; i++)
			{
				lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}
			mapGenerator.RequestMapData(position,OnMapDataReceived);
		}

		void OnMapDataReceived(MapData mapData)
		{
			this.mapData = mapData;
			hasMapData = true;
			Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;
			UpdateTerrainChunk();
		}

		void OnMeshDataReceived(MeshData meshData)
		{
			meshFilter.mesh = meshData.CreateMesh();
		}

		public void UpdateTerrainChunk() 
		{
			if(hasMapData)
			{
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if(visible)
				{
					int lodIndex = 0;
					for(int i = 0; i < detailLevels.Length-1; i++)
					{
						if(viewerDstFromNearestEdge > detailLevels[i].visibleDistance)
						{
							lodIndex = i + 1;
						}
						else
						{
							break;
						}
					}
					if(lodIndex != prevLodIndex)
					{
						LODMesh lODMesh = lODMeshes[lodIndex];
						if(lODMesh.hasMesh)
						{
							prevLodIndex = lodIndex;
							meshFilter.mesh = lODMesh.mesh;
						}
						else if(!lODMesh.hasRequestedMesh)
						{
							lODMesh.RequestMesh(mapData);
						}
					}
				}

				SetVisible(visible);
			}
		}

		public void SetVisible(bool visible) 
		{
			meshObject.SetActive (visible);
		}

		public bool IsVisible() 
		{
			return meshObject.activeSelf;
		}

	}

	class LODMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;
		public LODMesh(int levelOfDetail, System.Action updateCallback)
		{
			lod = levelOfDetail;
			this.updateCallback = updateCallback;
		}

		void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;

			updateCallback();
		}
		public void RequestMesh(MapData mapData)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, OnMeshDataReceived,lod);
		}
	}
	[System.Serializable]
	public struct LODInfo
	{
		public int lod;
		public float visibleDistance;
	}
}
