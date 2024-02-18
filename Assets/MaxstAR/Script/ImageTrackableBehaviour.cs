﻿/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.IO;
using JsonFx.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using UnityEngine.Rendering;

namespace maxstAR {
	public class ImageTrackableBehaviour : AbstractImageTrackableBehaviour {
		public bool IsTracked { get; private set; }

		public override void OnTrackSuccess(string id, string name, Matrix4x4 poseMatrix) {
			Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
			Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

			// Enable renderers
			foreach (Renderer component in rendererComponents) {
				component.enabled = true;
			}

			// Enable colliders
			foreach (Collider component in colliderComponents) {
				component.enabled = true;
			}

			transform.position = MatrixUtils.PositionFromMatrix(poseMatrix);
			transform.rotation = MatrixUtils.QuaternionFromMatrix(poseMatrix);
			IsTracked = true;
		}

		public override void OnTrackFail() {
			Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
			Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

			// Disable renderer
			foreach (Renderer component in rendererComponents) {
				component.enabled = false;
			}

			// Disable collider
			foreach (Collider component in colliderComponents) {
				component.enabled = false;
			}

			IsTracked = false;
		}
	}
}