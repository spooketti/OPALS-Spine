using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentroidManager : MonoBehaviour
{
    public GameObject spine;
    public Transform spineEulerTarget;
    private Vector3 spineCentroid = new Vector3(0, 0, 0);

    public Dictionary<int, Transform> idTransformMap;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        spineCentroid = new Vector3(0, 0, 0);
        spine.SetActive(true);
        int tagsDetected = 0;
        foreach (Transform child in transform)
        {
            if(!child.gameObject.activeInHierarchy)
            {
                continue;
            }
            tagsDetected++;
            Vector3 position = child.transform.position;
            spineCentroid.x += position.x;
            spineCentroid.y += position.y;
            spineCentroid.z += position.z;
        }
        if(tagsDetected <= 0) //im aware < 0 is not possible but oh well
        {
            spine.SetActive(false);
            Debug.Log("no tags detected");
            return;
        }
        // spine.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        spine.transform.position = new Vector3(spineCentroid.x / tagsDetected, spineCentroid.y / tagsDetected, spineCentroid.z / tagsDetected);
        spine.transform.LookAt(spineEulerTarget);
        spine.transform.eulerAngles = new Vector3(spine.transform.eulerAngles.x - 90, spine.transform.eulerAngles.y, spine.transform.eulerAngles.z);
    }
}
