using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.IO;

public struct TextureUV
{
	public int nameID;
	public float pixelStartX;
	public float pixelStartY;
	public float pixelStartX2;
	public float pixelEndY;
	public float pixelEndX;
	public float pixelStartY2;
	public float pixelEndX2;
	public float pixelEndY2;
}

public class TerrainGeneration : MonoBehaviour
{
    public int mRandomSeed;
    public int mWidth;
    public int mDepth;
    public int mMaxHeight;
    public Material mTerrainMaterial;

    private List<GameObject> realTerrains = new List<GameObject>();
    private NoiseAlgorithm terrainNoise;
    private List<TextureUV> terrainUVs;

    private int waterOrLava;
    private int sandOrObsidian;

    [SerializeField] private List<GameObject> prefabs;

	private List<TextureUV> LoadUVFromJSON(string inputFilePath)
    {
        string json = File.ReadAllText(inputFilePath);
        string[] splitJson = json.Split(' ');
        List<TextureUV> textureUVs = new List<TextureUV>();
        foreach (string line in splitJson) textureUVs.Add(JsonUtility.FromJson<TextureUV>(line));

        return textureUVs;
    }
    
    // code to get rid of fog from: https://forum.unity.com/threads/how-do-i-turn-off-fog-on-a-specific-camera-using-urp.1373826/
    // Unity calls this method automatically when it enables this component
    private void OnEnable()
    {
        // Add WriteLogMessage as a delegate of the RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering += BeginRender;
        RenderPipelineManager.endCameraRendering += EndRender;
    }
 
    // Unity calls this method automatically when it disables this component
    private void OnDisable()
    {
        // Remove WriteLogMessage as a delegate of the  RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering -= BeginRender;
        RenderPipelineManager.endCameraRendering -= EndRender;
    }
 
    // When this method is a delegate of RenderPipeline.beginCameraRendering event, Unity calls this method every time it raises the beginCameraRendering event
    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        // Write text to the console
        //Debug.Log($"Beginning rendering the camera: {camera.name}");
 
