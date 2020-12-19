using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
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
            float mass = Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c)) / 6.0f;
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

    void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 force = new Vector3(0, -9.8f, 0);
        rb.AddForce(force);
    }
}
