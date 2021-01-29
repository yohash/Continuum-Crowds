using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///  An example of the use case from OVerlord, to make a tilemap
///  that conformed to terrain and would display a texture
//private void setTileMapToMatrix(Vector2 baseCorner, Vector2[,] mat)
//{
//  GameObject temp = ObjectPool.S.GetPooledDebugTool_TileMap();
//  TileMap tm = temp.GetComponent<TileMap>();
//  float[,] map = NavSystem.S.GetRangeOfHeightData(baseCorner, new Vector2(mat.GetLength(0), mat.GetLength(1)));
//  tm.BuildMesh(Vector2.zero, map);

//  Texture2D hmap = new Texture2D(map.GetLength(0), map.GetLength(1));

//  float mm = vect2MatrixMax(mat);
//  if (mm == 0f) { mm = 1f; }

//  for (int n = 0; n < map.GetLength(0); n++) {
//    for (int m = 0; m < map.GetLength(1); m++) {
//      Color c = new Color(((mat[n, m].x) / mm) / 2f + 0.5f, 0f, ((mat[n, m].y) / mm) / 2f + 0.5f, 0.8f);
//      hmap.SetPixel(n, m, c);
//    }
//  }
//  hmap.Apply();
//  hmap.filterMode = FilterMode.Point;
//  tm.BuildTexture(hmap);
//  tm.transform.position = new Vector3(baseCorner.x, 0f, baseCorner.y);
//  tm.gameObject.SetActive(true);
//  tm.GetComponent<DisableAfterNsec>().NSeconds = CC_VelocityFieldUpdateTime * 1.25f;
//}


public class TileMap : MonoBehaviour
{
  public MeshFilter mesh_filter;
  public MeshRenderer mesh_renderer;
  public MeshCollider mesh_collider;

  public Material mat;

  public bool enableProceduralMaterial = false;

  int mapX;
  int mapZ;

  public float tileSize = 1f;

  void Awake()
  {
    mesh_filter = gameObject.GetOrAddComponent<MeshFilter>();
    mesh_renderer = gameObject.GetOrAddComponent<MeshRenderer>();
    mesh_collider = gameObject.GetOrAddComponent<MeshCollider>();

    if (enableProceduralMaterial) {
      Color c = Color.white;
      c.a = 0.7f;

      mat = new Material(Shader.Find("Standard"));
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
  }

  public void BuildTexture(Texture2D tex)
  {
    mesh_renderer.sharedMaterials[0].mainTexture = tex;
  }

  public void BuildCircle(List<Vector3> vertices)
  {
    // receive the vertices
    List<Vector3> vertexList = vertices;

    int numOfPoints = vertexList.Count - 3;

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
    mesh.vertices = vertexList.ToArray();
    mesh.triangles = triangleList.ToArray();
    mesh.uv = uvList.ToArray();
    mesh.normals = normalList.ToArray();

    // assign our mesh to our filter/renderer
    mesh_filter.mesh = mesh;
    mesh_collider.sharedMesh = mesh;
  }

  public void BuildMesh(Vector2 corner, float[,] mat)
  {
    mapX = mat.GetLength(0);
    mapZ = mat.GetLength(1);

    int numTiles = mapX * mapZ;
    int numTris = numTiles * 2;

    int vSize_x = mapX + 1;
    int vSize_z = mapZ + 1;
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
        if (z >= mapZ && x >= mapX) {
          h = mat[x - 1, z - 1];
        } else if (z >= mapZ) {
          h = mat[x, z - 1];
        } else if (x >= mapX) {
          h = mat[x - 1, z];
        } else {
          h = mat[x, z];
        }
        vertices[z * vSize_x + x] = new Vector3(corner.x + x * tileSize, h + .1f, corner.y + z * tileSize);
        normals[z * vSize_x + x] = Vector3.up;
        uv[z * vSize_x + x] = new Vector2((float)x / vSize_x, (float)z / vSize_z);
      }
    }

    for (z = 0; z < mapZ; z++) {
      for (x = 0; x < mapX; x++) {
        int squareIndex = z * mapX + x;
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
  }
}
