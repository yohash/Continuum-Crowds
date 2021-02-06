using UnityEngine;
using System.Collections.Generic;

public class TileMap
{
  [SerializeField] private MeshFilter mesh_filter;
  [SerializeField] private MeshRenderer mesh_renderer;
  [SerializeField] private MeshCollider mesh_collider;

  [SerializeField] private float tileSize = 1f;

  [SerializeField] private GameObject tileMap;

  private Vector3 _location;
  public Vector3 Location {
    get { return _location; }
    set {
      _location = value;
      tileMap.transform.position = value;
    }
  }

  // ***************************************************************************
  //    Tilemap
  // ***************************************************************************
  public TileMap()
  {
    tileMap = new GameObject($"TileMap", typeof(MeshFilter), typeof(MeshRenderer));

    mesh_filter = tileMap.GetOrAddComponent<MeshFilter>();
    mesh_renderer = tileMap.GetOrAddComponent<MeshRenderer>();
    mesh_collider = tileMap.GetOrAddComponent<MeshCollider>();

    Color c = Color.white;
    c.a = 0.7f;

    var mat = new Material(Shader.Find("Standard"));
    mat.SetColor("_Color", c);
    mat.SetFloat("_Mode", 3);
    mat.SetFloat("_Glossiness", 0);
    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    mat.SetInt("_ZWrite", 0);
    mat.DisableKeyword("_ALPHATEST_ON");
    mat.EnableKeyword("_ALPHABLEND_ON");
    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    mat.renderQueue = 3000;

    mesh_renderer.material = mat;
    mesh_renderer.sharedMaterials[0] = mat;
  }

  ~TileMap()
  {
    Object.Destroy(tileMap);
  }

  // ***************************************************************************
  //    Public
  // ***************************************************************************
  public TileMap Off() { tileMap.SetActive(false); return this; }
  public TileMap On() { tileMap.SetActive(true); return this; }

  public TileMap BuildTileMap(Texture2D texture, float[,] heightMap)
  {
    BuildTexture(texture);
    BuildMesh(heightMap);
    return this;
  }

  public TileMap BuildTexture(Texture2D tex)
  {
    mesh_renderer.sharedMaterials[0].mainTexture = tex;
    return this;
  }

  public TileMap BuildMesh(float[,] mat)
  {
    var xLength = mat.GetLength(0);
    var zLength = mat.GetLength(1);

    int numTiles = xLength * zLength;
    int numTris = numTiles * 2;

    int vSize_x = xLength + 1;
    int vSize_z = zLength + 1;
    int numVerts = vSize_x * vSize_z;

    // generate mesh data
    Vector3[] vertices = new Vector3[numVerts];
    Vector3[] normals = new Vector3[numVerts];
    Vector2[] uv = new Vector2[numVerts];

    int[] triangles = new int[numTris * 3];

    int x, z;
    for (z = 0; z < vSize_z; z++) {
      for (x = 0; x < vSize_x; x++) {
        float h;
        if (z >= zLength && x >= xLength) {
          h = mat[x - 1, z - 1];
        } else if (z >= zLength) {
          h = mat[x, z - 1];
        } else if (x >= xLength) {
          h = mat[x - 1, z];
        } else {
          h = mat[x, z];
        }
        vertices[z * vSize_x + x] = new Vector3(x * tileSize, h + .1f, z * tileSize);
        normals[z * vSize_x + x] = Vector3.up;
        uv[z * vSize_x + x] = new Vector2((float)x / vSize_x, (float)z / vSize_z);
      }
    }

    for (z = 0; z < zLength; z++) {
      for (x = 0; x < xLength; x++) {
        int squareIndex = z * xLength + x;
        int triOffset = squareIndex * 6;

        triangles[triOffset + 0] = z * vSize_x + x + 0;
        triangles[triOffset + 1] = z * vSize_x + x + vSize_x + 0;
        triangles[triOffset + 2] = z * vSize_x + x + vSize_x + 1;

        triangles[triOffset + 3] = z * vSize_x + x + 0;
        triangles[triOffset + 4] = z * vSize_x + x + vSize_x + 1;
        triangles[triOffset + 5] = z * vSize_x + x + 1;
      }
    }

    // create a new mesh and populate with data
    Mesh mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.normals = normals;
    mesh.uv = uv;

    // assign our mesh to our filter/renderer
    mesh_filter.mesh = mesh;
    return this;
  }

  public void BuildCircle(List<Vector3> vertices)
  {
    int numOfPoints = vertices.Count - 3;

    // instantiate other variables
    List<int> triangleList = new List<int>();
    List<Vector2> uvList = new List<Vector2>();
    List<Vector3> normalList = new List<Vector3>();
    // filler variables
    uvList.Add(Vector2.up);
    uvList.Add(Vector2.up);
    uvList.Add(Vector2.up);
    normalList.Add(Vector3.up);
    normalList.Add(Vector3.up);
    normalList.Add(Vector3.up);
    // Add triangle indices.
    triangleList.Add(0);
    triangleList.Add(1);
    triangleList.Add(2);
    int index = 3;
    for (int i = 0; i < numOfPoints; i++) {
      triangleList.Add(0);                      // Index of circle center.
      triangleList.Add(index - 1);
      triangleList.Add(index);
      index++;
      uvList.Add(Vector3.up);
      normalList.Add(Vector3.up);
    }
    Mesh mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangleList.ToArray();
    mesh.uv = uvList.ToArray();
    mesh.normals = normalList.ToArray();

    // assign our mesh to our filter/renderer
    mesh_filter.mesh = mesh;
    mesh_collider.sharedMesh = mesh;
  }

}
