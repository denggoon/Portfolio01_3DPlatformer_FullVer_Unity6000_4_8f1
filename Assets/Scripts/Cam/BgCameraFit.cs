using UnityEngine;

// Camera_bg에 부착. bg_test 스프라이트를 현재 화면 비율에 맞게 자동으로 스케일 조정.
// 퍼스펙티브/오소그래픽 카메라 모두 지원.
[RequireComponent(typeof(Camera))]
public class BgCameraFit : MonoBehaviour
{
    [SerializeField] private Transform bgSprite; // bg_test Transform

    void Start()
    {
        FitBgToScreen();
    }

    void FitBgToScreen()
    {
        if (bgSprite == null) return;

        Camera cam = GetComponent<Camera>();
        float aspect = (float)Screen.width / Screen.height;

        float height, width;

        if (cam.orthographic)
        {
            height = cam.orthographicSize * 2f;
            width  = height * aspect;
        }
        else
        {
            float dist = bgSprite.localPosition.z;
            height = 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            width  = height * aspect;
        }

        bgSprite.localScale = new Vector3(width, height, bgSprite.localScale.z);
    }
}
