/* Copyright 2022 okureya, Omnith LLC. Attributes CC-BY 3.0 */
using System.Collections;
using UnityEngine;

namespace vPad.Components
{
    /// <summary>
    /// A component for objects that enables them to be dragged. The target <see cref="GameObject"/> will be moved
    /// according to the movements of the gripped object.<para/>The <see cref="StartDraggingTarget"/> function must be linked
    /// to a <see cref="VRInteractable"/> grip and preferably triggered through an OnInteract event.
    /// </summary>
    /// <remarks> Written by okureya, thanks to C-137 for clarification </remarks>
    public class vDraggableObject : MonoBehaviour
    {
        public GameObject TargetGripGo;
        public bool handheldMode = true;
        public Vector3 handHeldRotRate = Vector3.one;
        public Vector3 handHeldRotOffset = Vector3.one;
        public bool hasSetInitialTargetPos;

        private bool draggingObject;
        private Vector3 localTargetPos;
        private Vector3 localTargetFwd;
        private Vector3 localTargetUp;

        private readonly Vector3 cameraShakeRate = Vector3.zero;
        private float cameraShakeAmt = 0f;
        private float camShakeDamping = 3f;

        private Quaternion handheldRotation;

        public void StartDraggingTarget(VRInteractable interactable)
        {
            base.StartCoroutine(TargetDragRoutine(interactable.activeController));
        }

        private IEnumerator TargetDragRoutine(VRHandController controller)
        {
            draggingObject = true;
            var interactable = controller.activeInteractable;
            // set transform parent to the hand so we're moving it
            TargetGripGo.transform.parent = controller.transform;
            // wait until we're done dragging the parent
            while (base.enabled && controller.activeInteractable == interactable)
            {
                yield return null;
            }
            draggingObject = false;
            if (TargetGripGo.transform.parent == controller.transform)
            {
                // then update the transform parent back to player local
                TargetGripGo.transform.parent = FlightSceneManager.instance.playerActor.transform;
                Transform parent = TargetGripGo.transform.parent;
                Vector3 position = TargetGripGo.transform.position;
                Vector3 forward = TargetGripGo.transform.forward;
                Vector3 up = TargetGripGo.transform.up;

                localTargetPos = parent.InverseTransformPoint(position);
                localTargetFwd = parent.InverseTransformDirection(forward);
                localTargetUp = parent.InverseTransformDirection(up);
            }
        }

        /// <summary> Updates the handheld rotation of the object. </summary>
        private void UpdateHandheldRotation()
        {
            Vector3 forward = TargetGripGo.transform.forward;
            Vector3 right = TargetGripGo.transform.right;
            Vector3 up = TargetGripGo.transform.up;
            Vector3 vector = handHeldRotRate;
            Vector3 vector2 = handHeldRotOffset;

            vector *= 2f;
            vector2 /= 2f;
            handheldRotation = Quaternion.identity;
            if (cameraShakeAmt > 0.001)
            {
                float angle4 = VectorUtils.FullRangePerlinNoise(12.62f, cameraShakeRate.x * Time.time) * vector2.x * cameraShakeAmt;
                float angle5 = VectorUtils.FullRangePerlinNoise(134.142f, cameraShakeRate.y * Time.time) * vector2.y * cameraShakeAmt;
                float angle6 = VectorUtils.FullRangePerlinNoise(1502.235f, cameraShakeRate.z * Time.time) * vector2.z * cameraShakeAmt;
                cameraShakeAmt = Mathf.Lerp(cameraShakeAmt, 0f, camShakeDamping * Time.deltaTime);
                handheldRotation = Quaternion.AngleAxis(angle6, forward) * Quaternion.AngleAxis(angle4, right) * Quaternion.AngleAxis(angle5, up) * handheldRotation;
            }
        }

        /// <summary> Sets the initial target object position. </summary>
        public void SetupTargetPosition()
        {
            TargetGripGo.transform.parent = FlightSceneManager.instance.playerActor.transform;
            if (!hasSetInitialTargetPos)
            {
                Transform parent = TargetGripGo.transform.parent;
                // position the target initially 0.4m in front of the player's face
                Vector3 position2 = VRHead.instance.transform.position + parent.forward * 0.4f;
                Vector3 direction2 = -parent.forward;
                Vector3 up = parent.up;
                localTargetPos = parent.InverseTransformPoint(position2);
                localTargetFwd = parent.InverseTransformDirection(direction2);
                localTargetUp = parent.InverseTransformDirection(up);
                hasSetInitialTargetPos = true;
            }
            TargetGripGo.transform.localPosition = localTargetPos;
            TargetGripGo.transform.localRotation = Quaternion.LookRotation(localTargetFwd, localTargetUp);
        }

        /// <summary> Called towards the end of the frame update by Unity. </summary>
        private void LateUpdate()
        {
            // we simply check to update handheld rotation, depending on state
            if (handheldMode)
            {
                UpdateHandheldRotation();
                if (!draggingObject)
                {
                    TargetGripGo.transform.localRotation =
                        handheldRotation * Quaternion.LookRotation(localTargetFwd, localTargetUp);
                }
                else
                {
                    TargetGripGo.transform.rotation = handheldRotation * TargetGripGo.transform.rotation;
                }
            }
        }
    }
}
