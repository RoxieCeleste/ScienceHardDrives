﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceHardDrives {

	[KSPModule("ScienceHardDrive")]
	public class ScienceHardDrive : PartModule, IScienceDataContainer {

		#region Fields

		protected float _usedSpace = 0f;
		protected bool xfering = false;

		protected List<ScienceData> storedData = new List<ScienceData>();

		#endregion

		#region Properties
		public float freeSpace {
			get {
				return capacity - usedSpace;
			}
		}

		public float usedSpace {
			get {
				return _usedSpace;
			}
			set {
				_usedSpace = value;
				percentFull = Mathf.Round(1000 * usedSpace / maxCapacity) / 10;
			}
		}
		#endregion

		#region KSPFields (Hidden)

		[KSPField(guiActive = false, isPersistant = true)]
		public float maxCapacity = 1f;

		[KSPField(guiActive = false, isPersistant = true)]
		public float xferRate = 0f;

		[KSPField(guiActive = false, isPersistant = true)]
		public float chunkSize = 1f;

		[KSPField(guiActive = false, isPersistant = true)]
		public float powerUsage = 0f;

		[KSPField(guiActive = false, isPersistant = true)]
		public bool storeDups = false;

		#endregion

		#region KSPFields(Shown)

		[KSPField(guiActive = true, guiName = "Capacity", guiUnits = " Mits", isPersistant = true), UI_FloatRange(scene = UI_Scene.Editor)]
		public float capacity = 0f;

		[KSPField(guiActive = true, guiName = "Percent Full", guiUnits = "%", isPersistant = false)]
		public float percentFull = 0f;

		#endregion

		#region KSPEvents
		[KSPEvent(guiName = "Review Data", guiActive = true, active = true, name = "ReviewDataEvent")]
		public void ReviewDataEvent() {
			HardDriveManager.ReviewContainer(this);
		}

		#endregion

		#region PartModule Methods

		public override string GetInfo() {
			StringBuilder info = new StringBuilder("Max Capacity: " + maxCapacity + " Mits.\n");
			info.Append("Transfer rate: " + xferRate + " Mits/s.\n");
			info.Append("\nRequires:\n");
			info.Append("-ElectricCharge: " + powerUsage + "/s during data transfers.");
			return info.ToString();
		}

		public override void OnLoad(ConfigNode node) {
			base.OnLoad(node);

			foreach(ConfigNode dataNode in node.GetNodes("ScienceData")) {
				AddData(new ScienceData(dataNode));
			}
		}

		public override void OnSave(ConfigNode node) {
			base.OnSave(node);

			node.RemoveNodes("ScienceData");
			foreach(ScienceData data in storedData) {
				data.Save(node.AddNode("ScienceData"));
			}
		}

		public override void OnStart(StartState state) {
			base.OnStart(state);

			if(maxCapacity <= 0) {
				maxCapacity = 1f;
				DargonUtils.Print("Check config for " + part.partInfo + ", maxCapacity <= 0.");
			}
			if(xferRate < 0) {
				xferRate = 0;
				DargonUtils.Print("Check config for " + part.partInfo + ", xferRate < 0.");
			}
			if(chunkSize <= 0) {
				chunkSize = 1f;
				DargonUtils.Print("Check config for " + part.partInfo + ", chunkSize < 1");
			}
			if(powerUsage < 0) {
				powerUsage = 0f;
				DargonUtils.Print("Check config for " + part.partInfo + ", powerUsage < 0");
			}

			if(state == StartState.Editor) {
				UI_FloatRange capacityFloatRange = ((UI_FloatRange)Fields["capacity"].uiControlEditor);
				capacityFloatRange.minValue = 0f;
				capacityFloatRange.maxValue = maxCapacity;
				capacityFloatRange.stepIncrement = chunkSize;
				capacity = maxCapacity;
			}
		}

		#endregion

		#region ISCienceDataContainer Methods

		public void DumpData(ScienceData data) {
			storedData.Remove(data);
			usedSpace -= data.dataAmount;
		}

		public ScienceData[] GetData() {
			return storedData.ToArray();
		}

		public int GetScienceCount() {
			return storedData.Count();
		}

		public bool IsRerunnable() {
			return false;
		}

		public void ReturnData(ScienceData data) {
			AddData(data);
		}

		public void ReviewData() {
			HardDriveManager.ReviewContainer(this);
		}

		public void ReviewDataItem(ScienceData data) {
			HardDriveManager.ReviewData(data);
		}

		#endregion

		#region ExperimentResultDialogPage Callbacks
		#endregion

		#region Other Methods
		public void AddData(ScienceData data) {
			if(data != null) {
				storedData.Add(data);
				data.container = part.flightID;
				usedSpace += data.dataAmount;
			}
		}

		#endregion

	}
}
