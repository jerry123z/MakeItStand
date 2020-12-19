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

        for(int face = 0; face<mesh.triangles.Length; face += 3)
        {
            Vector3 a, b, c;
            a = mesh.vertices[mesh.triangles[face]];
            b = mesh.vertices[mesh.triangles[face+1]];
            c = mesh.vertices[mesh.triangles[face+2]];
            //the Centeroid of a tetrahedron is the average of its 4 points,
            //We assume the origin is at 0
            float v = Vector3.Dot(Vector3.Cross(a, b), c);
            //Check direction of the average vertex normal to ensure its the same direction
            //towards the face center
            //Vector3 faceCenter = (a + b + c) / 3;
            //Vector3 an, bn, cn;
            //an = mesh.normals[mesh.triangles[face]];
            //bn = mesh.normals[mesh.triangles[face + 1]];
            //cn = mesh.normals[mesh.triangles[face + 2]];
            //Vector3 averagen = (an + bn + cn) / 3;

            int sign = 1;
            //use vector projection to check relative direction of normal to face.
            //if the value is negative we can subtract volume.
            //this way we can account for concave surfaces on our mesh
            //if (Vector3.Dot(averagen, faceCenter) / faceCenter.magnitude >= 0)
            //{
            //    sign = 1;
            //}
            //else
            //{
            //    sign = -1;
            //}

            centerOfMass += v *sign * (a + b + c) / 4;
            volumeTotal += v *sign;
        }
        centerOfMass /= volumeTotal;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
    }
}
