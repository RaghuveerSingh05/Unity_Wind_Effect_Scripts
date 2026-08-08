using UnityEngine;
using System.Collections.Generic;

public class TreeLeafController : MonoBehaviour
{
    [Header("Leaf Settings")]
    public float windInfluence = 2f;
    public float activationThreshold = 0.1f;
    public int maxLeaves = 300;
    public float leafSize = 0.3f;
    
    [Header("Spawn Points")]
    public List<GameObject> spawnPoints = new List<GameObject>();
    
    [Header("Leaf Color")]
    public Color leafColor = new Color(0.2f, 0.6f, 0.1f);
    
    [Header("Auto Detection")]
    public bool autoFindTrees = true;
    public string treeTag = "Tree";
    
    private DynamicWindController windController;
    private List<TreeLeafData> trees = new List<TreeLeafData>();
    private Material leafMaterial;
    private Mesh leafMesh;
    
    [System.Serializable]
    public class TreeLeafData
    {
        public GameObject tree;
        public ParticleSystem particleSystem;
        public bool isActive;
        public float phase;
        public float influence;
    }
    
    void Start()
    {
        windController = FindObjectOfType<DynamicWindController>();
        
        if (windController == null)
        {
            Debug.LogError("DynamicWindController not found!");
            return;
        }
        
        CreateLeafMesh();
        CreateLeafMaterial();
        
        if (spawnPoints.Count > 0)
        {
            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    CreateLeafSystemAtSpawnPoint(spawnPoint);
                }
            }
        }
        else if (autoFindTrees)
        {
            FindAllTreesByTag();
        }
        
        windController.OnWindChanged.AddListener(OnWindChanged);
        windController.OnWindStrengthChanged.AddListener(OnWindStrengthChanged);
        
        OnWindStrengthChanged(windController.GetWindStrength());
    }
    
    void CreateLeafMesh()
    {
        leafMesh = new Mesh();
        leafMesh.name = "LeafMesh";
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(-0.3f, 0.2f, 0f),
            new Vector3(0f, 0.25f, 0f),
            new Vector3(0.3f, 0.2f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0.3f, -0.2f, 0f),
            new Vector3(0f, -0.25f, 0f),
            new Vector3(-0.3f, -0.2f, 0f),
            new Vector3(-0.5f, 0f, 0.02f),
            new Vector3(-0.3f, 0.2f, 0.02f),
            new Vector3(0f, 0.25f, 0.02f),
            new Vector3(0.3f, 0.2f, 0.02f),
            new Vector3(0.5f, 0f, 0.02f),
            new Vector3(0.3f, -0.2f, 0.02f),
            new Vector3(0f, -0.25f, 0.02f),
            new Vector3(-0.3f, -0.2f, 0.02f),
        };
        
        int[] triangles = new int[]
        {
            0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7,
            8, 10, 9, 8, 11, 10, 8, 12, 11, 8, 13, 12, 8, 14, 13, 8, 15, 14,
            0, 8, 1, 1, 8, 9, 1, 9, 2, 2, 9, 10, 2, 10, 3, 3, 10, 11,
            3, 11, 4, 4, 11, 12, 4, 12, 5, 5, 12, 13, 5, 13, 6, 6, 13, 14,
            6, 14, 7, 7, 14, 15, 7, 15, 0, 0, 15, 8,
        };
        
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0f, 0.5f), new Vector2(0.2f, 0.8f),
            new Vector2(0.5f, 1f), new Vector2(0.8f, 0.8f),
            new Vector2(1f, 0.5f), new Vector2(0.8f, 0.2f),
            new Vector2(0.5f, 0f), new Vector2(0.2f, 0.2f),
            new Vector2(0f, 0.5f), new Vector2(0.2f, 0.8f),
            new Vector2(0.5f, 1f), new Vector2(0.8f, 0.8f),
            new Vector2(1f, 0.5f), new Vector2(0.8f, 0.2f),
            new Vector2(0.5f, 0f), new Vector2(0.2f, 0.2f),
        };
        
        leafMesh.vertices = vertices;
        leafMesh.triangles = triangles;
        leafMesh.uv = uvs;
        leafMesh.RecalculateNormals();
        leafMesh.RecalculateBounds();
    }
    
    void CreateLeafMaterial()
    {
        leafMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        leafMaterial.SetFloat("_Surface", 1);
        leafMaterial.SetFloat("_Blend", 0);
        leafMaterial.SetFloat("_AlphaClip", 1);
        leafMaterial.SetFloat("_Cull", 0);
        leafMaterial.color = leafColor;
        leafMaterial.mainTexture = CreateLeafTexture();
    }
    
    Texture2D CreateLeafTexture()
    {
        Texture2D texture = new Texture2D(64, 64);
        texture.name = "LeafTexture";
        
        Color[] colors = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = (x - 32) / 32f;
                float dy = (y - 32) / 32f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist < 1f)
                {
                    float alpha = Mathf.Clamp01(1f - dist * 1.1f);
                    Color leafCol = new Color(leafColor.r, leafColor.g, leafColor.b, alpha);
                    colors[y * 64 + x] = leafCol;
                }
                else
                {
                    colors[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }
    
    void CreateLeafSystemAtSpawnPoint(GameObject spawnPoint)
    {
        if (spawnPoint == null) return;
        
        GameObject leafObj = new GameObject("FallingLeaves");
        leafObj.transform.parent = spawnPoint.transform;
        leafObj.transform.localPosition = Vector3.zero;
        leafObj.transform.localRotation = Quaternion.identity;
        
        ParticleSystem ps = leafObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 10f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(leafSize * 0.5f, leafSize * 1.5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.maxParticles = maxLeaves / Mathf.Max(1, trees.Count + 1);
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(3f, 0.5f, 3f);
        shape.position = Vector3.zero;
        
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        
        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.x = 0f;
        force.y = -0.5f;
        force.z = 0f;
        
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = 0f;
        velocity.z = 0f;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = leafMesh;
        renderer.material = leafMaterial;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        
        TreeLeafData data = new TreeLeafData();
        data.tree = spawnPoint;
        data.particleSystem = ps;
        data.isActive = false;
        data.phase = Random.Range(0f, 360f);
        data.influence = windInfluence + Random.Range(-0.3f, 0.3f);
        
        trees.Add(data);
        
        if (!Application.isPlaying)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    
    void FindAllTreesByTag()
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(treeTag);
        
        if (taggedObjects.Length == 0)
        {
            return;
        }
        
        foreach (GameObject tree in taggedObjects)
        {
            GameObject spawnPoint = new GameObject("AutoSpawnPoint");
            spawnPoint.transform.parent = tree.transform;
            Vector3 topPos = GetTreeTopPosition(tree);
            spawnPoint.transform.localPosition = topPos;
            CreateLeafSystemAtSpawnPoint(spawnPoint);
        }
    }
    
    Vector3 GetTreeTopPosition(GameObject tree)
    {
        float highestY = 0f;
        Vector3 position = tree.transform.position;
        
        Renderer[] renderers = tree.GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer renderer in renderers)
        {
            float rendererTop = renderer.bounds.max.y;
            if (rendererTop > highestY)
            {
                highestY = rendererTop;
            }
        }
        
        if (highestY == 0f)
        {
            highestY = position.y + 5f;
        }
        
        return new Vector3(0f, highestY - position.y, 0f);
    }
    
    void OnWindChanged(Vector3 direction, float strength)
    {
        if (!Application.isPlaying) return;
        
        Vector3 windForce = direction * strength;
        
        foreach (TreeLeafData data in trees)
        {
            if (data.particleSystem == null) continue;
            
            var force = data.particleSystem.forceOverLifetime;
            if (force.enabled)
            {
                float influence = data.influence * (0.8f + Mathf.Sin(Time.time * 0.3f + data.phase) * 0.2f);
                
                var x = force.x;
                x.constantMin = windForce.x * influence * 0.3f;
                x.constantMax = windForce.x * influence * 0.8f;
                force.x = x;
                
                var y = force.y;
                y.constantMin = -0.8f + (windForce.y * 0.2f);
                y.constantMax = -0.3f + (windForce.y * 0.3f);
                force.y = y;
                
                var z = force.z;
                z.constantMin = windForce.z * influence * 0.3f;
                z.constantMax = windForce.z * influence * 0.8f;
                force.z = z;
            }
            
            var velocity = data.particleSystem.velocityOverLifetime;
            if (velocity.enabled)
            {
                var turbX = velocity.x;
                turbX.constantMin = -strength * 0.3f;
                turbX.constantMax = strength * 0.3f;
                velocity.x = turbX;
                
                var turbZ = velocity.z;
                turbZ.constantMin = -strength * 0.3f;
                turbZ.constantMax = strength * 0.3f;
                velocity.z = turbZ;
            }
        }
    }
    
    void OnWindStrengthChanged(float strength)
    {
        if (!Application.isPlaying) return;
        
        bool shouldBeActive = strength > activationThreshold;
        
        foreach (TreeLeafData data in trees)
        {
            if (data.particleSystem == null) continue;
            
            if (shouldBeActive != data.isActive)
            {
                data.isActive = shouldBeActive;
                
                var emission = data.particleSystem.emission;
                emission.rateOverTime = shouldBeActive ? 20f : 0f;
            }
        }
    }
    
    public void AddSpawnPoint(GameObject spawnPoint)
    {
        if (!spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
            CreateLeafSystemAtSpawnPoint(spawnPoint);
        }
    }
    
    public void RemoveSpawnPoint(GameObject spawnPoint)
    {
        if (spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Remove(spawnPoint);
            for (int i = trees.Count - 1; i >= 0; i--)
            {
                if (trees[i].tree == spawnPoint)
                {
                    if (trees[i].particleSystem != null)
                    {
                        Destroy(trees[i].particleSystem.gameObject);
                    }
                    trees.RemoveAt(i);
                }
            }
        }
    }
    
    public void UpdateLeafColor(Color newColor)
    {
        leafColor = newColor;
        if (leafMaterial != null)
        {
            leafMaterial.color = leafColor;
        }
        leafMaterial.mainTexture = CreateLeafTexture();
    }
    
    void OnDestroy()
    {
        if (windController != null)
        {
            windController.OnWindChanged.RemoveListener(OnWindChanged);
            windController.OnWindStrengthChanged.RemoveListener(OnWindStrengthChanged);
        }
    }
}