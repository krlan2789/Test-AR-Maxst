using maxstAR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LAN
{
    public class PanelController : MonoBehaviour
    {
        [SerializeField] private GameObject panelBtnGroup;
        [SerializeField] private FixedJoystick joystick;
        [SerializeField] private float zoomStep = .1f;

        public readonly Dictionary<string, PlayerController> imageTrackablesMap = new();
        private readonly Dictionary<string, Vector3> initialZoomMap = new();
        private readonly float maxZoom = 32;

        public FixedJoystick Joystick { get { return joystick; }  }

        private void Start() {
            SetActive(false);

            imageTrackablesMap.Clear();
            initialZoomMap.Clear();
            ImageTrackableBehaviour[] imageTrackables = FindObjectsOfType<ImageTrackableBehaviour>();
            foreach (var trackable in imageTrackables) {
                var player = trackable.GetComponentInChildren<PlayerController>();
                imageTrackablesMap.Add(trackable.TrackableName, player);
                initialZoomMap.Add(trackable.TrackableName, player.Body ? player.Body.localScale : Vector3.one);
                Debug.Log("Trackable add: " + trackable.TrackableName);
            }

            ZoomReset();
        }

        public void SetActive(bool active) {
            panelBtnGroup.SetActive(active);
            joystick.gameObject.SetActive(active);
        }

        public void ZoomReset() {
            foreach (var item in imageTrackablesMap) {
                if (initialZoomMap.ContainsKey(item.Key) && item.Value.Body) item.Value.Body.localScale = initialZoomMap[item.Key];
            }
        }

        public void ZoomIn() {
            foreach (var item in imageTrackablesMap) {
                if (item.Value.IsTracked) {
                    var player = item.Value.GetComponentInChildren<PlayerController>();
                    player.Body.localScale = new(player.Body.localScale.x + zoomStep, player.Body.localScale.y + zoomStep, player.Body.localScale.z + zoomStep);
                    if (player.Body.localScale.x > zoomStep * maxZoom) player.Body.localScale = Vector3.one * (zoomStep * maxZoom);
                }
            }
        }

        public void ZoomOut() {
            foreach (var item in imageTrackablesMap) {
                if (item.Value.IsTracked) {
                    var player = item.Value.GetComponentInChildren<PlayerController>();
                    player.Body.localScale = new(player.Body.localScale.x - zoomStep, player.Body.localScale.y - zoomStep, player.Body.localScale.z - zoomStep);
                    if (player.Body.localScale.x < zoomStep) player.Body.localScale = initialZoomMap[item.Key];
                }
            }
        }
    }
}
