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
    Dictionary<int, Vector3> rotations;

    Dictionary<int, Vector3> translations;

    // Start is called before the first frame update

    public Vector3 starting;

    public Vector3 lowest_vertice;

    void Start()
    {

        starting = transform.position;

        rotations = new Dictionary<int, Vector3>();
        rotations.Add(2, new Vector3(180, 0, 0));
        rotations.Add(3, new Vector3(0, 0, 16));

        translations = new Dictionary<int, Vector3>();
        translations.Add(2, new Vector3(0, -5, 0));

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
        print(temp);
        Vector3 Voxels_c = new Vector3(temp[0], temp[1], temp[2]);
        float Voxels_mass = temp[3];

        voxel_c = Voxels_c;

        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 c = rb.centerOfMass;
        
        print(rb.centerOfMass);
        print(mass);

        // rb.centerOfMass = (mass * c - Voxels_mass * Voxels_c) / (mass - Voxels_mass);
        // rb.centerOfMass = Vector3.zero;
        // rb.centerOfMass = new Vector3(0.3f, -0.6f, -0.1f);
        rb.centerOfMass = transform.TransformPoint(lowest_vertice + 0.25f * (new Vector3(-1, 1, -1)));
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


        var base_stand = Instantiate(_voxelModel, transform.TransformPoint(lowest_vertice), Quaternion.identity) as GameObject;
        base_stand.transform.localScale = new Vector3(1, 0.01f, 1);
        base_stand.transform.SetParent(rootTransform, true);
        Renderer rend = GetComponent<Renderer>();
        rend.material = material;
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
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        Mesh m = new Mesh();

        m.CombineMeshes(combine);

        Vector4 Voxels_c = CoM(m);
        return Voxels_c;
    }

    Vector4 CoM(Mesh mesh)
    {

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
            float mass = Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c)) / 18.0f;
            // float mass = Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c)) / 6.0f;
            centerOfMass += density * Vector3.Scale(Vector3.Cross(b - a, c - a), g) / 24.0f;
            massTotal += density * mass;
        }
        centerOfMass /= massTotal;
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

        // get the average vertices with y value with delta range
        float delta = 1;
        for (int i = 0; i < m.vertices.Length; i++) {
            // print("check: ");
            //print(m.vertices[i]);
            Vector3 v = transform.TransformPoint(m.vertices[i]);
            // print(v);
            if (v.y - lowest_vertice.y < delta) {
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
           
        reset();

        float min_height = mesh.vertices[0].y;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 v = mesh.vertices[i];
            if (v.y < min_height)
            {
                min_height = v.y;
                lowest_vertice = v;
            }
        }
        // plane.transform.position = lowest_vertice + (-1f) * Vector3.up;
        // transform.position = plane.transform.position + transform.TransformPoint(lowest_vertice) + (1f) * Vector3.up;
        transform.position = plane.transform.position - transform.TransformPoint(lowest_vertice) + 1 * Vector3.up;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void reset(){
        int index = GameObject.Find("Dropdown").GetComponent<Dropdown>().value;
        transform.position = starting;

        if (translations.ContainsKey(index)){
            transform.position += translations[index];
        } 

        if (rotations.ContainsKey(index)){
            transform.localRotation = Quaternion.Euler(rotations[index].x, rotations[index].y, rotations[index].z);
        } else {
            transform.localRotation = Quaternion.identity;
        }



        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public void Perturb(){
        reset();
        plane.transform.position = lowest_vertice + (-0.5f) * Vector3.up;
        // plane.transform.position = lowest_vertice + (-0.1f) * Vector3.up;

        GetComponent<Rigidbody>().useGravity = true;

        float eps = 1;

        GetComponent<Rigidbody>().velocity = eps * (new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));

    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.black;
        // Gizmos.DrawWireCube(GetComponent<Renderer>().bounds.center, GetComponent<Renderer>().bounds.size);
        // Gizmos.DrawWireSphere(GetComponent<Rigidbody>().centerOfMass, 1f);
        Gizmos.DrawLine(GetComponent<Rigidbody>().centerOfMass - 5f * Vector3.up , GetComponent<Rigidbody>().centerOfMass + 5f * Vector3.up);
        // Gizmos.DrawIcon(GetComponent<Rigidbody>().centerOfMass, "Light Gizmo.tiff", true);
        // Gizmos.DrawLine(voxel_c - 5 * Vector3.up , voxel_c + 5 * Vector3.up);
    }
}
