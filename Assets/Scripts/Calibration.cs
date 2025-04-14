using UnityEngine;
using TMPro;
using Image = UnityEngine.UI.Image;
using System.Collections.Generic;

public class Calibration : MonoBehaviour
{
    public ZEDArUcoDetectionManager detectionManager;
    public GameObject centroid;
    public GameObject tagPrefab;
    public Image radialProgressImage;

    public GameObject eulerPanel;
    public GameObject RadialObject;
    public TMP_Text eulerStatus;
    public GameObject systemPanel;
    public Transform spineEulerTarget;

    private int lastPositionListSize = 0;
    public bool isCalibrated = false;
    private bool isAngleCalibrated = false;
    private bool isSystemCalibrated = false;
    private bool isEulerTagSeen = false;
    private int eulerID = 0;
    private float timer = 0f;
    private GameObject spine;
    public Dictionary<int, Transform> tagOrigins = new Dictionary<int, Transform>();

    private void Start()
    {
        spine = GameObject.Find("Spine");
    }

    private void Update()
    {
        if (isCalibrated)
        {
            RadialObject.SetActive(false);
            return;
        }

        if (!isAngleCalibrated)
        {
            HandleEulerCalibration();
            return;
        }

        if (!isSystemCalibrated)
        {
            HandleSystemCalibration();
            return;
        }

        FinalizeCalibration();
    }

    private void HandleEulerCalibration()
    {
        spine.SetActive(false);
        eulerPanel.SetActive(true);

        if (lastPositionListSize != detectionManager.CalibrationIDList.Count)
        {
            lastPositionListSize = detectionManager.CalibrationIDList.Count;
            isEulerTagSeen = false;
            timer = 0f;

            int detectedTags = detectionManager.numberOfDetectedTags;

            switch (detectedTags)
            {
                case 0:
                    eulerStatus.text = "No Tags Detected";
                    break;

                case 1:
                    isEulerTagSeen = true;
                    int tagID = detectionManager.oneTagDetectedId;
                    eulerStatus.text = $"Tag Detected: {tagID}";

                    var moveScript = spineEulerTarget.GetComponent<MarkerObject_MoveToMarkerSimple>();
                    moveScript.markerID = tagID;

                    detectionManager.UnregisterAllMarkers();
                    detectionManager.DynamicRegisterMarker(moveScript);
                    break;

                default:
                    eulerStatus.text = "Please have only one tag showing";
                    break;
            }
        }

        if (isEulerTagSeen)
        {
            timer += Time.deltaTime;
            radialProgressImage.color = Color.Lerp(Color.white, Color.green, Mathf.Clamp01(timer / 5f));
            radialProgressImage.fillAmount = Mathf.Clamp01(timer / 5f);

            if (timer >= 5f)
            {
                isAngleCalibrated = true;
                eulerID = detectionManager.oneTagDetectedId;
                timer = 0f;
                radialProgressImage.fillAmount = 0f;

                eulerPanel.SetActive(false);
                systemPanel.SetActive(true);

                Debug.Log("Euler calibration completed");
            }
            return;
        }
        timer = 0f;
        radialProgressImage.fillAmount = 0f;
    }


    private void HandleSystemCalibration()
    {
        spine.SetActive(true);
        centroid.GetComponent<CentroidManager>().enabled = true;

        if (lastPositionListSize != detectionManager.CalibrationIDList.Count)
        {
            timer = 0f;
            lastPositionListSize = detectionManager.CalibrationIDList.Count;

            foreach (int id in detectionManager.CalibrationIDList)
            {
                if (id == eulerID || GameObject.Find($"tag{id}")) continue;

                GameObject tag = CreateMarkerObject($"tag{id}", id);
                tag.transform.SetParent(centroid.transform);
            }
        }
        if (detectionManager.CalibrationIDList.Count > 1)
        {
            timer += Time.deltaTime;
            radialProgressImage.color = Color.Lerp(Color.white, Color.green, Mathf.Clamp01(timer / 7f));
            radialProgressImage.fillAmount = Mathf.Clamp01(timer / 7f);
            if (timer >= 7f)
            {
                isSystemCalibrated = true;
                systemPanel.SetActive(false);
                foreach (Transform child in centroid.transform)
                {
                    if (!child.gameObject.activeInHierarchy)
                    {
                        Destroy(child.gameObject);
                        continue;
                    }
                    tagOrigins.Add(child.GetComponent<MarkerObject_MoveToMarkerSimple>().markerID, child);
                }
                return;
            }
        }
    }

    private void FinalizeCalibration()
    {
        isCalibrated = true;
        systemPanel.SetActive(false);
        RadialObject.SetActive(false);
    }

    private GameObject CreateMarkerObject(string name, int id)
    {
        GameObject marker = Instantiate(tagPrefab, Vector3.zero, Quaternion.identity);
        marker.name = name;
        marker.SetActive(false);

        var moveScript = marker.GetComponent<MarkerObject_MoveToMarkerSimple>();
        moveScript.markerID = id;

        marker.GetComponent<ScaleToMarkerSize>().arucoManager = detectionManager;
        marker.SetActive(true);

        return marker;
    }
}
