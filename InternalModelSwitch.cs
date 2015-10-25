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

	public class DIVA_InternalModelSwitch : InternalModule
	{
		public class SwitchInfo
		{
			public enum Mode {
				DisableWhenConnected,
				EnableWhenConnected,
			};
			public string nodeName { get; private set; }
			public string transformName { get; private set; }
			public Mode mode { get; private set; }

			public DIVA_NodeChecker.NodeInfo node { get; private set; }
			public Transform transform { get; private set; }

			public InternalProp prop { get; private set; }

			public SwitchInfo (InternalProp prop)
			{
				this.prop = prop;
			}

			public void Load (ConfigNode node)
			{
				nodeName = node.GetValue ("node");
				transformName = node.GetValue ("transform");
				var s = node.GetValue ("mode");
				mode = (Mode) Enum.Parse (typeof (Mode), s);
			}

			public void Save (ConfigNode node)
			{
				node.AddValue ("node", nodeName);
				node.AddValue ("transform", transformName);
				node.AddValue ("mode", mode);
			}

			DIVA_NodeChecker.NodeInfo FindNode ()
			{
				Part part = prop.internalModel.part;
				var checkers = part.FindModulesImplementing<DIVA_NodeChecker>();
				foreach (var cc in checkers) {
					var c = cc as DIVA_NodeChecker;
					foreach (var n in c.node_infos) {
						if (n.name == nodeName) {
							return n;
						}
					}
				}
				return null;
			}

			public void Start ()
			{
				node = FindNode ();
				transform = prop.FindModelTransform (transformName);
				Debug.Log (String.Format ("[DIVA] IMS: {0} {1}", transformName, transform));
				if (node == null) {
					return;
				}
				Update ();
			}

			public void Update ()
			{
				bool enabled = node.connected ^ (mode == Mode.DisableWhenConnected);
				Debug.Log (String.Format ("[DIVA] IMS: {0} {1}", nodeName, enabled));
				foreach (var r in transform.GetComponentsInChildren<Renderer> ()) {
					r.enabled = enabled;
				}
			}
		}

		List<SwitchInfo> switch_infos;
		public string configString;

		public void SetVisible ()
		{
			foreach (var si in switch_infos) {
				si.Update ();
			}
		}

		public override void OnAwake ()
		{
			if (!HighLogic.LoadedSceneIsFlight) {
				return;
			}
			var node = ConfigNode.Parse (configString).GetNode ("MODULE");
			CreateSwitchInfos (node);
			foreach (var si in switch_infos) {
				si.Start ();
			}
		}

		void CreateSwitchInfos (ConfigNode node)
		{
			var si_list = node.GetNodes ("SwitchInfo");
			switch_infos = new List<SwitchInfo> ();
			foreach (var si_node in si_list) {
				var si = new SwitchInfo (internalProp);
				si.Load (si_node);
				switch_infos.Add (si);
			}
		}

		public override void OnLoad (ConfigNode node)
		{
			configString = node.ToString ();
			switch_infos = new List<SwitchInfo> ();
			CreateSwitchInfos (node);
		}

		public override void OnSave (ConfigNode node)
		{
			//foreach (var si in switch_infos) {
			//	si.Save (node.AddNode ("SwitchInfo"));
			//}
		}
	}
}
