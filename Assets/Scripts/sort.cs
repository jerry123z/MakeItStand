using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sort : MonoBehaviour
{
    // // Start is called before the first frame update
    // IEnumerator ExampleCoroutine() {
    //     yield return new WaitForSeconds(2);
    //     Vector3 c0 = new Vector3(0, 0, 0);
    //     Vector3 c_star = new Vector3(0, 0, 0);

    //     GameObject Voxel_Root = GameObject.Find("Voxel Root");
    //     Transform transform = Voxel_Root.GetComponent<Transform>();
        
    //     double voxel_size = 1;

    //     // hard code surface to be plane at some height
    //     float surface_height = 0;
    //     Vector3 projection = (c0 - c_star);
    //     projection.z = surface_height;

    //     List<GameObject> voxel_list = new List<GameObject>();

    //     foreach (Transform child in Voxel_Root.transform) {
            
    //         // depends whether origin of object is at center or at corner
    //         // Vector3 centroid = child.position + voxel_size * new Vector3(1, 1, 1);
    //         Vector3 centroid = child.position;
    //         child.gameObject.GetComponent<d>().dist = Vector3.Dot((centroid - c_star), projection);
    //         voxel_list.Add(child.gameObject);
    //     }
    //     voxel_list.Sort(delegate(GameObject a, GameObject b) {
    //         return (a.GetComponent<d>().dist).CompareTo(b.GetComponent<d>().dist);
    //         });

    // }

    void Start()
    {
        // StartCoroutine(ExampleCoroutine());
    }
}
