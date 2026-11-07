using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset;
    public float rotateSpeed = 6f;
    public Transform focusView;

    private Vector3 velocity = Vector3.zero;
    private Quaternion defaultRotation;
    private Transform cam;

    private void Start()
    {
        defaultRotation = transform.rotation;
        Camera c = GetComponentInChildren<Camera>();
        cam = c != null ? c.transform : transform;
    }
    private void Update()
    {
        Vector3 targetPos;
        Quaternion targetRot;

        if (focusView != null)
        {
            if (cam == transform)
            {
                targetPos = focusView.position;
                targetRot = focusView.rotation;
            }
            else
            {
                targetRot = focusView.rotation * Quaternion.Inverse(cam.localRotation);
                targetPos = focusView.position - targetRot * cam.localPosition;
            }
        }
        else if (target != null)
        {
            targetPos = target.position + offset;
            targetRot = defaultRotation;
        }
        else
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
}