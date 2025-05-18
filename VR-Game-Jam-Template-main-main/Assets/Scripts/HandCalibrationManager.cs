using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Firestore;

public class HandCalibrationManager : MonoBehaviour
{
    public Transform rightHand;
    public Transform leftHand;
    public Transform bodyCenter;

    public Button calibrateRightButton;
    public Button calibrateLeftButton;

    private FirebaseFirestore db;
    private float rightArmLengthCM;
    private float leftArmLengthCM;
    private bool rightCalibrated = false;
    private bool leftCalibrated = false;

    void Awake()
    {
        if (rightHand == null)
            rightHand = GameObject.Find("Right Hand Model")?.transform;

        if (leftHand == null)
            leftHand = GameObject.Find("Left Hand Model")?.transform;

        if (bodyCenter == null)
            bodyCenter = Camera.main?.transform;
    }

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        calibrateRightButton.onClick.AddListener(CalibrateRightHand);
        calibrateLeftButton.onClick.AddListener(CalibrateLeftHand);
    }

    public void CalibrateRightHand()
    {
        if (rightHand != null && bodyCenter != null)
        {
            Vector3 shoulderPos = bodyCenter.position + bodyCenter.right * 0.15f + bodyCenter.up * -0.1f;
            rightArmLengthCM = Vector3.Distance(shoulderPos, rightHand.position) * 100f;
            rightCalibrated = true;

            Debug.Log($"Sað kol uzunluðu: {rightArmLengthCM:F2} cm");

            CheckCalibrationComplete();
        }
    }

    public void CalibrateLeftHand()
    {
        if (leftHand != null && bodyCenter != null)
        {
            Vector3 shoulderPos = bodyCenter.position + bodyCenter.right * -0.15f + bodyCenter.up * -0.1f;
            leftArmLengthCM = Vector3.Distance(shoulderPos, leftHand.position) * 100f;
            leftCalibrated = true;

            Debug.Log($"Sol kol uzunluðu: {leftArmLengthCM:F2} cm");

            CheckCalibrationComplete();
        }
    }

    private void CheckCalibrationComplete()
    {
        if (IsCalibrationComplete())
        {
            Debug.Log("Her iki kol kalibre edildi. Sahne deðiþtiriliyor...");
            SceneManager.LoadScene("LVL2"); 
        }
    }

    public float GetRightArmLengthCM() => rightArmLengthCM;
    public float GetLeftArmLengthCM() => leftArmLengthCM;
    public bool IsCalibrationComplete() => rightCalibrated && leftCalibrated;
}


