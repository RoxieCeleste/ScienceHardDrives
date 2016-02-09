using System.Collections.Generic;
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

		protected static string dataResultText;
		protected static string dataSizeText;
		protected static string dataRecoverText;
		protected static string dataXmitText;

		protected static float dataValue;
		protected static float dataXmitValue;
		protected static float dataRefValue;
		protected static float dataValueAfterRec;
		protected static float dataValueAfterXmit;

		#endregion

		#region Create/Destroy Methods

		public void Awake() {
			DargonUtils.Print("HardDriveManager started.");

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

			DargonUtils.Print("HardDriveManager destroyed.");
		}

		#endregion

		#region GUI Methods

		public static void OnDraw() {
			if(isVisable) {
				GUI.skin = skin;
				windowPos = KSPUtil.ClampRectToScreen(GUILayout.Window(170263, windowPos, OnWindow, "Hard Drive Manager"));
			}
		}

		public static void OnWindow(int WindowID) {
			string title;

			GUILayout.BeginVertical();

			#region Container Selection
			scrollPos = GUILayout.BeginScrollView(scrollPos, skin.FindStyle("selectBox"), GUILayout.Height(188f));

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
					vesselSettings.SelectContainer(container);
				}

				GUILayout.EndHorizontal();

				#region Data Selection

				if(vesselSettings.expanded.Contains(container)) {
					foreach(ScienceData data in container.GetData().OrderBy(d => d.title)) {
						GUILayout.BeginHorizontal();

						GUILayout.Label("\u2514", skin.FindStyle("expandLabel"));
						if(GUILayout.Button((title = data.title).Count() > 40 ? (title.Substring(0, 40) + "...") : title, skin.FindStyle(data == vesselSettings.selectedData ? "selectButtonDown" : "selectButtonUp"))) {
							vesselSettings.SelectData(data);

							if(data == vesselSettings.selectedData) {
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
				GUILayout.Button("", skin.FindStyle("discard button"));
				GUILayout.Button("", skin.FindStyle("lab button"));
				GUILayout.Space(15);
				GUILayout.Button((dataXmitValue / dataRefValue * 100f) + "%", skin.FindStyle("transmit button"));
				GUILayout.Space(15);
				GUILayout.Button("", skin.FindStyle("transferButton"));
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
			DargonUtils.Print("Vessel was changed");
			if(FlightGlobals.ActiveVessel == vessel) {
				vesselSettings = vessel.GetComponent<VesselSettings>();
				isVisable = vesselSettings.containers.Count() > 0;
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
			ScienceSubject subjectId = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
			float scienceMultiplier = HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

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
	}
}
