using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tt : MonoBehaviour
{
    
    public AnimationCurve[] scales;
    public float width = 1;
    public float height = 1;

    public int xAxisSquares = 128;
    public int zAxisSquares = 128;
    public GameObject player;
    
    public ComputeShader cs;
    Dictionary<Vector2Int, Chunk> chunki2 = new Dictionary<Vector2Int, Chunk>();
    public int rendered = 10;
    Vector2Int playerChunk = new Vector2Int(10,10);
     bool doIt = false;

    public void FixedUpdate(){
            if(Input.GetKeyDown("r") || doIt){
                spawnam = true;
                if(pregled){
                    doIt = true;
            }else{
                doIt = false;
                var tempArr = transform.GetComponentInChildren<Transform>();
                foreach (Transform item in tempArr)
                {
                    Destroy(item.gameObject);
                }
                player.transform.position = new Vector3(player.transform.position.x, 500, player.transform.position.z);
                playerChunk = new Vector2Int(10,10);
                chunki2.Clear();
                print("hello");
                spawnam = false;
                pregled = false;
                Start();
            }
        }
        int x = Mathf.RoundToInt(player.transform.position.x/(xAxisSquares));
        int y = Mathf.RoundToInt(player.transform.position.z/(zAxisSquares));
        if ((x != playerChunk.x || y != playerChunk.y) && !spawnam && !pregled){
            playerChunk.x = x;
            playerChunk.y = y;
            water.transform.position = new Vector3(player.transform.position.x-3000, 44, player.transform.position.z-3000);
            StartCoroutine(spawnChunk(rendered, x, y));
            
           
        }else{
            if(!pregled && !spawnam){
                StartCoroutine(checkChunks());
            }
            
        }
        
    }
    bool pregled = false;
    IEnumerator checkChunks(){
        pregled = true;
        //a temporary dictionary is created so we can change the data, all the entries are copied to it from the one that is going to be changed
        Dictionary<Vector2Int, Chunk> chunki3 = new Dictionary<Vector2Int, Chunk>();
        foreach (var item in chunki2)
        {
            chunki3.Add(item.Key, item.Value);
        }
        //checks the distance of each chunk in the dictionary to the player, if its far enough it gets removed
        //otherwise it checks if the chunks quality needs to be lowered
        int yieldCheck = 0;
        foreach (var item in chunki2)
        {
            float dl = (player.transform.position - item.Value.g.transform.position).magnitude;
            
            if (Mathf.RoundToInt(dl/2)/xAxisSquares > rendered/2f){
                Destroy(item.Value.g);
                chunki3.Remove(item.Key);
            }else{
                int temp = Mathf.Clamp(Mathf.RoundToInt(dl/(xAxisSquares*width*2f))-1,0,6);
                yieldCheck += 36 - temp*temp;
                if (yieldCheck >= 100){
                    yield return null;
                    yieldCheck = 0;
                }
                item.Value.updateLOD(temp);


            }
        }
        //the changed data is transfered to the original dictionary
        chunki2.Clear();
        foreach (var item in chunki3)
        {
            chunki2.Add(item.Key, item.Value);
        }
        pregled = false;
        yield return null;
    }
    bool prvic = true;
    bool spawnam = false;
    //function spawnChunk is created as a coroutine so we reduce lag spikes when new chunks spawn
    //at the start all the chunks are generated without yield as the player wont notice it going into the game
    IEnumerator spawnChunk(int rendered, int x, int y){
        spawnam = true;
        int yieldCheck = 0;
        for (int i = -rendered; i < rendered; i++)
        {
            for (int j = -rendered; j < rendered; j++)
            {
                
                /*it checks if if the chunk already exists
                if not we create a new chunk and store it and the intVector of its position in the grid and not world position*/
                Vector2Int check = new Vector2Int(x +i, y+j);
                if(!chunki2.ContainsKey(check)){
                    Vector3 pos = new Vector3(check.x*xAxisSquares*width, 0, check.y*zAxisSquares*height);
                    float dl = (player.transform.position - pos).magnitude;
                    //if the chunk is too far we skip it
                    if (Mathf.RoundToInt(dl/2)/xAxisSquares > rendered/2f){
                        continue;
                    }
                    GameObject temp = Instantiate(anchor, transform);
                    temp.transform.position = pos;
                    int renderDistance = Mathf.Clamp(Mathf.Clamp(Mathf.RoundToInt(dl),0,7)-1,0,6);
                    Chunk ttemp = new Chunk((int)(xAxisSquares),
                        (int)(zAxisSquares), width,
                        height, terrainMaterial,
                        temp, cs, seed);
                    chunki2.Add(new Vector2Int(check.x,check.y), ttemp);
                    yieldCheck += 36-renderDistance*renderDistance;
                    if(!prvic){
                        if(yieldCheck >=100){
                            yield return null;
                            yieldCheck = 0;
                        }
                        
                    }
                    ttemp.updateLOD(renderDistance);
                    
                    
                }
            }
            
            
        }
        prvic = false;
        spawnam = false;
        yield return null;
        
    }

    public Material terrainMaterial;
    public GameObject anchor;
    public Material waterMat;
    GameObject water;
    public static int[][] test = new int[7][];
    public static Vector3[][] pozicije = new Vector3[7][];
    //we pregenerate the triangles and positions for use in meshes
    //we generate one array for each LOD level
    //the water is created, the code is more complex than it needs to be because at first the mesh was more detailed so the shader could manipulate the vertices
    //a different approach to water was later taken
    public Vector2Int seed = new Vector2Int(0,0);
    public void Start(){
        seed.x = Random.Range(100,1000000);
        seed.y = Random.Range(100,1000000);

        for (int i = 0; i < test.Length; i++)
        {
            test[i] = tris2(xAxisSquares, zAxisSquares, i);
            pozicije[i] = Pozicije(xAxisSquares, zAxisSquares, i);
        }
        water = Instantiate(anchor, transform);
        MeshRenderer meshRenderer2 = water.AddComponent<MeshRenderer>();
        meshRenderer2.sharedMaterial = waterMat;
        MeshFilter meshFilter2 = water.AddComponent<MeshFilter>();
        int a = 2;
        Vector3[] verti = new Vector3[a*a];
        for (int y = 0; y < a; y++)
            {
                for (int x = 0; x < a; x++)
                {
                    verti[y*a + x] = new Vector3(1*x, -35, 1*y);
                }
                
            }
        int[] tris = new int[6*(a-1)*(a-1)];
        int vertexCount = 0;
        for(int y = 0; y < a-1; y++){
            for (int x = 0; x < a-1; x++)
            {
                tris[vertexCount] = y*a+x;
                tris[vertexCount+2] = y*a+x + 1;
                tris[vertexCount+1] = y*a+x + a + 1;
                tris[vertexCount+3] = y*a+x;
                tris[vertexCount+5] = y*a+a + 1 + x;
                tris[vertexCount+4] = y*a+ a +x;

                vertexCount+=6;
            }
        }
        Mesh mesh2 = new Mesh();
        
        mesh2.vertices = verti;
        mesh2.triangles = tris;
        meshFilter2.mesh = mesh2;
        water.transform.localScale = new Vector3(6000f,1,6000f);
    }
    public Vector2Int updateLOD(int ostkx, int ostky, int quality){
        quality = Mathf.Clamp(quality,0,6);
        int stkx = (int)(ostkx/Mathf.Pow(2, quality));
        int stky = (int)(ostky/Mathf.Pow(2, quality));
        return new Vector2Int(stkx, stky);
    }
    public int[] tris2(int xos, int yos, int quality){
        Vector2Int temp = updateLOD(xos, yos, quality);
        xos = temp.x;
        yos = temp.y;
        xos +=1;
        yos +=1; 
        int[] verti = new int[6*(xos-1)*(yos-1)];
        int vertexCount = 0;
        for(int y = 0; y < yos-1; y++){
            for (int x = 0; x < xos-1; x++)
            {
                
                verti[vertexCount] = y*xos+x;
                verti[vertexCount+2] = y*xos+x + 1;
                verti[vertexCount+1] = y*xos+x + xos + 1;
                verti[vertexCount+3] = y*xos+x;
                verti[vertexCount+5] = y*xos+xos + 1 + x;
                verti[vertexCount+4] = y*xos+ xos +x;

                vertexCount+=6;
            }
        }
        return verti;
    }
    public Vector3[] Pozicije(int stx, int sty, int quality){
        Vector2Int temp = updateLOD(stx, sty, quality);
        stx = temp.x;
        sty = temp.y;
        stx +=1;
        sty +=1;
        Vector3[] inVisine = new Vector3[stx*sty];
        for (int i = 0; i < stx; i++)
        {
            for (int j = 0; j < sty; j++)
            {
                inVisine[j*stx + i] = new Vector3(i,0,j);
            }
        }
        return inVisine;
    }




    
}