        if(camera.name == "Main Camera No Fog")
        {
            //Debug.Log("Turn fog off");
            RenderSettings.fog = false;
        }
         
    }
 
    void EndRender(ScriptableRenderContext context, Camera camera)
    {
        //Debug.Log($"Ending rendering the camera: {camera.name}");
        if (camera.name == "Main Camera No Fog")
        {
            //Debug.Log("Turn fog on");
            RenderSettings.fog = true;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
		// create a height map using perlin noise and fractal brownian motion
		// COOL SEEDS:
		// 60983
		//mRandomSeed = (int)(UnityEngine.Random.value * 100000);
        terrainNoise = new NoiseAlgorithm();
        terrainNoise.InitializeNoise(mWidth + 1, mDepth + 1, mRandomSeed);
        terrainNoise.InitializePerlinNoise(1.0f, 0.5f, 8, 2.0f, 0.5f, 0.01f, 1.0f);
        NativeArray<float> terrainHeightMap = new NativeArray<float>((mWidth+1) * (mDepth+1), Allocator.Persistent);

        terrainUVs = LoadUVFromJSON("uvinfo.dat");
		waterOrLava = CoinFlip() ? 4 : 3;
		sandOrObsidian = waterOrLava == 4 ? (CoinFlip() ? 12 : 13) : 5;

        // create the mesh and set it to the terrain variable
        int numChunks = 256;
        int mapWidth = (int)Mathf.Sqrt(numChunks);
        int depthLevel = -1;
		for (int i = 0; i < numChunks; i++)
        {
			realTerrains.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
			MeshRenderer meshRenderer = realTerrains[i].GetComponent<MeshRenderer>();
			MeshFilter meshFilter = realTerrains[i].GetComponent<MeshFilter>();
			meshRenderer.material = mTerrainMaterial;

			if (i % mapWidth == 0) depthLevel++;
			terrainNoise.setNoise(terrainHeightMap, mWidth * (i % mapWidth), mDepth * depthLevel);
			realTerrains[i].transform.position = new Vector3(mWidth * (i % mapWidth), 0, mDepth * depthLevel);
			meshFilter.mesh = GenerateTerrainMesh(terrainHeightMap, mWidth * (i % mapWidth), mDepth * depthLevel);
		}

        terrainHeightMap.Dispose();
        NoiseAlgorithm.OnExit();
    }

    private void Update()
    {
      
    }

    /// <summary>
    /// Flip a coin and return the result.
    /// </summary>
    private bool CoinFlip()
    {
        return UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f;
	}

    // create a new mesh with
    // perlin noise done blankly from Mathf.PerlinNoise in Unity
    // without any other features
    // makes a quad and connects it with the next quad
    // uses whatever texture the material is given
    public Mesh GenerateTerrainMesh(NativeArray<float> heightMap, float xPos, float zPos)
    {
		int width = mWidth + 1, depth = mDepth + 1;
        int height = mMaxHeight;
        int indicesIndex = 0;
        int vertexIndex = 0;
        int vertexMultiplier = 4; // create quads to fit uv's to so we can use more than one uv (4 vertices to a quad)

        Mesh terrainMesh = new Mesh();
        List<Vector3> vert = new List<Vector3>(width * depth * vertexMultiplier);
        List<int> indices = new List<int>(width * depth * 6);
        List<Vector2> uvs = new List<Vector2>(width * depth);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (x < width - 1 && z < depth - 1)
                {
                    // note: since perlin goes up to 1.0 multiplying by a height will tend to set
                    // the average around maxheight/2. We remove most of that extra by subtracting maxheight/2
                    // so our ground isn't always way up in the air
                    float y = heightMap[(x) * (width) + (z)] * height - (mMaxHeight/2.0f);
                    float useAltXPlusY = heightMap[(x + 1) * (width) + (z)] * height - (mMaxHeight/2.0f);
                    float useAltZPlusY = heightMap[(x) * (width) + (z + 1)] * height- (mMaxHeight/2.0f);
                    float useAltXAndZPlusY = heightMap[(x + 1) * (width) + (z + 1)] * height- (mMaxHeight/2.0f);
                    
                    vert.Add(new float3(x, y, z));
                    vert.Add(new float3(x, useAltZPlusY, z + 1)); 
                    vert.Add(new float3(x + 1, useAltXPlusY, z));  
                    vert.Add(new float3(x + 1, useAltXAndZPlusY, z + 1));

                    // add uv's based on height
                    // remember to give it all 4 sides of the image coords

                    // Water or Lava
                    if (y + mMaxHeight / 2f < mMaxHeight * 0.43f)
                    {
                        uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelStartX, terrainUVs[waterOrLava].pixelStartY));
                        uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelStartX, terrainUVs[waterOrLava].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelEndX, terrainUVs[waterOrLava].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelEndX, terrainUVs[waterOrLava].pixelStartY));
                    }
                    // Sand or Obsidian
                    else if (y + mMaxHeight / 2f < mMaxHeight * 0.45f)
                    {
                        uvs.Add(new Vector2(terrainUVs[sandOrObsidian].pixelStartX, terrainUVs[sandOrObsidian].pixelStartY));
                        uvs.Add(new Vector2(terrainUVs[sandOrObsidian].pixelStartX, terrainUVs[sandOrObsidian].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[sandOrObsidian].pixelEndX, terrainUVs[sandOrObsidian].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[sandOrObsidian].pixelEndX, terrainUVs[sandOrObsidian].pixelStartY));
                    }
                    // Grass. 1% chance to spawn a tree
                    else if (y + mMaxHeight / 2f < mMaxHeight * 0.54f)
                    {
                        uvs.Add(new Vector2(terrainUVs[2].pixelStartX, terrainUVs[2].pixelStartY));
                        uvs.Add(new Vector2(terrainUVs[2].pixelStartX, terrainUVs[2].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[2].pixelEndX, terrainUVs[2].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[2].pixelEndX, terrainUVs[2].pixelStartY));

                        if (UnityEngine.Random.Range(0f, 1f) < 0.01f)
                        {
                            Instantiate(CoinFlip() ? prefabs[0] : prefabs[1], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
                        }
                    }
                    // Rock. 1% chance to spawn ore
                    else if (y + mMaxHeight / 2f < mMaxHeight * 0.6f)
                    {
                        uvs.Add(new Vector2(terrainUVs[15].pixelStartX, terrainUVs[15].pixelStartY));
                        uvs.Add(new Vector2(terrainUVs[15].pixelStartX, terrainUVs[15].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[15].pixelEndX, terrainUVs[15].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[15].pixelEndX, terrainUVs[15].pixelStartY));

						if (UnityEngine.Random.Range(0f, 1f) < 0.01f)
						{
                            switch(UnityEngine.Random.Range(0, 6))
                            {
								case 0:
                                    Instantiate(prefabs[2], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
									break;
								case 1:
									Instantiate(prefabs[3], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
									break;
								case 2:
									Instantiate(prefabs[4], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
									break;
								case 3:
									Instantiate(prefabs[5], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
									break;
								case 4:
									Instantiate(prefabs[6], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
									break;
								case 5:
									Instantiate(prefabs[7], new Vector3(x + xPos, y, z + zPos), Quaternion.identity);
									break;
							}
						}
					}
                    // Snow
                    else
                    {
                        uvs.Add(new Vector2(terrainUVs[14].pixelStartX, terrainUVs[14].pixelStartY));
                        uvs.Add(new Vector2(terrainUVs[14].pixelStartX, terrainUVs[14].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[14].pixelEndX, terrainUVs[14].pixelEndY));
                        uvs.Add(new Vector2(terrainUVs[14].pixelEndX, terrainUVs[14].pixelStartY));
                    }
                    
                    // front or top face indices for a quad
                    //0,2,1,0,3,2
                    indices.Add(vertexIndex);
                    indices.Add(vertexIndex + 1);
                    indices.Add(vertexIndex + 2);
                    indices.Add(vertexIndex + 3);
                    indices.Add(vertexIndex + 2);
                    indices.Add(vertexIndex + 1);
                    indicesIndex += 6;
                    vertexIndex += vertexMultiplier;
                }
            }

        }
        
        // set the terrain var's for the mesh
        terrainMesh.vertices = vert.ToArray();
        terrainMesh.triangles = indices.ToArray();
        terrainMesh.SetUVs(0, uvs);
        
        // reset the mesh
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();
       
        return terrainMesh;
    }

}
