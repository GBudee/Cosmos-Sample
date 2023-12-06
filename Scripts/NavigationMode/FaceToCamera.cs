using UnityEngine;

namespace NavigationMode
{
    public class FaceToCamera : MonoBehaviour
    {
        [SerializeField] private float _Pitch = 35;
        
        void LateUpdate()
        {
            var towardCamera = transform.position - Camera.main.transform.position;
            var planarFacing = transform.parent.InverseTransformDirection(Vector3.ProjectOnPlane(towardCamera, transform.parent.up));
            float angle = Mathf.Atan2(planarFacing.x, planarFacing.z) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(_Pitch, angle, 0);
        }
    }
}