public class Chunk{
    float width;
    float height;
    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    public GameObject g;
    Material mat;
    int xAxisSquares;
    int zAxisSquares;
    int oxAxisSquares;
    int ozAxisSquares;
    float oWidth;
    float oHeight;
    //old quality equals -1 so it doesnt interfere at the start before the mesh is generated
    public int oldQuality = -1;
    public ComputeShader cs;
    Vector2Int seed = new Vector2Int(0,0);


    //all the variables are set but the mesh is not generated, it is generated at a later stage by calling the function updateLOD
    /*
    stkx and stky: the number of squares on the x and z axis per each chunk
    vkx and vky: the x and z size of each square
    mat is the material
    g is the GameObject for this chunk
    */
    public Chunk(int _xAxisSquares, int _zAxisSquares, float _width, float _height, Material _mat, GameObject _g, ComputeShader _cs, Vector2Int _seed){
        xAxisSquares = _xAxisSquares;
        zAxisSquares = _zAxisSquares;
        oxAxisSquares = _xAxisSquares;
        ozAxisSquares = _zAxisSquares;
        width = _width;
        height = _height;
        oWidth = _width;
        oHeight = _height;
        mat = _mat;
        g = _g;
        cs = _cs;
        seed = _seed;
    }
    //function is called from the outside and it checks and clamps the quality
    //if the quality is different the chunk is regenerated
    //the renderDistance calculation is added for the island generation so chunks outside dont generate high resolution meshes
    public void updateLOD(int quality){
        int renderDistance = Mathf.Clamp(Mathf.Clamp(Mathf.RoundToInt((g.transform.position/256).magnitude),0,7)-1,0,6);
        if(renderDistance > 5){
            quality = renderDistance;
        }
        quality = Mathf.Clamp(quality,0,6);
        if (oldQuality != quality){
            oldQuality = quality;
            width = oWidth*Mathf.Pow(2, quality);
            height = oHeight*Mathf.Pow(2, quality);
            xAxisSquares = (int)(oxAxisSquares/Mathf.Pow(2, quality));
            zAxisSquares = (int)(ozAxisSquares/Mathf.Pow(2, quality));
            ustvariChunk(quality);
        }
    }
    public void ustvariChunk(int quality){
        if (!(meshRenderer != null)){    
            meshRenderer = g.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mat;
            meshFilter = g.AddComponent<MeshFilter>();
        }
        Vector2[] uvs = new Vector2[(xAxisSquares+1)*(zAxisSquares+1)];
        for (int i = 0; i < xAxisSquares+1; i++)
        {
            for (int j = 0; j < zAxisSquares+1; j++)
            {
                uvs[j*(xAxisSquares+1) + i] = new Vector2(i*width,j*height);
            }
        }

        mesh = new Mesh();
        
        mesh.vertices = vis(xAxisSquares,zAxisSquares, quality);
        mesh.triangles = tt.test[quality];
        
        mesh.uv = uvs;

        //180 is not accurate and was just a quick and rough estimate of the max height based on the combination of the noise functions
        //change this to automatic calculation to have amplitude adjustment
        mat.SetFloat("minHeight", 0);
        mat.SetFloat("maxHeight", 180);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        GameObject.Destroy(g.GetComponent<MeshCollider>());
        g.AddComponent<MeshCollider>();
        /*if (quality <= 5){
            if(!(g.GetComponent<MeshCollider>() != null)){
                g.AddComponent<MeshCollider>();
            }
        }else{
            if(g.GetComponent<MeshCollider>() != null){
                GameObject.Destroy(g.GetComponent<MeshCollider>());
            }
        }*/
        //this is not used in the island generation as its not needed
        
        
    }
    
    /*
    based on the quality we take a pregenerated array of vectors which we pass into the
    compute shader along with the needed variables

    */
    public Vector3[] vis(int stx, int sty, int quality)
    {
        stx +=1;
        sty +=1;
        Vector3[] inVisine = tt.pozicije[quality];
        Vector3[] outVisine = new Vector3[stx*sty];

        ComputeBuffer cb  = new ComputeBuffer(inVisine.Length, 12);
        cb.SetData(inVisine);
        cs.SetInt("vkx", xAxisSquares);
        cs.SetFloat("xpos", g.transform.position.x + seed.x);
        cs.SetFloat("zpos", g.transform.position.z + seed.y);
        cs.SetFloat("seedx", seed.x);
        cs.SetFloat("seedy", seed.y);
        cs.SetFloat("velikost", width);
        cs.SetFloat("razdalja", 400000);
        int kernel = cs.FindKernel("CSMain");
        cs.SetBuffer(kernel, "Result", cb);
        cs.Dispatch(kernel, (int)Mathf.Ceil(inVisine.Length/32.0f), 1, 1);
        cb.GetData(outVisine);
        cb.Dispose();
        return outVisine;
    }
}