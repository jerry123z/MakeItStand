using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C : MonoBehaviour
{
    public Material material;
    public GameObject plane;
    // Start is called before the first frame update
    void Start()
    {
        // CoM(GetComponent<meshFilter>());
        Rigidbody rb = GetComponent<Rigidbody>();
        // hard coding so i don't have to run CoM every time
        rb.centerOfMass = new Vector3(0, 1, -1);

        int _xDensity = 8;
        int _yDensity = 16;
        int _zDensity = 16;
        // int _yDensity = 8;
        // int _zDensity = 8;

        Bounds _bounds = new Bounds(new Vector3(0, 1f, -1f), new Vector3(2, 4, 4));
        Voxeliser _voxeliser = new Voxeliser(_bounds, _xDensity, _yDensity, _zDensity);
        _voxeliser.Voxelize(GetComponent<Transform>().parent);

        
        //This will return a 3D bool array that contains the voxel data. solid == true empty == false
        GameObject _voxelModel = GameObject.Find("Cube");
        
        var gridCubeSize = new Vector3(
            _bounds.size.x / _xDensity,
            _bounds.size.y / _yDensity,
            _bounds.size.z / _zDensity);
        // var voxelRoot = new GameObject("Voxel Root");
        // var rootTransform = voxelRoot.transform;
        var rootTransform = transform;
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

        List<GameObject> sorted_voxels = sorted(rb.centerOfMass, plane.transform.position.y);  

        find_base(GetComponent<MeshFilter>().mesh, plane.transform.position.y);

        int n = sorted_voxels.Count;

        // Delete max_d min_d later not used, 
        //float max_d = -100;
        //float min_d = 100;

        for (int i = 0; i < n; i++) {
            GameObject voxel = sorted_voxels[i];
            Renderer rend = voxel.GetComponent<Renderer> ();
            rend.material = new Material(Shader.Find("Specular"));

            float d = voxel.GetComponent<d>().dist;
            if (d > 0) {
                float sigmoid_d = 1 / (1 + Mathf.Pow(2.71f, d)); 
                rend.material.color = new Color(1 - d, d, 0);
            } else {
                rend.material.color = new Color(1, 1, 1);
            }
            // float d = 1 / (1 + Mathf.Pow(2.71f, voxel.GetComponent<d>().dist));
            // if (voxel.GetComponent<d>().dist > max_d){
            //     max_d = voxel.GetComponent<d>().dist;
            // }
            // if (voxel.GetComponent<d>().dist < min_d){
            //     min_d = voxel.GetComponent<d>().dist;
            // }
            // rend.material.color =  (float) i/ (float) n * (new Color(0, 1, 0));
            // print(rend.material.color);
        }
        // print(max_d);
        // print(min_d);

    }

    void Update()
    {
        // we need a button for this
        if (Input.GetMouseButtonDown(0))
        {
            // Rigidbody rb = GetComponent<Rigidbody>();
            // GameObject Voxel_Root = GameObject.Find("Voxel Root");
            // Transform transform = Voxel_Root.GetComponent<Transform>();

            // // need 2 for loops and a to_be_deleted list because if you delete while iterating it skips items;
            List<Transform> to_be_deleted = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.gameObject.GetComponent<d>().dist < 0)
                {
                    to_be_deleted.Add(child);
                    //child.parent = null;
                    //child.gameObject.SetActive(false);
                }
            }
            foreach(Transform child in to_be_deleted)
            {
                Destroy(child.gameObject);
                // child.parent = null;
                // child.gameObject.SetActive(false);
            }

            // transform.parent = Voxel_Root.transform;
            // combineMeshes();
            // Voxel_Root.AddComponent<MeshCollider>();
            // Voxel_Root.GetComponent<MeshCollider>().convex = true;
            // Voxel_Root.AddComponent<Rigidbody>();
            // Voxel_Root.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    void combineMeshes()
    {
        GameObject Voxel_Root = GameObject.Find("Voxel Root");

        transform.parent = Voxel_Root.transform;

        MeshFilter[] meshFilters = Voxel_Root.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            //if (meshFilters[i].gameObject.GetComponent<d>() && meshFilters[i].gameObject.GetComponent<d>().dist > 0)
            //{
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);
            //}

            i++;
        }
        Voxel_Root.AddComponent<MeshFilter>();
        Voxel_Root.GetComponent<MeshFilter>().mesh = new Mesh();
        Voxel_Root.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        Voxel_Root.AddComponent<MeshRenderer>();
        Renderer rend = Voxel_Root.GetComponent<Renderer>();
        //rend.material = new Material(Shader.Find("Specular"));
        rend.material = material;
        //Voxel_Root.gameObject.SetActive(true);

        //Voxel_Root.transform.parent = transform;
    }

    void CoM(MeshFilter meshFilter)
    {
        // MeshFilter meshFilter = GetComponent<MeshFilter>();
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
        // print(centerOfMass);
        centerOfMass /= massTotal;
        // print(massTotal);
        // print(centerOfMass);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        //SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        //sc.radius = 0.5f;
        //sc.center = centerOfMass;
    }
    List<GameObject> sorted(Vector3 c0, float ground_height){
        // Vector3 c_star = new Vector3(0, 0, 0);
        Vector3 c_star = find_base(GetComponent<MeshFilter>().mesh, ground_height);
        print(c_star);

        // GameObject Voxel_Root = GameObject.Find("Voxel Root");
        // Transform transform = Voxel_Root.GetComponent<Transform>();
        
        double voxel_size = 1;

        Vector3 projection = (c0 - c_star);
        projection.y = ground_height;

        List<GameObject> voxel_list = new List<GameObject>();

        foreach (Transform child in transform) {
            
            // depends whether origin of object is at center or at corner
            // Vector3 centroid = child.position + voxel_size * new Vector3(1, 1, 1);
            Vector3 centroid = child.position;
            child.gameObject.GetComponent<d>().dist = Vector3.Dot((centroid - c_star), projection) - ((centroid - c_star).y * projection.y);
            // print(Vector3.Dot((centroid - c_star), projection));
            voxel_list.Add(child.gameObject);
        }
        voxel_list.Sort(delegate(GameObject a, GameObject b) {
            return (a.GetComponent<d>().dist).CompareTo(b.GetComponent<d>().dist);
            });
        return voxel_list;
    }

    Vector3 find_base(Mesh m, float ground_height){
        print(ground_height);
        Vector3 center = new Vector3();
        int k = 0;
        Vector3[] vertices = m.vertices;

        // first get lowest vertex in mesh
        float min_height = 100;
        Vector3 lowest = new Vector3();
        for (int i = 0; i < m.vertices.Length; i++)
        {
            Vector3 v = m.vertices[i];
            if (v.y < min_height)
            {
                min_height = v.y;
                lowest = v;
            }
        }

        // get the average vertices with y value with delta range
        float delta = 1;
        for (int i = 0; i < m.vertices.Length; i++) {
            // print("check: ");
            //print(m.vertices[i]);
            Vector3 v = transform.TransformPoint(m.vertices[i]);
            // print(v);
            if (v.y - min_height < delta) {
                center += v;
                k += 1;
            }
        }
        m.vertices = vertices;
        center /= k;
        // GameObject test = GameObject.Instantiate(GameObject.Find("Sphere"));
        // test.transform.position = center;
        // print(center);
        return center;
    }

}
