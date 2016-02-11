using System.Linq;
using UnityEngine;

namespace ScienceHardDrives {

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	class HardDriveManager : MonoBehaviour {

		#region Fields

		protected static bool isVisable;

		protected static Vector2 scrollPos;
		protected static Rect windowPos;

		protected static VesselSettings vesselSettings;

		protected static GUISkin skin;

		protected static IScienceDataContainer dataContainer;

		protected static string dataResultText;
		protected static string dataSizeText;
		protected static string dataRecoverText;
		protected static string dataXmitText;

		protected static float dataValue;
		protected static float dataXmitValue;
		protected static float dataRefValue;
		protected static float dataValueAfterRec;
		protected static float dataValueAfterXmit;

		protected static float dataLabBoost;

		#endregion

		#region Create/Destroy Methods

		public void Awake() {
			Print("HardDriveManager started.");

			isVisable = false;

			scrollPos = new Vector2();
			windowPos = new Rect();

			skin = DargonUtils.managerSkin;

			RenderingManager.AddToPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Add(OnVesselChange);
		}

		public void OnDestroy() {
			RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Remove(OnVesselChange);

			Print("HardDriveManager destroyed.");
		}

		#endregion

		#region GUI Methods

		public static void OnDraw() {
			if(isVisable) {
				GUI.skin = skin;
				windowPos = KSPUtil.ClampRectToScreen(GUILayout.Window(170263, windowPos, OnWindow, "Hard Drive Manager", GUILayout.Height(500f), GUILayout.Width(375f)));
			}
		}

		public static void OnWindow(int WindowID) {
			string title;

			if(GUI.Button(new Rect(windowPos.width - 21, 1, 20, 18), "_", skin.FindStyle("expandButton"))) {
				isVisable = false;
			}

			GUILayout.BeginVertical();

			#region Container Selection
			scrollPos = GUILayout.BeginScrollView(scrollPos, skin.FindStyle("selectBox"), GUILayout.Height(188f), GUILayout.Width(382f));

			foreach(IScienceDataContainer container in vesselSettings.containers) {
				GUILayout.BeginHorizontal();

				if(container.GetScienceCount() > 0) {
					if(GUILayout.Button(vesselSettings.expanded.Contains(container) ? "-" : "+", skin.FindStyle("expandbutton"))) {
						vesselSettings.ExpandOrCollapseContainer(container);

						if(vesselSettings.selectedData != null && container.GetData().Contains(vesselSettings.selectedData)) {
							vesselSettings.selectedData = null;
						}
					}
				}
				else {
					GUILayout.Label("-", skin.FindStyle("expandLabel"));
				}

				if(GUILayout.Button((title = ((PartModule)container).part.partInfo.title).Length > 40 ? (title.Substring(0, 40) + "...") : title, skin.FindStyle(container == vesselSettings.selectedContainer ? "selectButtonDown" : "selectButtonUp"))) {
					if(container == vesselSettings.selectedContainer) {
						vesselSettings.SelectContainer(null);
					}
					else {
						vesselSettings.SelectContainer(container);
					}
				}

				GUILayout.EndHorizontal();

				#region Data Selection

				if(vesselSettings.expanded.Contains(container)) {
					foreach(ScienceData data in container.GetData().OrderBy(d => d.title)) {
						GUILayout.BeginHorizontal();

						GUILayout.Label("\u2514", skin.FindStyle("expandLabel"));
						if(GUILayout.Button((title = data.title).Count() > 40 ? (title.Substring(0, 40) + "...") : title, skin.FindStyle(data == vesselSettings.selectedData ? "selectButtonDown" : "selectButtonUp"))) {

							if(data == vesselSettings.selectedData) {
								vesselSettings.SelectData(null);
							}
							else {
								vesselSettings.SelectData(data);
								UpdateScienceInfo(data);
							}
						}
						GUILayout.EndHorizontal();
					}
				}
				#endregion
			}

			GUILayout.EndScrollView();
			#endregion

			Rect temp = GUILayoutUtility.GetLastRect();

			#region Info

			if(vesselSettings.selectedContainer != null) {
			}
			else if(vesselSettings.selectedData != null) {
				GUILayout.BeginHorizontal();

				#region Data Info
				GUILayout.BeginVertical();

				GUIStyle resultField = skin.FindStyle("resultfield");
				GUIStyle icons = skin.FindStyle("icons");
				GUIStyle iconstext = skin.FindStyle("iconstext");

				GUILayout.BeginHorizontal();
				GUILayout.Label(vesselSettings.selectedData.title);
				if(GUILayout.Button("X", skin.GetStyle("deselectButton"))) {
					vesselSettings.selectedContainer = null;
					vesselSettings.selectedData = null;
				}
				GUILayout.EndHorizontal();

				GUILayout.Box(dataResultText);

				GUILayout.BeginHorizontal(resultField);
				GUILayout.Box(DargonUtils.dataIcon, icons);
				GUILayout.Label(dataSizeText, iconstext);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(resultField);
				GUILayout.Box(DargonUtils.scienceIcon, icons);
				GUILayout.Label(dataRecoverText, iconstext);
				GUILayout.FlexibleSpace();
				GUIUtil.Layout.ProgressBar(0f, dataValue / dataRefValue, skin.FindStyle("progressBarBG"), skin.FindStyle("progressBarFill"), GUILayout.Width(100f));
				GUIUtil.ProgressBar(GUILayoutUtility.GetLastRect(), 0f, dataValue / dataRefValue - dataValueAfterRec / dataRefValue, skin.FindStyle("progressBarFill2"));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(resultField);
				GUILayout.Box(DargonUtils.scienceIcon, icons);
				GUILayout.Label(dataXmitText, iconstext);
				GUILayout.FlexibleSpace();
				GUIUtil.Layout.ProgressBar(0f, dataXmitValue / dataRefValue, skin.FindStyle("progressBarBG"), skin.FindStyle("progressBarFill3"), GUILayout.Width(100f));
				GUIUtil.ProgressBar(GUILayoutUtility.GetLastRect(), 0f, dataXmitValue / dataRefValue - dataValueAfterXmit / dataRefValue, skin.FindStyle("progressBarFill4"));
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				#endregion

				#region Buttons
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.Space(5);
				if(GUILayout.Button("", skin.FindStyle(dataContainer.IsRerunnable() ? "discard button" : "reset button"))) {
					Print(vesselSettings.selectedData.title + " dumped.");

					dataContainer.ReviewDataItem(vesselSettings.selectedData);
					ExperimentsResultDialog.Instance.currentPage.OnDiscardData(vesselSettings.selectedData);
					vesselSettings.selectedData = null;
					if(dataContainer.GetData().Length == 0) {
						vesselSettings.CollapseContainer(dataContainer);
					}
				}
				if(GUILayout.Button("", skin.FindStyle("lab button"))) {
					if(ModuleScienceLab.IsLabData(FlightGlobals.ActiveVessel, vesselSettings.selectedData)) {
						Print(vesselSettings.selectedData.title + " is set to be sent to the lab.");

						dataContainer.ReviewDataItem(vesselSettings.selectedData);
						ExperimentsResultDialog.Instance.currentPage.OnSendToLab(vesselSettings.selectedData);
						vesselSettings.selectedData = null;
						if(dataContainer.GetData().Length == 0) {
							vesselSettings.CollapseContainer(dataContainer);
						}
					}
					else {
						Print(vesselSettings.selectedData.title + " is not lab data.");
					}
				}
				GUILayout.Space(5);
				if(GUILayout.Button((dataXmitValue / dataRefValue * 100f) + "%", skin.FindStyle("transmit button"))) {
					Print(vesselSettings.selectedData.title + " is set to be transmitted.");

					dataContainer.ReviewDataItem(vesselSettings.selectedData);
					ExperimentsResultDialog.Instance.currentPage.OnTransmitData(vesselSettings.selectedData);
					vesselSettings.selectedData = null;
					if(dataContainer.GetData().Length == 0) {
						vesselSettings.CollapseContainer(dataContainer);
					}
				}
				GUILayout.Space(5);
				if(GUILayout.Button("", skin.FindStyle("transferButton"))) {
					Print(vesselSettings.selectedData.title + " is set to be transfered.");

					int i = 0;
					Callback<ScienceHardDrive> driveSelectCallback = new Callback<ScienceHardDrive>(DriveSelectCallback);
					DialogOption<ScienceHardDrive>[] dialogOptions = new DialogOption<ScienceHardDrive>[vesselSettings.containers.Where(c => c is ScienceHardDrive).Count() + 1];

					foreach(ScienceHardDrive d in vesselSettings.containers.Where(c => c is ScienceHardDrive).ToArray()) {
						dialogOptions[i++] = new DialogOption<ScienceHardDrive>(d.part.partInfo.title, driveSelectCallback, d);
					}
					dialogOptions[i] = new DialogOption<ScienceHardDrive>("Cancel", driveSelectCallback, null);

					PopupDialog.SpawnPopupDialog(new MultiOptionDialog("Select which drive to transfer ScienceData to.", "", HighLogic.Skin, dialogOptions), false, HighLogic.Skin);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				#endregion

				GUILayout.EndHorizontal();
			}
			else {
			}

			#endregion

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		#endregion

		#region Event Methods

		protected void OnVesselChange(Vessel vessel) {
			Print("Vessel was changed");
			if(FlightGlobals.ActiveVessel == vessel) {
				vesselSettings = vessel.GetComponent<VesselSettings>();
				isVisable = vesselSettings.containers.Count() > 0;
			}
		}

		#endregion

		#region Callbacks

		protected static void DriveSelectCallback(ScienceHardDrive drive) {
			if(drive != null) {
				IScienceDataContainer container = vesselSettings.containers.First(c => ((PartModule)c).part.flightID == vesselSettings.selectedData.container);

				if(container.IsRerunnable()) {
					QueueManager.instance.QueueDataForXfer(drive, container, vesselSettings.selectedData);
					vesselSettings.selectedData = null;
				}
				else {
					Callback<ScienceHardDrive> xferAnywaysCallback = new Callback<ScienceHardDrive>(XferAnywaysCallback);

					DialogOption<ScienceHardDrive>[] dialogOptions = new DialogOption<ScienceHardDrive>[2];
					dialogOptions[0] = new DialogOption<ScienceHardDrive>("Transfer Anyways", xferAnywaysCallback, drive);
					dialogOptions[1] = new DialogOption<ScienceHardDrive>("Abort Transfer", xferAnywaysCallback, null);
					PopupDialog.SpawnPopupDialog(new MultiOptionDialog("Transfering science from a nonrerunnable experiment will cause it to become inoperable.", "Warning", HighLogic.Skin, dialogOptions), false, HighLogic.Skin);
				}
			}
			else {
				Print("Transfer canceled.");
			}
		}

		protected static void XferAnywaysCallback(ScienceHardDrive drive) {
			if(drive != null) {
				QueueManager.instance.QueueDataForXfer(drive, vesselSettings.containers.First(c => ((PartModule)c).part.flightID == vesselSettings.selectedData.container), vesselSettings.selectedData);
				vesselSettings.selectedData = null;
			}
			else {
				Print("Transfer canceled.");
			}
		}

		#endregion

		#region Other Methods

		public static void ReviewContainer(IScienceDataContainer container) {
			if(vesselSettings.SelectData(container.GetData().OrderBy(d => d.title).First())) {
				UpdateScienceInfo(vesselSettings.selectedData);
				isVisable = true;
			}
		}

		public static void ReviewData(ScienceData data) {
			if(vesselSettings.SelectData(data)) {
				UpdateScienceInfo(vesselSettings.selectedData);
				isVisable = true;
			}
		}

		public static void UpdateScienceInfo(ScienceData data) {
			Print("Upating selected ScienceData information.");

			ScienceSubject subjectId = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
			float scienceMultiplier = HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

			dataContainer = vesselSettings.containers.First(c => ((PartModule)c).part.flightID == data.container);

			dataRefValue = ResearchAndDevelopment.GetReferenceDataValue(data.dataAmount, subjectId) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
			dataValue = ResearchAndDevelopment.GetScienceValue(data.dataAmount, subjectId, 1f) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
			dataXmitValue = ResearchAndDevelopment.GetScienceValue(data.dataAmount, subjectId, data.transmitValue) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
			dataValueAfterRec = ResearchAndDevelopment.GetNextScienceValue(data.dataAmount, subjectId, 1f) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
			dataValueAfterXmit = ResearchAndDevelopment.GetNextScienceValue(data.dataAmount, subjectId, data.transmitValue) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

			dataResultText = ResearchAndDevelopment.GetResults(subjectId.id);
			dataSizeText = "Data Size: " + data.dataAmount + " Mits";
			dataRecoverText = "Recovery: +" + dataValue.ToString("0.0") + " Science";
			dataXmitText = "Transmit: +" + dataXmitValue.ToString("0.0") + " Science";


		}

		#endregion

		#region Utility Methods

		private static void Print(string toPrint) {
			DargonUtils.Print("HDM", toPrint);
		}

		#endregion
	}
}