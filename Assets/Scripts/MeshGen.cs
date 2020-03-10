using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGen : MonoBehaviour
{

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        int width = 1;
        int height = 1;
        Vector3[] vertices = new Vector3[8]
        {
    new Vector3(0, 0, 0),
    new Vector3(width, 0, 0),
    new Vector3(0, height, 0),
    new Vector3(width, height, 0),
        new Vector3(0, 0, 0.1f),
    new Vector3(width, 0, 0.1f),
    new Vector3(0, height, 0.1f),
    new Vector3(width, height, 0.1f)
    };
        int[] tris = new int[12]
    {
    // lower left triangle
    0, 2, 1,
    // upper right triangle
    2, 3, 1,
    // lower left triangle
    0, 2, 1,
    // upper right triangle
    2, 3, 1
    };
        Vector3[] normals = new Vector3[8]
    {
    -Vector3.forward,
    -Vector3.forward,
    -Vector3.forward,
    -Vector3.forward,
        Vector3.forward,
    Vector3.forward,
    Vector3.forward,
    Vector3.forward
    };
        Vector2[] uv = new Vector2[8]
    {
      new Vector2(0, 0),
      new Vector2(1, 0),
      new Vector2(0, 1),
      new Vector2(1, 1),
            new Vector2(0, 0),
      new Vector2(1, 0),
      new Vector2(0, 1),
      new Vector2(1, 1)
    };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = tris;
        mesh.uv = uv;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
