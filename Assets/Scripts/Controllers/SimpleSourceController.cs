using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class SimpleSourceController : MonoBehaviour
    {
        private bool hasMoved = false; // no movement in the beginning
        private bool movementStopped = true; // no movement in the beginning

        public float upper_limit_y = float.PositiveInfinity;
        public float lower_limit_y = float.NegativeInfinity;
        public float upper_limit_x = float.PositiveInfinity;
        public float lower_limit_x = float.NegativeInfinity;
        public float upper_limit_z = float.PositiveInfinity;
        public float lower_limit_z = float.NegativeInfinity;

        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public float upper_limit_y = float.PositiveInfinity;
            public float lower_limit_y = float.NegativeInfinity;
            public float upper_limit_x = float.PositiveInfinity;
            public float lower_limit_x = float.NegativeInfinity;
            public float upper_limit_z = float.PositiveInfinity;
            public float lower_limit_z = float.NegativeInfinity;


            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;

                ApplyBounds();
            }

            public void ApplyBounds() 
            {
                // check x bounds
                if (x > upper_limit_x)
                {
                    x = upper_limit_x;
                }
                else if (x < lower_limit_x)
                {
                    x = lower_limit_x;
                }

                // check y bounds
                if (y > upper_limit_y) { 
                    y = upper_limit_y;
                }
                else if (y < lower_limit_y) { 
                    y = lower_limit_y;
                }

                // check z bounds
                if (z > upper_limit_z)
                {
                    z = upper_limit_z;
                }
                else if (z < lower_limit_z)
                {
                    z = lower_limit_z;
                }
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;

                ApplyBounds();
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }



            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;

        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (Input.anyKey)
            {
                if (Input.GetKey(KeyCode.U))
                {
                    direction += Vector3.forward;
                    movementStopped = false;
                    hasMoved = true;
                }
                if (Input.GetKey(KeyCode.J))
                {
                    direction += Vector3.back;
                    movementStopped = false;
                    hasMoved = true;
                }
                if (Input.GetKey(KeyCode.H))
                {
                    direction += Vector3.left;
                    movementStopped = false;
                    hasMoved = true;
                }
                if (Input.GetKey(KeyCode.K))
                {
                    direction += Vector3.right;
                    movementStopped = false;
                    hasMoved = true;
                }
                if (Input.GetKey(KeyCode.Y))
                {
                    direction += Vector3.up;
                    movementStopped = false;
                    hasMoved = true;
                }
                if (Input.GetKey(KeyCode.I))
                {
                    direction += Vector3.down;
                    movementStopped = false;
                    hasMoved = true;
                }
            }
            else
            {
                movementStopped = true;
            }
            return direction;
        }

        void Update()
        {

            m_TargetCameraState.lower_limit_x = lower_limit_x;
            m_TargetCameraState.upper_limit_x = upper_limit_x;
            m_TargetCameraState.lower_limit_y = lower_limit_y;
            m_TargetCameraState.upper_limit_y = upper_limit_y;
            m_TargetCameraState.lower_limit_z = lower_limit_z;
            m_TargetCameraState.upper_limit_z = upper_limit_z;

            m_InterpolatingCameraState.lower_limit_x = lower_limit_x;
            m_InterpolatingCameraState.upper_limit_x = upper_limit_x;
            m_InterpolatingCameraState.lower_limit_y = lower_limit_y;
            m_InterpolatingCameraState.upper_limit_y = upper_limit_y;
            m_InterpolatingCameraState.lower_limit_z = lower_limit_z;
            m_InterpolatingCameraState.upper_limit_z = upper_limit_z;


            // Hide and lock cursor when right mouse button pressed
            if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.Period))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            // Unlock and show cursor when right mouse button released
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Rotation
            if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.Period))
            {
                var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }

            // Translation
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (Input.GetKey(KeyCode.LeftShift))
            {
                translation *= 10.0f;
            }

            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            boost += Input.mouseScrollDelta.y * 0.2f;
            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }

        public bool ReachedTarget() {

            CameraState a = m_InterpolatingCameraState;
            CameraState b = m_TargetCameraState;

            return (a.yaw == b.yaw &&
                a.pitch == b.pitch &&
                a.roll == b.roll &&
                a.x == b.x &&
                a.y == b.y &&
                a.z == b.z);
        }

        public void JumpTo(Vector3 position) {

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

            m_TargetCameraState.x = position.x;
            m_TargetCameraState.y = position.y;
            m_TargetCameraState.z = position.z;

            //m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            //m_InterpolatingCameraState.UpdateTransform(transform);

            hasMoved = true;
            movementStopped = false;

        } 

        public bool HasMoved()
        {
            return hasMoved && movementStopped; // only register change in source position when the movement has stopped
        }

        public void AckMovement()
        {
            hasMoved = false;
        }

        public void DirectJumpTo(Vector3 position) {
            m_TargetCameraState.y = position.y;
            m_TargetCameraState.x = position.x;
            m_TargetCameraState.z = position.z;

            m_InterpolatingCameraState.x = m_TargetCameraState.x;
            m_InterpolatingCameraState.y = m_TargetCameraState.y;
            m_InterpolatingCameraState.z = m_TargetCameraState.z;
        }

        public void DirectLookAt(float yaw, float pitch, float roll) {

            m_TargetCameraState.yaw = yaw; m_TargetCameraState.pitch = pitch; m_TargetCameraState.roll = roll;
            m_InterpolatingCameraState.yaw = yaw; m_InterpolatingCameraState.pitch = pitch; m_InterpolatingCameraState.roll = roll;

        }

    }

}