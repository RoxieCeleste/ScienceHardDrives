using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceHardDrives {

	[KSPModule("ScienceHardDrive")]
	public class ScienceHardDrive_old : PartModule, IScienceDataContainer {
		#region Fields

		protected float _usedSpace = 0f;
		private HardDriveManager_old hardDriveManager = null;
		protected List<ScienceData> storedData = new List<ScienceData>();
		protected bool xfering = false;

		public bool viewing = false;

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
				percentful = Mathf.Round(1000 * usedSpace / maxCapacity) / 10;
			}
		}

		#endregion

		#region KSPFields (Hidden)

		[KSPField(guiActive = false, isPersistant = true)]
		public float maxCapacity = 1f;

		[KSPField(guiActive = false, isPersistant = true)]
		public float xferRate = 1f;

		[KSPField(guiActive = false, isPersistant = true)]
		public float chunkSize = 1f;

		[KSPField(guiActive = false, isPersistant = true)]
		public float activePowerUsage = 0f;

		#endregion

		#region KSPFields (Shown)

		[KSPField(guiActive = true, guiName = "Capacity", guiUnits = " Mits", isPersistant = true), UI_FloatRange(scene = UI_Scene.Editor)]
		public float capacity = 0f;

		[KSPField(guiActive = true, guiName = "Percent Full", guiUnits = "%", isPersistant = false)]
		public float percentful = 0f;

		[KSPField(guiActive = false, guiName = "Store Duplicates", isPersistant = true)]
		public bool canStoreDups = false;

		#endregion

		#region KSPEvents

		[KSPEvent(guiName = "Review Data", guiActive = true, active = true, name = "ReviewDataEvent")]
		public void ReviewDataEvent() {
			ReviewData();
		}

		#endregion

		#region PartModule Methods

		public override string GetInfo() {
			StringBuilder info = new StringBuilder("Max Capacity: " + maxCapacity + " mits.\n");
			info.Append("Transfer rate: " + xferRate + " mits/s.\n");
			info.Append("\nRequires:\n");
			info.Append("-ElectricCharge: " + activePowerUsage + "/s during data transfers.");
			return info.ToString();
		}

		public override void OnFixedUpdate() {
			base.OnFixedUpdate();
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

			hardDriveManager = HardDriveManager_old.instance;

			if(maxCapacity <= 0) {
				maxCapacity = 1f;
				print("SHD: Check config for " + part.partInfo.title + ". maxCapacity <= 0");
			}
			if(xferRate <= 0) {
				xferRate = 1f;
				print("SHD: Check config for " + part.partInfo.title + ". xferRate <= 0");
			}
			if(chunkSize <= 0) {
				chunkSize = 1f;
				print("SHD: Check config for " + part.partInfo.title + ". chunkSize <= 0");
			}
			if(activePowerUsage < 0) {
				activePowerUsage = 0f;
				print("SHD: Check config for " + part.partInfo.title + ". activePowerUsage < 0");
			}

			if(state == StartState.Editor) {
				UI_FloatRange capacityFloatRange = ((UI_FloatRange)Fields["capacity"].uiControlEditor);
				capacityFloatRange.minValue = chunkSize;
				capacityFloatRange.stepIncrement = chunkSize;
				capacityFloatRange.maxValue = maxCapacity;
				capacity = maxCapacity;
			}
		}

		public override void OnUpdate() {
			base.OnUpdate();
		}

		#endregion

		#region IScienceDataContainer Methods

		public void DumpData(ScienceData data) {
			storedData.Remove(data);
			usedSpace -= data.dataAmount;
			//ScreenMessages.PostScreenMessage("<color=#ff9900ff>[" + part.partInfo.title + "]: " + data.title + " Removed</color>", 5f, ScreenMessageStyle.UPPER_LEFT);
		}

		public ScienceData[] GetData() {
			return storedData.ToArray();
		}

		public int GetScienceCount() {
			return storedData.Count;
		}

		public bool IsRerunnable() {
			return false;
		}

		public void ReturnData(ScienceData data) {
			AddData(data);
		}

		public void ReviewData() {
			foreach(ScienceData data in storedData) {
				ReviewDataItem(data);
			}
		}

		public void ReviewDataItem(ScienceData data) {
			ExperimentsResultDialog.DisplayResult(new ExperimentResultDialogPage(
				part,
				data,
				data.transmitValue,
				ModuleScienceLab.GetBoostForVesselData(vessel, data),
				data.transmitValue != 1f,
				"Transmitting this data will result in a " + (1f - Mathf.Round(1000 * data.transmitValue) / 10) + "% loss in awarded Science.",
				false,
				ModuleScienceLab.IsLabData(vessel, data),
				new Callback<ScienceData>(OnDiscardData),
				new Callback<ScienceData>(OnKeepData),
				new Callback<ScienceData>(OnTransmitData),
				new Callback<ScienceData>(OnSendDataToLab)));
		}

		#endregion

		#region ExperimentResultDialogPage Callbacks

		public void OnDiscardData(ScienceData data) {
			DumpData(data);
		}

		public void OnKeepData(ScienceData data) {}

		public void OnTransmitData(ScienceData data) {
			List<IScienceDataTransmitter> transList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if(transList.Count > 0) {
				IScienceDataTransmitter trans = transList.OrderBy(t => ScienceUtil.GetTransmitterScore(t)).First(t => t.CanTransmit());
				if(trans != null) {
					trans.TransmitData(new List<ScienceData> { data });
					DumpData(data);
				}
				else {
					ScreenMessages.PostScreenMessage("<color=#ff9900ff>No opperational transmitter on this vessel.</color>", 4f, ScreenMessageStyle.UPPER_CENTER);
				}
			}
			else {
				ScreenMessages.PostScreenMessage("<color=#ff9900ff>No transmitter on this vessel.</color>", 4f, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		public void OnSendDataToLab(ScienceData data) {

		}

		public void OnLabComplete(ScienceData data) {
			ReviewDataItem(data);
		}

		#endregion

		#region Other Methods

		public void AddData(ScienceData data) {
			if(data != null) {
				storedData.Add(data);
				data.container = part.flightID;
				usedSpace += data.dataAmount;
			}
		}

		public bool isXfering() {
			return xfering;
		}

		public IEnumerator transferData() {
			if(!xfering) {
				xfering = true;

				float transferRate = xferRate;
				float secTotal = 0f;
				float secRemaining = 0f;
				ScreenMessage msg = null;
				ScienceData data = null;
				IScienceDataContainer container = null;
				KeyValuePair<ScienceData, IScienceDataContainer> kvp = new KeyValuePair<ScienceData, IScienceDataContainer>();

				while((kvp = hardDriveManager.getNextInQueue(this)).Key != null) {
					data = kvp.Key;
					container = kvp.Value;

					if(data.dataAmount <= capacity - usedSpace) {
						if(container is ScienceHardDrive_old && ((ScienceHardDrive_old)container).xferRate < xferRate) {
							transferRate = ((ScienceHardDrive_old)container).xferRate;
						}

						secTotal = secRemaining = data.dataAmount / transferRate;
						msg = new ScreenMessage("<color=#99ff00ff>Transfering " + data.title + " to " + part.partInfo.title + ":  0%</color>", (float)secTotal + 5f, ScreenMessageStyle.UPPER_LEFT);

						ScreenMessages.PostScreenMessage(msg);
						while(secRemaining > 0 && Mathf.Round((float)part.RequestResource("ElectricCharge", .1 * activePowerUsage) * 10000) == .1 * activePowerUsage * 10000) {
							yield return new WaitForSeconds(.1f);
							secRemaining -= .1f;
							msg.message = "<color=#99ff00ff>Transfering " + data.title + " to " + part.partInfo.title + ": " + Mathf.Round(1000 * (secTotal - secRemaining) / secTotal) / 10 + "%</color>";
						}

						if(secRemaining > 0) {
							ScreenMessages.RemoveMessage(msg);
							ScreenMessages.PostScreenMessage("<color=#ff9900ff>Ran out of Electric Charge while transfering " + data.title + " to " + part.partInfo.title + ".</color>", 5f, ScreenMessageStyle.UPPER_LEFT);
							hardDriveManager.clearQueue(this);
						}
						else if(secRemaining <= 0) {
							msg.message = "<color=#99ff00ff>" + data.title + " transfered to " + part.partInfo.title + ".</color>";
							storedData.Add(data);
							usedSpace += data.dataAmount;
							container.DumpData(data);
						}
					}
					hardDriveManager.removeFromQueue(this, data);
				}

				yield return new WaitForSeconds(1f);
				xfering = false;
			}
		}

		#endregion
	}
}
