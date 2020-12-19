using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfGravity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        Vector3 centerOfMass = new Vector3(0, 0, 0);
        float volumeTotal = 0f;
        for(int face = 0; face < mesh.triangles.Length; face += 3)
        {
            int va, vb, vc;
            va = mesh.triangles[face];
            vb = mesh.triangles[face+1];
            vc = mesh.triangles[face+2];
            Vector3 a, b, c;
            a = mesh.vertices[va];
            b = mesh.vertices[vb];
            c = mesh.vertices[vc];
            
            //Check direction of the face normal and face center
            Vector3 faceCenter = (a + b + c) / 3;
            Vector3 faceNormal;
            faceNormal = Vector3.Cross(b - a, c - a);

            int sign=1;
            //use vector projection to check relative direction of normal.
            //if the value is negative we can subtract volume.
            //this way we can account for concave surfaces on our mesh
            if (Vector3.Dot(faceNormal, faceCenter) / faceCenter.magnitude >= 0)
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }
            //the Centeroid of a tetrahedron is the average of its 4 points,
            //We assume the origin is at 0
            float vol = Vector3.Dot(Vector3.Cross(a, b), c) / 6.0f;
            centerOfMass += vol * sign * (a + b + c) / 4;
            volumeTotal += vol * sign;
        }
        print(centerOfMass);
        print(volumeTotal);
        centerOfMass /= volumeTotal;
        print(centerOfMass);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.radius = 0.5f;
        sc.center = centerOfMass;
    }

    void FixedUpdate() {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 force = new Vector3(0, -9.8f, 0);
        rb.AddForce(force);
    }
}
