using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //CoM();
        int _xDensity = 8;
        int _yDensity = 16;
        int _zDensity = 16;

        Bounds _bounds = new Bounds(new Vector3(0, 1f, -1f), new Vector3(2, 4, 4));
        Voxeliser _voxeliser = new Voxeliser(_bounds, _xDensity, _yDensity, _zDensity);
        _voxeliser.Voxelize(GetComponent<Transform>().parent);

        
        //This will return a 3D bool array that contains the voxel data. solid == true empty == false
        GameObject _voxelModel = GameObject.Find("Cube");
        
        var gridCubeSize = new Vector3(
            _bounds.size.x / _xDensity,
            _bounds.size.y / _yDensity,
            _bounds.size.z / _zDensity);
        var voxelRoot = new GameObject("Voxel Root");
        var rootTransform = voxelRoot.transform;
        var worldCentre = _bounds.min + gridCubeSize / 2;
        for (int x = 0; x < _xDensity; x++)
        {
            for (int y = 0; y < _yDensity; y++)
            {
                for (int z = 0; z < _zDensity; z++)
                {
                    if (_voxeliser.VoxelMap[x][y][z])
                    {
                        var go = Instantiate(_voxelModel, new Vector3(
                            x * gridCubeSize.x,
                            y * gridCubeSize.y,
                            z * gridCubeSize.z) + worldCentre, Quaternion.identity) as GameObject;
                        go.transform.localScale = gridCubeSize;
                        go.transform.SetParent(rootTransform, true);
                    }
                }
            }
        }
    }

    void CoM()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        //float quality = 0.1f;
        //var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        //meshSimplifier.Initialize(mesh);
        //meshSimplifier.SimplifyMesh(quality);
        //mesh = meshSimplifier.ToMesh();
        //meshFilter.mesh = mesh;

        Vector3 centerOfMass = new Vector3(0, 0, 0);
        float massTotal = 0f;
        for (int face = 0; face < mesh.triangles.Length; face += 3)
        {
            //assume density of 1
            int va, vb, vc;
            va = mesh.triangles[face];
            vb = mesh.triangles[face + 1];
            vc = mesh.triangles[face + 2];
            Vector3 a, b, c;
            a = mesh.vertices[va];
            b = mesh.vertices[vb];
            c = mesh.vertices[vc];
            Vector3 g;

            g = Vector3.Scale(a, a) + Vector3.Scale(a, b) + Vector3.Scale(b, b) + Vector3.Scale(b, c) + Vector3.Scale(c, c) + Vector3.Scale(c, a);

            //the Centeroid of a tetrahedron is the average of its 4 points,
            //We assume the origin is at 0
            float mass = Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c)) / 18.0f;
            centerOfMass += Vector3.Scale(Vector3.Cross(b - a, c - a), g) / 24.0f;
            massTotal += mass;
        }
        print(centerOfMass);
        print(massTotal);
        centerOfMass /= massTotal;
        print(centerOfMass);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        //SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        //sc.radius = 0.5f;
        //sc.center = centerOfMass;
    }
}
