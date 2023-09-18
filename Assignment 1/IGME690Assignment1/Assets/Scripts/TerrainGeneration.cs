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

// COOL SEEDS:
// 7581
// 59544
// 60983
// 59787
public class TerrainGeneration : MonoBehaviour
{
    public int mRandomSeed;
    public int mWidth;
    public int mDepth;
    public int mMaxHeight;
    public Material mTerrainMaterial;

    private GameObject realTerrain;
    private NoiseAlgorithm terrainNoise;
    private List<TextureUV> terrainUVs;

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
        mRandomSeed= (int)(UnityEngine.Random.value * 100000);
        terrainNoise = new NoiseAlgorithm();
        terrainNoise.InitializeNoise(mWidth + 1, mDepth + 1, mRandomSeed);
        terrainNoise.InitializePerlinNoise(1.0f, 0.5f, 8, 2.0f, 0.5f, 0.01f, 1.0f);
        NativeArray<float> terrainHeightMap = new NativeArray<float>((mWidth+1) * (mDepth+1), Allocator.Persistent);
        terrainNoise.setNoise(terrainHeightMap, 0, 0);
        terrainUVs = LoadUVFromJSON("uvinfo.dat");

		// create the mesh and set it to the terrain variable
		realTerrain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        realTerrain.transform.position = new Vector3(0, 0, 0);
        MeshRenderer meshRenderer = realTerrain.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = realTerrain.GetComponent<MeshFilter>();
        meshRenderer.material = mTerrainMaterial;
        meshFilter.mesh = GenerateTerrainMesh(terrainHeightMap);
        terrainHeightMap.Dispose();
        NoiseAlgorithm.OnExit();
    }

    private void Update()
    {
      
    }

    // create a new mesh with
    // perlin noise done blankly from Mathf.PerlinNoise in Unity
    // without any other features
    // makes a quad and connects it with the next quad
    // uses whatever texture the material is given
    public Mesh GenerateTerrainMesh(NativeArray<float> heightMap)
    {
		int waterOrLava = UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? 15 : 8;
		int stoneOrGrass = UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? 14 : 6;

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
					if (y + mMaxHeight / 2f < mMaxHeight * 0.4f)
                    {
						uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelStartX, terrainUVs[waterOrLava].pixelStartY));
						uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelStartX, terrainUVs[waterOrLava].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelEndX, terrainUVs[waterOrLava].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[waterOrLava].pixelEndX, terrainUVs[waterOrLava].pixelStartY));
					}
                    else if(y + mMaxHeight / 2f < mMaxHeight * 0.45f)
                    {
						uvs.Add(new Vector2(terrainUVs[12].pixelStartX, terrainUVs[12].pixelStartY));
						uvs.Add(new Vector2(terrainUVs[12].pixelStartX, terrainUVs[12].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[12].pixelEndX, terrainUVs[12].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[12].pixelEndX, terrainUVs[12].pixelStartY));
					}
                    else if(y + mMaxHeight / 2f < mMaxHeight * 0.6f)
                    {
						uvs.Add(new Vector2(terrainUVs[stoneOrGrass].pixelStartX, terrainUVs[stoneOrGrass].pixelStartY));
						uvs.Add(new Vector2(terrainUVs[stoneOrGrass].pixelStartX, terrainUVs[stoneOrGrass].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[stoneOrGrass].pixelEndX, terrainUVs[stoneOrGrass].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[stoneOrGrass].pixelEndX, terrainUVs[stoneOrGrass].pixelStartY));
					}
                    else
                    {
						uvs.Add(new Vector2(terrainUVs[13].pixelStartX, terrainUVs[13].pixelStartY));
						uvs.Add(new Vector2(terrainUVs[13].pixelStartX, terrainUVs[13].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[13].pixelEndX, terrainUVs[13].pixelEndY));
						uvs.Add(new Vector2(terrainUVs[13].pixelEndX, terrainUVs[13].pixelStartY));
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
