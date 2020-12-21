using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class C : MonoBehaviour
{
    public Mesh[] meshes;
    public Material material;
    public Material solid_material;
    public GameObject plane;
    public float mass;

    public Vector3 voxel_c;
    // mesh import scalings (couldnt figure out how to access so hardcoding in start)
    Dictionary<int, float> scalings;
    Dictionary<int, Vector3> rotations;

    // Start is called before the first frame update

    public Vector3 starting;

    void Start()
    {
        scalings = new Dictionary<int, float>();
        scalings.Add(0, 1.0f);
        scalings.Add(1, 5.0f);
        scalings.Add(2, 1.0f);

        starting = transform.position;

        rotations = new Dictionary<int, Vector3>();
        // rotations.Add(1, new Vector3(-16, 0, -27));
        // scalings.Add(0, 1.0f);
        // scalings.Add(1, 5.0f);
        // scalings.Add(2, 1.0f);

        starting = transform.position;
    }

    public void Drop(){
        GetComponent<Rigidbody>().useGravity = true;
    }

    public void Balance(){
        // need 2 for loops and a to_be_deleted list because if you delete while iterating it skips items;
        
        List<Transform> to_be_deleted = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<d>().dist < 0)
            {
                to_be_deleted.Add(child);
            }
        }

        foreach(Transform child in to_be_deleted)
        {
            child.parent = null;
            Destroy(child.gameObject);
        }

        Vector4 temp = getComOfChildren();
        // print(CoM(GetComponent<MeshFilter>().mesh));
        print(temp);
        Vector3 Voxels_c = new Vector3(temp[0], temp[1], temp[2]);
        float Voxels_mass = temp[3];

        voxel_c = Voxels_c;

        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 c = rb.centerOfMass;
        
        print(rb.centerOfMass);
        print(mass);

        rb.centerOfMass = (mass * c - Voxels_mass * Voxels_c) / (mass - Voxels_mass);
        // rb.centerOfMass = Vector3.zero;

        print(rb.centerOfMass);

    }


    public void Carve(){
        // sets material to transparent and shows voxels inside
        // add alpha value later?

        Vector4 temp = CoM(GetComponent<MeshFilter>().mesh);
        Vector3 c0 = new Vector3(temp[0], temp[1], temp[2]);
        mass = temp[3];
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = c0;
        // hard coding so i don't have to run CoM every time
        // rb.centerOfMass = new Vector3(0, 1, -1);

        int _xDensity = 8;
        int _yDensity = 16;
        int _zDensity = 16;
        // int _xDensity = 32;
        // int _yDensity = 32;
        // int _zDensity = 32;
        // int _yDensity = 8;
        // int _zDensity = 8;

        Bounds _bounds = GetComponent<MeshFilter>().mesh.bounds;

        Voxeliser _voxeliser = new Voxeliser(_bounds, _xDensity, _yDensity, _zDensity);
        _voxeliser.Voxelize(GetComponent<Transform>().parent);
        
        //This will return a 3D bool array that contains the voxel data. solid == true empty == false
        GameObject _voxelModel = GameObject.Find("Cube");
        
        var gridCubeSize = new Vector3(
            _bounds.size.x / _xDensity,
            _bounds.size.y / _yDensity,
            _bounds.size.z / _zDensity);
        var rootTransform = transform;
        var worldCentre = _bounds.min + gridCubeSize / 2;

        float ground_height = 0;

        Vector3 c_star = find_base(GetComponent<MeshFilter>().mesh);
        Vector3 projection = (c0 - c_star);
        projection.y = ground_height;

        for (int x = 0; x < _xDensity; x++)
        {
            for (int y = 0; y < _yDensity; y++)
            {
                for (int z = 0; z < _zDensity; z++)
                {
                    if (_voxeliser.VoxelMap[x][y][z])
                    {
                        var voxel = Instantiate(_voxelModel, new Vector3(
                            x * gridCubeSize.x,
                            y * gridCubeSize.y,
                            z * gridCubeSize.z) + worldCentre, Quaternion.identity) as GameObject;
                        voxel.transform.localScale = gridCubeSize;
                        voxel.transform.SetParent(rootTransform, true);

                        // double voxel_size = 1;

                        // depends whether origin of object is at center or at corner
                        // Vector3 centroid = child.position + voxel_size * new Vector3(1, 1, 1);
                        Vector3 centroid = voxel.transform.position;
                        float d = Vector3.Dot((centroid - c_star), projection) - ((centroid - c_star).y * projection.y);
                        voxel.gameObject.GetComponent<d>().dist = d;

                        Renderer voxel_rend = voxel.GetComponent<Renderer>();
                        // voxel_rend.material = new Material(Shader.Find("Specular"));

                        if (d > 0) {
                            float sigmoid_d = 1 / (1 + Mathf.Pow(2.71f, d)); 
                            voxel_rend.material.color = new Color(1 - d, d, 0);
                        } else {
                            voxel_rend.material.color = new Color(1, 1, 1);
                        }
                    }
                }
            }
        }

        Renderer rend = GetComponent<Renderer>();
        rend.material = material;

        // // List<GameObject> sorted_voxels = sorted(rb.centerOfMass, plane.transform.position.y);  

        // find_base(GetComponent<MeshFilter>().mesh, plane.transform.position.y);

        // // Delete max_d min_d later not used, 

        // for (int i = 0; i < n; i++) {
        //     GameObject voxel = sorted_voxels[i];
        //     Renderer voxel_rend = voxel.GetComponent<Renderer> ();
        //     voxel_rend.material = new Material(Shader.Find("Specular"));

        //     float d = voxel.GetComponent<d>().dist;
        //     if (d > 0) {
        //         float sigmoid_d = 1 / (1 + Mathf.Pow(2.71f, d)); 
        //         voxel_rend.material.color = new Color(1 - d, d, 0);
        //     } else {
        //         voxel_rend.material.color = new Color(1, 1, 1);
        //     }
        // }
    } 

    void Update()
    {
        // Bounds _bounds = GetComponent<Renderer>().bounds;
        // print(_bounds);

        // we need a button for this
        // if (Input.GetMouseButtonDown(1))
        // {
        //     // Rigidbody rb = GetComponent<Rigidbody>();
        //     // GameObject Voxel_Root = GameObject.Find("Voxel Root");
        //     // Transform transform = Voxel_Root.GetComponent<Transform>();

        //     // // need 2 for loops and a to_be_deleted list because if you delete while iterating it skips items;
            
        //     List<Transform> to_be_deleted = new List<Transform>();
        //     // print(transform.childCount);
        //     foreach (Transform child in transform)
        //     {
        //         if (child.gameObject.GetComponent<d>().dist < 0)
        //         {
        //             to_be_deleted.Add(child);
        //             //child.gameObject.SetActive(false);
        //         }
        //     }

        //     // print(to_be_deleted.Count);
        //     foreach(Transform child in to_be_deleted)
        //     {
        //         child.parent = null;
        //         Destroy(child.gameObject);
        //         // child.parent = null;
        //         // child.gameObject.SetActive(false);
        //     }
        //     // print(transform.childCount);

        //     // transform.parent = Voxel_Root.transform;
        //     // combineMeshes();
        //     // Voxel_Root.AddComponent<MeshCollider>();
        //     // Voxel_Root.GetComponent<MeshCollider>().convex = true;
        //     // Voxel_Root.AddComponent<Rigidbody>();
        //     // Voxel_Root.GetComponent<Rigidbody>().useGravity = true;

        //     Vector4 temp = getComOfChildren();
        //     print(CoM(GetComponent<MeshFilter>().mesh));
        //     print(temp);
        //     Vector3 Voxels_c = new Vector3(temp[0], temp[1], temp[2]);
        //     float Voxels_mass = temp[3];

        //     Rigidbody rb = GetComponent<Rigidbody>();
        //     Vector3 c = rb.centerOfMass;

        //     rb.centerOfMass = (mass * c - Voxels_mass * Voxels_c) / (mass - Voxels_mass);
        //     print(rb.centerOfMass);
        //     print((mass * c - Voxels_mass * Voxels_c));
        //     print((mass - Voxels_mass));
        //     GameObject test = GameObject.Instantiate(GameObject.Find("Sphere"));
        //     test.transform.position = rb.centerOfMass;
        // }

    }

    Vector4 getComOfChildren()
    {

        MeshFilter[] meshFilters_temp = gameObject.GetComponentsInChildren<MeshFilter>();
        MeshFilter[] meshFilters = new MeshFilter[meshFilters_temp.Length - 1];

        for (int j = 0; j < meshFilters.Length; j++){
            meshFilters[j] = meshFilters_temp[j+1];
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            //if (meshFilters[i].gameObject.GetComponent<d>() && meshFilters[i].gameObject.GetComponent<d>().dist > 0)
            //{
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                // meshFilters[i].gameObject.SetActive(false);
            //}

            i++;
        }

        Mesh m = new Mesh();

        m.CombineMeshes(combine);

        Vector4 Voxels_c = CoM(m);
        return Voxels_c;
    }

    Vector4 CoM(Mesh mesh)
    {
        // MeshFilter meshFilter = GetComponent<MeshFilter>();
        // Mesh mesh = meshFilter.mesh;
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

            //the Centroid of a tetrahedron is the average of its 4 points,
            //We assume the origin is at 0

            float density = 1;
            // float mass = Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c)) / 18.0f;
            float mass = Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c)) / 6.0f;
            centerOfMass += density * Vector3.Scale(Vector3.Cross(b - a, c - a), g) / 24.0f;
            massTotal += density * mass;
        }
        // print(centerOfMass);
        centerOfMass /= massTotal;
        // print(massTotal);
        // print(centerOfMass);
        // Rigidbody rb = GetComponent<Rigidbody>();
        // rb.centerOfMass = centerOfMass;
        //SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        //sc.radius = 0.5f;
        //sc.center = centerOfMass;
        return new Vector4(centerOfMass.x, centerOfMass.y, centerOfMass.z, massTotal);
    }
    List<GameObject> sorted(Vector3 c0, float ground_height){
        Vector3 c_star = find_base(GetComponent<MeshFilter>().mesh);
        // double voxel_size = 1;
        Vector3 projection = (c0 - c_star);
        projection.y = ground_height;
        List<GameObject> voxel_list = new List<GameObject>();
        foreach (Transform child in transform) {
            // depends whether origin of object is at center or at corner
            // Vector3 centroid = child.position + voxel_size * new Vector3(1, 1, 1);
            Vector3 centroid = child.position;
            child.gameObject.GetComponent<d>().dist = Vector3.Dot((centroid - c_star), projection) - ((centroid - c_star).y * projection.y);
            voxel_list.Add(child.gameObject);
        }
        voxel_list.Sort(delegate(GameObject a, GameObject b) {
            return (a.GetComponent<d>().dist).CompareTo(b.GetComponent<d>().dist);
            });
        return voxel_list;
    }

    Vector3 find_base(Mesh m){
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
    
    public void ChangeMesh(){
        int index = GameObject.Find("Dropdown").GetComponent<Dropdown>().value;

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Mesh mesh;
        // mesh = meshes[index];


        float quality = 0.1f;
        var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        meshSimplifier.Initialize(meshes[index]);
        meshSimplifier.SimplifyMesh(quality);
        mesh = meshSimplifier.ToMesh();

        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        Renderer rend = GetComponent<Renderer>();
        rend.material = solid_material;
        
        List<Transform> to_be_deleted = new List<Transform>();
        foreach (Transform child in transform)
        {
            to_be_deleted.Add(child);
        }

        foreach(Transform child in to_be_deleted)
        {
            Destroy(child.gameObject);
        }
           
        transform.position = starting;

        if (rotations.ContainsKey(index)){
            transform.localRotation = Quaternion.Euler(rotations[index].x, rotations[index].y, rotations[index].z);
        } else {
            transform.localRotation = Quaternion.identity;
        }
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        float min_height = 100;
        Vector3 lowest = new Vector3();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 v = mesh.vertices[i];
            if (v.y < min_height)
            {
                min_height = v.y;
                lowest = v;
            }
        }

        // plane.transform.position = lowest + (-0.5f) * Vector3.up;
    }
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.black;
        // Gizmos.DrawWireCube(GetComponent<Renderer>().bounds.center, GetComponent<Renderer>().bounds.size);
        // Gizmos.DrawWireSphere(GetComponent<Rigidbody>().centerOfMass, 1f);
        Gizmos.DrawLine(GetComponent<Rigidbody>().centerOfMass - 5 * Vector3.up , GetComponent<Rigidbody>().centerOfMass + 5 * Vector3.up);
        // Gizmos.DrawIcon(GetComponent<Rigidbody>().centerOfMass, "Light Gizmo.tiff", true);
        // Gizmos.DrawLine(voxel_c - 5 * Vector3.up , voxel_c + 5 * Vector3.up);
    }
}
