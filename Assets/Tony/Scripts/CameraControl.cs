using UnityEngine;
using Unity.Cinemachine;
public class CameraControl : MonoBehaviour
{

    public CinemachineBrain brain;

    private CinemachineCamera cam1, cam2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam1 = GetComponent<CinemachineCamera>();
        cam2 = GetComponent<CinemachineCamera>();
        brain.SetCameraOverride(1, 1, cam1, cam2, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
