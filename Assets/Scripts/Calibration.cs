using UnityEngine;
using TMPro;
using Image = UnityEngine.UI.Image;

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
    private bool isCalibrated = false;
    private bool isAngleCalibrated = false;
    private bool isSystemCalibrated = false;
    private bool isEulerTagSeen = false;
    private int eulerID = 0;
    private float timer = 0f;

    private void Update()
    {
        if (isCalibrated)
        {
            Debug.Log("bro");
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
            radialProgressImage.fillAmount = Mathf.Clamp01(timer / 5f);

            if (timer >= 5f)
            {
                isAngleCalibrated = true;
                eulerID = detectionManager.oneTagDetectedId;

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
            radialProgressImage.fillAmount = Mathf.Clamp01(timer / 10f);
            if (timer >= 10f)
            {
                isSystemCalibrated = true;
                systemPanel.SetActive(false);
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
