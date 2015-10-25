/*
This file is part of Dynamic IVA.

Dynamic IVA is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Dynamic IVA is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Dynamic IVA.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace DynamicIVA {

    [KSPAddon(KSPAddon.Startup.Flight, false)]
	public class DIVA_CameraWatcher : MonoBehaviour
	{
		CameraManager.CameraMode lastCameraMode;

		void Awake ()
		{
			lastCameraMode = CameraManager.Instance.currentCameraMode;
		}

		void UpdateDIVA (InternalProp prop)
		{
			foreach (InternalModule mod in prop.internalModules) {
				if (mod is DIVA_InternalModelSwitch) {
					(mod as DIVA_InternalModelSwitch).SetVisible ();
				}
			}
		}

		void SetInternalsVisible ()
		{
			var vessel = FlightGlobals.ActiveVessel;
			foreach (var part in vessel.parts) {
				if (part.internalModel != null) {
					part.internalModel.SetVisible (true);
					foreach (var prop in part.internalModel.props) {
						UpdateDIVA (prop);
					}
				}
			}
		}

		IEnumerator WaitAndSetInternalsVisible ()
		{
			yield return null;
			SetInternalsVisible ();
		}

		void Update ()
		{
			var current = CameraManager.Instance.currentCameraMode;
			if (current == CameraManager.CameraMode.IVA) {
				if (lastCameraMode != current) {
					lastCameraMode = current;
					StartCoroutine (WaitAndSetInternalsVisible ());
				}
			} else {
				lastCameraMode = current;
			}
		}
	}
}
