using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceHardDrives {

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class HardDriveManager : MonoBehaviour {

		#region Fields

		protected static bool isVisable;

		protected static Rect windowPos;
		protected static Vector2 scrollPos;

		protected static ScienceHardDrive[] drives;

		protected static Part[] containers;
		protected static List<Part> viewing;

		protected static ScienceHardDrive selectedDrive;
		protected static ScienceData selectedData;

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
			RenderingManager.AddToPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Add(OnVesselChange);
			GameEvents.onVesselWasModified.Add(OnVesselWasModified);
			windowPos = new Rect();
			scrollPos = new Vector2();
			isVisable = false;
			skin = DargonUtils.managerSkin;

		}

		public void OnDestroy() {
			RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Remove(OnVesselChange);
			GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
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

			GUILayout.BeginHorizontal();

			#region Left Side
			GUILayout.BeginVertical();

			#region Hard Drive Selection
			scrollPos = GUILayout.BeginScrollView(scrollPos, skin.FindStyle("selectBox"), GUILayout.Height(188f));
			
			foreach(ScienceHardDrive drive in drives) {
				GUILayout.BeginHorizontal();

				if(drive.GetScienceCount() > 0) {
					if(GUILayout.Button(drive.viewing ? "-" : "+", skin.FindStyle("expandbutton"))) {
						drive.viewing ^= true;

						if(selectedData != null && drive.GetData().Contains(selectedData)) {
							selectedData = null;
						}
					}
				}
				else {
					GUILayout.Label("-", skin.FindStyle("expandLabel"));
				}

				if(GUILayout.Button((title = drive.part.partInfo.title).Length > 30 ? (title.Substring(0, 30) + "...") : title, skin.FindStyle(drive == selectedDrive ? "selectButtonDown" : "selectButtonUp"))) {
					selectedData = null;
					if(selectedDrive == drive) {
						selectedDrive = null;
					}
					else {
						selectedDrive = drive;
					}
				}

				GUILayout.EndHorizontal();

				#region Data Selection

				if(drive.viewing) {
					foreach(ScienceData data in drive.GetData().OrderBy(d => d.title)) {
						GUILayout.BeginHorizontal();

						GUILayout.Label("\u2514", skin.FindStyle("expandLabel"));
						if(GUILayout.Button((title = data.title).Count() > 30 ? (title.Substring(0, 30) + "...") : title, skin.FindStyle(data == selectedData ? "selectButtonDown" : "selectButtonUp"))) {
							selectedDrive = null;

							if(selectedData == data) {
								selectedData = null;
							}
							else {
								selectedData = data;

								ScienceSubject subjectId = ResearchAndDevelopment.GetSubjectByID(selectedData.subjectID);
								float scienceMultiplier = HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

								dataRefValue = ResearchAndDevelopment.GetReferenceDataValue(selectedData.dataAmount, subjectId) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								dataValue = ResearchAndDevelopment.GetScienceValue(selectedData.dataAmount, subjectId, 1f) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								dataXmitValue = ResearchAndDevelopment.GetScienceValue(selectedData.dataAmount, subjectId, selectedData.transmitValue) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								dataValueAfterRec = ResearchAndDevelopment.GetNextScienceValue(selectedData.dataAmount, subjectId, 1f) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								dataValueAfterXmit = ResearchAndDevelopment.GetNextScienceValue(selectedData.dataAmount, subjectId, selectedData.transmitValue) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

								dataResultText = ResearchAndDevelopment.GetResults(subjectId.id);
								dataSizeText = "Data Size: " + selectedData.dataAmount + " Mits";
								dataRecoverText = "Recovery: +" + dataValue.ToString("0.0") + " Science";
								dataXmitText = "Transmit: +" + dataXmitValue.ToString("0.0") + " Science";
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
			GUILayout.BeginHorizontal();

			if(selectedDrive != null) {
			}
			else if(selectedData != null) {
				GUILayout.BeginVertical();

				GUIStyle resultField = skin.FindStyle("resultfield");
				GUIStyle icons = skin.FindStyle("icons");
				GUIStyle iconstext = skin.FindStyle("iconstext");

				GUILayout.Label(selectedData.title);
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

				#region Buttons
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.Button("", skin.FindStyle("discard button"));
				GUILayout.FlexibleSpace();
				GUILayout.Button("", skin.FindStyle("lab button"));
				GUILayout.FlexibleSpace();
				GUILayout.Button((dataXmitValue / dataRefValue * 100f) + "%", skin.FindStyle("transmit button"));
				GUILayout.FlexibleSpace();
				GUILayout.Button("", skin.FindStyle("transferButton"));
				GUILayout.FlexibleSpace();



				GUILayout.EndVertical();

				#endregion

			}
			else {
			}

			GUILayout.EndHorizontal();

			#endregion

			GUILayout.EndVertical();
			#endregion

			GUILayout.EndHorizontal();
			GUI.DragWindow();

		}

		#endregion

		#region Event Methods

		protected void OnVesselChange(Vessel vessel) {
			UpdateDriveList();
		}

		protected void OnVesselWasModified(Vessel vessel) {
			UpdateDriveList();
		}

		#endregion

		#region Other Methods

		protected static void UpdateDriveList() {
			drives = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ScienceHardDrive>().OrderBy(d => d.part.partInfo.title).ToArray();
			isVisable = drives.Length > 0;
			if(!drives.Contains(selectedDrive)) {
				selectedData = null;
				selectedDrive = null;
			}
		}

		public static void ReviewDrive(ScienceHardDrive drive) {
			ReviewData(drive, drive.GetData().OrderBy(d => d.title).ToArray()[0]);
		}

		public static void ReviewData(ScienceHardDrive drive, ScienceData data) {
			isVisable = true;
			drive.viewing = true;
			selectedData = data;
		}

		private void promptCallBack(bool xfer) {
			if(xfer) {
			}
		}

		#endregion
	}
}
