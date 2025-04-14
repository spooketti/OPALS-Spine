using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CentroidManager : MonoBehaviour
{
    public GameObject spine;
    public Transform spineEulerTarget;
    private Vector3 spineCentroid = new Vector3(0, 0, 0);

    public Calibration calibration;

    private List<Transform> blockedTags = new List<Transform>();

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
        Vector3 originDisplacement = new Vector3(0, 0, 0);
        blockedTags.Clear();
        foreach (Transform child in transform)
        {
            tagsDetected++;
            if (!child.gameObject.activeInHierarchy)
            {
                blockedTags.Add(child);
                continue;
            }
            int tagID = int.Parse(child.gameObject.name.Substring(3));
            if (calibration.isCalibrated && calibration.tagOrigins.ContainsKey(tagID))
            {
                originDisplacement = child.position - calibration.tagOrigins[tagID].position;
            }
            Vector3 position = child.position;

            spineCentroid.x += position.x;
            spineCentroid.y += position.y;
            spineCentroid.z += position.z;
        }
        if (tagsDetected <= 0) //im aware < 0 is not possible but oh well
        {
            spine.SetActive(false);
            Debug.Log("no tags detected");
            return;
        }
        foreach (Transform child in blockedTags)
        {
            if(!calibration.isCalibrated)
            {
                break;
            }
            int tagID = int.Parse(child.gameObject.name.Substring(3));
            Vector3 position = calibration.tagOrigins[tagID].position + originDisplacement;

            spineCentroid.x += position.x;
            spineCentroid.y += position.y;
            spineCentroid.z += position.z;
        }
        spine.transform.position = new Vector3(spineCentroid.x / tagsDetected, spineCentroid.y / tagsDetected, spineCentroid.z / tagsDetected);
        spine.transform.LookAt(spineEulerTarget);
        spine.transform.localEulerAngles = new Vector3(spine.transform.eulerAngles.x + 90, spine.transform.eulerAngles.y, spine.transform.eulerAngles.z);
    }
}

//given points (-1,1) (1,1) ()