using maxstAR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LAN {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private float speed;
        [SerializeField] private Transform body;
        [SerializeField] private PanelController panelController;

        private ImageTrackableBehaviour trackableBehaviour;

        public Transform Body=> body;
        public bool IsTracked => trackableBehaviour.IsTracked;

        private void Start() {
            if (body == null) body = transform;
            trackableBehaviour = transform.parent.GetComponent<ImageTrackableBehaviour>();
        }

        private void SetActive(bool active) {
            if (panelController != null) panelController.SetActive(active);
            //else Debug.LogError("PanelController is empty!");
        }

        public void FixedUpdate() {
            SetActive(trackableBehaviour.IsTracked);

            if (panelController != null && panelController.Joystick != null) {
                Vector3 direction = (Vector3.down * panelController.Joystick.Vertical) + (Vector3.right * panelController.Joystick.Horizontal);
                body.Translate((speed / 10) * Time.fixedDeltaTime * direction);
            }
        }
    }
}
