using System.Collections.Generic;
using UnityEngine;

namespace ScienceHardDrives {

	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class DargonUtils : MonoBehaviour {

		#region Fields
		private static bool ranOnce = false;
		private static readonly bool debug = true;

		internal static GUISkin gameSkin;
		internal static GUISkin expSkin;
		internal static GUISkin managerSkin;

		internal static Texture2D dataIcon;
		internal static Texture2D scienceIcon;
		internal static Texture2D managerTransferNormal;
		#endregion

		#region Create/Destroy Methods

		public void Start() {
			Print("DargonUtils started.");
		}

		public void OnDestroy() {
			Print("DargonUtils destroyed.");
		}

		#endregion

		#region GUI

		public void OnGUI() {
			if(!ranOnce) {
				ranOnce = true;
				InitializeTextures();
				InitializeSkins();
			}
		}

		#endregion

		#region Initialization Methods

		private static void InitializeSkins() {
			Print("Initializing Skins");
			GUIStyle tempStyle;
			List<GUIStyle> customStyles;

			gameSkin = GUI.skin;
			expSkin = AssetBase.GetGUISkin("ExperimentsDialogSkin");
			managerSkin = (GUISkin)MonoBehaviour.Instantiate(expSkin);

			customStyles = new List<GUIStyle>(managerSkin.customStyles);
			Print("Initial customStyles copied");

			tempStyle = managerSkin.window;
			tempStyle.padding = new RectOffset(8, 8, 20, 8);
			tempStyle.fixedWidth = 375f;
			tempStyle.contentOffset = new Vector2(0, -18);
			tempStyle.overflow = new RectOffset(10, 0, 0, 0);
			Print("window modified");

			customStyles.Add(tempStyle = new GUIStyle(managerSkin.FindStyle("resultfield")));
			tempStyle.name = "selectBox";
			tempStyle.overflow = new RectOffset(0, 1, 1, 2);
			Print("selectBox added.");

			customStyles.Add(tempStyle = new GUIStyle(gameSkin.button));
			tempStyle.name = "expandButton";
			tempStyle.margin = new RectOffset(4, 0, 0, 0);
			tempStyle.fixedHeight = 18f;
			tempStyle.fixedWidth = 20f;
			tempStyle.alignment = TextAnchor.MiddleCenter;
			tempStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			Print("expandButton added.");

			customStyles.Add(tempStyle = new GUIStyle(gameSkin.button));
			tempStyle.name = "deselectButton";
			tempStyle.margin = new RectOffset(4, 4, 2, 2);
			tempStyle.fixedHeight = 22f;
			tempStyle.fixedWidth = 22f;
			tempStyle.alignment = TextAnchor.MiddleCenter;
			tempStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

			customStyles.Add(tempStyle = new GUIStyle(gameSkin.button));
			tempStyle.name = "selectButtonUp";
			tempStyle.margin = new RectOffset(0, 4, 0, 0);
			tempStyle.fixedHeight = 18f;
			tempStyle.alignment = TextAnchor.MiddleLeft;
			tempStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			Print("selectButtonUp added.");

			customStyles.Add(tempStyle = new GUIStyle(tempStyle));
			tempStyle.name = "selectButtonDown";
			tempStyle.normal = tempStyle.active;
			tempStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			Print("selectButtonDown added.");

			customStyles.Add(tempStyle = new GUIStyle(gameSkin.label));
			tempStyle.name = "expandLabel";
			tempStyle.margin = new RectOffset(4, 0, 0, 0);
			tempStyle.fixedHeight = 18f;
			tempStyle.fixedWidth = 20f;
			tempStyle.alignment = TextAnchor.MiddleCenter;
			tempStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			Print("expandLabel added.");

			customStyles.Add(tempStyle = new GUIStyle(expSkin.FindStyle("discard button")));
			tempStyle.name = "transferButton";
			tempStyle.normal.background = managerTransferNormal;
			tempStyle.normal.textColor = ConvertColor(164, 45, 208);
			//TODO: Hover & Active GUIStyleStates
			Print("transferButton added.");

			customStyles.Add(tempStyle = new GUIStyle(expSkin.button));
			tempStyle.name = "expandWinButton";
			tempStyle.margin = new RectOffset(8, 4, 0, 0);
			tempStyle.fixedHeight = 16f;
			tempStyle.fontSize = 11;
			tempStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			tempStyle.alignment = TextAnchor.MiddleCenter;
			Print("expandWinButton added.");

			managerSkin.customStyles = customStyles.ToArray();
			Print("customStyles copied to managerSkin");
		}

		private static void InitializeTextures() {
			Print("Initializing Textures");
			string textureLocation = "SirDargon/ScienceHardDrives/img/";
			Texture2D[] textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[];


			dataIcon = AssetBase.GetTexture("resultsdialog_datasize");
			scienceIcon = AssetBase.GetTexture("resultsdialog_scivalue");
			managerTransferNormal = GameDatabase.Instance.GetTexture(textureLocation + "manager_transfer_normal", false);
		}

		#endregion

		#region Utility Methods

		internal static Color ConvertColor(int r, int g, int b) {
			return new Color(r / 255f, g / 255f, b / 255f);
		}

		internal static void Print(string toPrint) {
			if(debug) {
				print("SirDargon [SHD]: " + toPrint);
			}
		}

		#endregion
	}
}
