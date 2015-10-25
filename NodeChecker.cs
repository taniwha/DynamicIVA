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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace DynamicIVA {

	public class DIVA_NodeChecker : PartModule
	{
		public class NodeInfo
		{
			public string name { get; private set; }
			public string type { get; private set; }

			public bool connected { get; private set; }

			AttachNode attachNode;
			Part part;

			public NodeInfo (Part part, ConfigNode node)
			{
				name = node.GetValue ("name");
				type = node.GetValue ("type");
				this.part = part;
			}

			public void Load (ConfigNode node)
			{
				if (node.HasValue ("connected")) {
					connected = bool.Parse (node.GetValue ("connected"));
				}
			}

			public void Save (ConfigNode node)
			{
				node.AddValue ("connected", connected);
			}

			public void Start (PartModule.StartState state)
			{
				attachNode = part.findAttachNode (name);
				if (state == PartModule.StartState.Editor) {
					Update ();
				}
			}

			public void Update ()
			{
				connected = attachNode.attachedPart != null;
			}
		}

		public string configString;
		public List<NodeInfo> node_infos { get; private set; }

		void onEditorShipModified (ShipConstruct ship)
		{
			foreach (var ni in node_infos) {
				ni.Update ();
			}
		}

		public override void OnAwake ()
		{
			node_infos = new List<NodeInfo> ();
		}

		public override void OnStart (PartModule.StartState state)
		{
			if (state == PartModule.StartState.None) {
				return;
			}
			if (state == PartModule.StartState.Editor) {
				ConfigNode n = ConfigNode.Parse (configString).GetNode("MODULE");
				CreateNodeInfos (n);
				GameEvents.onEditorShipModified.Add (onEditorShipModified);
			}
			foreach (var ni in node_infos) {
				ni.Start (state);
			}
		}

		void OnDestroy ()
		{
			GameEvents.onEditorShipModified.Remove (onEditorShipModified);
		}

		void CreateNodeInfos (ConfigNode node)
		{
			var ni_list = node.GetNodes ("NodeInfo");
			node_infos = new List<NodeInfo> ();
			foreach (var ni_node in ni_list) {
				var ni = new NodeInfo (part, ni_node);
				ni.Load (ni_node);
				node_infos.Add (ni);
			}
		}

		void LoadNodeInfos (ConfigNode node)
		{
			var ni_list = node.GetNodes ("NodeInfo");
			for (int i = 0; i < ni_list.Length; i++) {
				node_infos[i].Load (ni_list[i]);
			}
		}

		public override void OnLoad (ConfigNode node)
		{
			if (configString == null) {
				configString = node.ToString ();
				CreateNodeInfos (node);
			} else {
				ConfigNode n = ConfigNode.Parse (configString).GetNode("MODULE");
				CreateNodeInfos (n);
				LoadNodeInfos (node);
			}
		}

		public override void OnSave (ConfigNode node)
		{
			foreach (var ni in node_infos) {
				ni.Save (node.AddNode ("NodeInfo"));
			}
		}
	}
}
