using System.Linq;
using UnityEngine;

namespace ScienceHardDrives.src {

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	class NewHardDriveManager : MonoBehaviour {

		#region Fields

		protected Rect windowPos;
		protected bool isVisable;

		protected ScienceHardDrive[] drives;
		protected Vector2 HDDScrollPos;

		protected ScienceHardDrive selectedDrive;
		protected ScienceData selectedData;

		protected string resultText;
		protected string dataSizeText;
		protected string dataRecoverText;
		protected string dataXmitText;

		protected float scienceValue;
		protected float xmitValue;
		protected float refValue;
		protected float valueAfterRec;
		protected float valueAfterXmit;

		protected Texture2D dataIcon;
		protected Texture2D scienceIcon;

		protected GUISkin gameSkin;
		protected GUISkin expSkin;

		protected GUIStyle windowStyle;

		protected GUIStyle selectBoxStyle;

		protected GUIStyle expandButton;
		protected GUIStyle partButton;
		protected GUIStyle dataButton;
		protected GUIStyle expandedLabel;

		protected GUIStyle dataTitleLabel;
		protected GUIStyle dataResultBox;
		protected GUIStyle dataResultField;

		protected GUIStyle iconStyle;
		protected GUIStyle iconTextStyle;

		protected GUIStyle transferButton;
		protected GUIStyle expandWinButton;

		/*-------------------------------------*/

		protected GUIStyle horzStyle;

		protected GUIStyle textLabel;
		protected GUIStyle statLabel;

		protected GUIStyle discBox;

		protected GUIStyle bgProgressBar;
		protected GUIStyle barPrograssBar;

		#endregion

		#region Properties
		#endregion

		#region Create/Destroy

		public void Awake() {
			RenderingManager.AddToPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Add(OnVesselChange);
			GameEvents.onVesselWasModified.Add(OnVesselWasModified);
			windowPos = new Rect();
			isVisable = false;
			SetupSkins();

		}

		public void OnDestroy() {
			RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Remove(OnVesselChange);
			GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
		}

		protected void SetupSkins() {
			GUISkin[] skins = Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[];
			Texture2D[] textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[];

			dataIcon = textures.First(t => t.name.Equals("resultsdialog_datasize"));
			scienceIcon = textures.First(t => t.name.Equals("resultsdialog_scivalue"));

			gameSkin = skins.First(s => s.name.Equals("GameSkin"));
			expSkin = skins.First(s => s.name.Equals("ExperimentsDialogSkin"));

			windowStyle = new GUIStyle(expSkin.window);
			windowStyle.padding = new RectOffset(8, 8, 20, 8);
			windowStyle.fixedWidth = (300f);
			windowStyle.contentOffset = new Vector2(0, -18);
			windowStyle.overflow = new RectOffset(10, 0, 0, 0);

			selectBoxStyle = new GUIStyle(expSkin.customStyles.First(s => s.name.Equals("resultfield")));
			selectBoxStyle.overflow = new RectOffset(0, 1, 1, 2);

			expandButton = new GUIStyle(gameSkin.button);
			expandButton.margin = new RectOffset(4, 0, 0, 0);
			expandButton.fixedHeight = 18f;

			partButton = new GUIStyle(expandButton);
			partButton.margin = new RectOffset(0, 4, 0, 0);
			partButton.alignment = TextAnchor.MiddleLeft;

			dataButton = new GUIStyle(partButton);
			dataButton.margin = new RectOffset(4, 4, 0, 0);

			expandedLabel = new GUIStyle(gameSkin.label);
			expandedLabel.margin = new RectOffset(4, 0, 0, 0);
			expandedLabel.fixedHeight = 18f;
			expandedLabel.alignment = TextAnchor.MiddleCenter;

			dataTitleLabel = new GUIStyle(expSkin.label);

			dataResultBox = new GUIStyle(expSkin.box);

			dataResultField = new GUIStyle(expSkin.customStyles.First(s => s.name.Equals("resultfield")));

			iconStyle = new GUIStyle(expSkin.customStyles.First(s => s.name.Equals("icons")));

			iconTextStyle = new GUIStyle(expSkin.customStyles.First(s => s.name.Equals("iconstext")));

			transferButton = new GUIStyle(expSkin.customStyles.First(s => s.name.Equals("transmit button")));
			transferButton.normal.background = GameDatabase.Instance.GetTexture("SirDargon/ScienceHardDrives/img/manager_transfer_normal", false);
			transferButton.normal.textColor = new Color(164f / 255f, 45f / 255f, 208f / 255f);

			expandWinButton = new GUIStyle(expSkin.button);
			expandWinButton.margin = new RectOffset(8, 4, 0, 0);
			expandWinButton.fixedHeight = 16f;
			expandWinButton.fontSize = 11;



			/*--------------------------------------*/

			horzStyle = new GUIStyle();
			horzStyle.alignment = TextAnchor.MiddleLeft;

			textLabel = new GUIStyle(gameSkin.box);
			textLabel.alignment = TextAnchor.MiddleLeft;

			statLabel = new GUIStyle(textLabel);
			statLabel.fontSize = 11;
			statLabel.margin = new RectOffset(4, 4, 1, 1);

			discBox = new GUIStyle(gameSkin.box);
			discBox.alignment = TextAnchor.UpperLeft;
			discBox.wordWrap = true;

			bgProgressBar = new GUIStyle(expSkin.customStyles[9]);
			bgProgressBar.margin = new RectOffset(0, 0, 0, 4);

			barPrograssBar = new GUIStyle(expSkin.customStyles[10]);
			barPrograssBar.margin = new RectOffset(0, 0, 0, 4);
		}

		#endregion

		#region GUI

		public void OnDraw() {
			if(isVisable) {
				GUI.skin = gameSkin;
				windowPos = KSPUtil.ClampRectToScreen(GUILayout.Window(170264, windowPos, OnWindow, "Hard Drive Manager", windowStyle));
				GUI.skin = null;
			}
		}

		public void OnWindow(int WindowID) {
			string title;
			GUILayout.BeginVertical();// GUILayout.Width(350f));//, GUILayout.Height(600));

			#region Hard Drive Selection

			HDDScrollPos = GUILayout.BeginScrollView(HDDScrollPos, selectBoxStyle, GUILayout.Height(188f));
			foreach(ScienceHardDrive drive in drives) {
				GUILayout.BeginHorizontal();

				if(drive.GetScienceCount() > 0) {
					if(GUILayout.Button(drive.viewing ? "-" : "+", expandButton, GUILayout.Width(20f))) {
						drive.viewing = !drive.viewing;
						selectedData = null;
						selectedDrive = null;
					}
				}
				else {
					GUILayout.Label("-", expandedLabel, GUILayout.Width(20f));
				}
				if(GUILayout.Button((title = drive.part.partInfo.title).Count() > 30 ? (title.Substring(0, 30) + "...") : title, partButton)) {
					if(selectedDrive == drive) {
						selectedDrive = null;
					}
					else {
						selectedDrive = drive;
					}
					selectedData = null;
				}

				GUILayout.EndHorizontal();

				if(drive.viewing) {
					foreach(ScienceData data in drive.GetData().OrderBy(d => d.title)) {
						GUILayout.BeginHorizontal();

						GUILayout.Label("\u2514", expandedLabel, GUILayout.Width(20f));
						if(GUILayout.Button((title = data.title).Count() > 30 ? (title.Substring(0, 30) + "...") : title, partButton)) {
							if(selectedData == data) {
								selectedData = null;
							}
							else {
								selectedData = data;
								ScienceSubject subjectId = ResearchAndDevelopment.GetSubjectByID(selectedData.subjectID);
								float scienceMultiplier = HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

								refValue = ResearchAndDevelopment.GetReferenceDataValue(selectedData.dataAmount, subjectId) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								scienceValue = ResearchAndDevelopment.GetScienceValue(selectedData.dataAmount, subjectId, 1f) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								xmitValue = ResearchAndDevelopment.GetScienceValue(selectedData.dataAmount, subjectId, selectedData.transmitValue) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								valueAfterRec = ResearchAndDevelopment.GetNextScienceValue(selectedData.dataAmount, subjectId, 1f) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
								valueAfterXmit = ResearchAndDevelopment.GetNextScienceValue(selectedData.dataAmount, subjectId, selectedData.transmitValue) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

								resultText = ResearchAndDevelopment.GetResults(subjectId.id);
								dataSizeText = "Data Size: " + selectedData.dataAmount + " Mits";
								dataRecoverText = "Recovery: +" + scienceValue.ToString("0.0") + " Science";
								dataXmitText = "Transmit: +" + xmitValue.ToString("0.0") + " Science";
							}
							selectedDrive = null;
						}
						GUILayout.EndHorizontal();
					}
				}
			}
			GUILayout.EndScrollView();

			#endregion

			if(selectedDrive != null) {
				/*GUILayout.Label(selectedDrive.part.name, textLabel);
				GUILayout.Box(selectedDrive.GetComponent<ScienceHardDrive>().GetInfo(), discBox, GUILayout.Height(200f));*/
			}
			else if(selectedData != null) {
				GUILayout.Label(selectedData.title, dataTitleLabel);
				GUILayout.Box(resultText, dataResultBox, GUILayout.Height(100f));

				GUILayout.BeginHorizontal(dataResultField);
				GUILayout.Box(dataIcon, iconStyle);
				GUILayout.Label(dataSizeText, iconTextStyle);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(dataResultField);
				GUILayout.Box(scienceIcon, iconStyle);
				GUILayout.Label(dataRecoverText, iconTextStyle);
				GUILayout.FlexibleSpace();
				GUIUtil.Layout.ProgressBar(0f, scienceValue / refValue, expSkin.customStyles[9], expSkin.customStyles[10], GUILayout.Width(100f));
				GUIUtil.ProgressBar(GUILayoutUtility.GetLastRect(), 0f, scienceValue / refValue - valueAfterRec / refValue, expSkin.customStyles[11]);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(dataResultField);
				GUILayout.Box(scienceIcon, iconStyle);
				GUILayout.Label(dataXmitText, iconTextStyle);
				GUILayout.FlexibleSpace();
				GUIUtil.Layout.ProgressBar(0f, xmitValue / refValue, expSkin.customStyles[9], expSkin.customStyles[12], GUILayout.Width(100f));
				GUIUtil.ProgressBar(GUILayoutUtility.GetLastRect(), 0f, xmitValue / refValue - valueAfterXmit / refValue, expSkin.customStyles[13]);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Button(selectedData.dataAmount + " M", transferButton);
				GUILayout.FlexibleSpace();
				GUILayout.Button("", expSkin.customStyles.First(s => s.name.Equals("lab button")));
				GUILayout.FlexibleSpace();
				GUILayout.Button((xmitValue/refValue*100f) + "%", expSkin.customStyles.First(s => s.name.Equals("transmit button")));
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				GUILayout.Button("", expSkin.customStyles.First(s => s.name.Equals("discard button")));
				GUILayout.Button(">>>", expandWinButton);
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				/*GUILayout.Label(dataSizeText, statLabel, GUILayout.Height(18f));

				GUILayout.BeginHorizontal(horzStyle);
				GUILayout.Label(dataRecText, statLabel, GUILayout.Height(18f));
				GUIUtil.Layout.ProgressBar(0f, scienceValue / refValue, bgProgressBar, barPrograssBar, GUILayout.Width(150f), GUILayout.Height(18f));
				GUILayout.EndHorizontal();

				GUILayout.Label(dataXmitText, statLabel, GUILayout.Height(18f));*/
			}

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		#endregion

		#region Other Methods

		protected void OnVesselChange(Vessel vessel) {
			UpdateDriveList();
		}

		protected void OnVesselWasModified(Vessel vessel) {
			UpdateDriveList();
		}

		protected void UpdateDriveList() {
			drives = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ScienceHardDrive>().OrderBy(d => d.part.partInfo.title).ToArray();
			if(drives.Count() > 0) {
				isVisable = true;
			}
			else {
				isVisable = false;
			}
		}

		#endregion
	}
}
