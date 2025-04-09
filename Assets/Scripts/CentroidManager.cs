using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentroidManager : MonoBehaviour
{
    public GameObject spine;
    public Transform spineEulerTarget;
    private Vector3 spineCentroid = new Vector3(0, 0, 0);
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
        foreach (GameObject child in gameObject.GetComponentsInChildren<GameObject>())
        {
            if(!child.activeInHierarchy)
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
        spine.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        spine.transform.position = new Vector3(spineCentroid.x / tagsDetected, spineCentroid.y / tagsDetected, spineCentroid.z / tagsDetected);
        spine.transform.LookAt(spineEulerTarget);
        spine.transform.eulerAngles = new Vector3(spine.transform.eulerAngles.x + 90, spine.transform.eulerAngles.y, spine.transform.eulerAngles.z);
    }
}
