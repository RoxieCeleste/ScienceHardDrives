using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceHardDrives {

	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class Utils : MonoBehaviour {

		public void Awake() {
			print("SirDargon: Utils Started!");

			//GetStyles("ExperimentsDialogSkin");
			//FindTexture("resultsdialog_delete_normal");
			//PrintTextures();
			//GetTextureSize("resultsdialog_lab_normal");
		}

		private static void PrintTextures() {
			Texture2D[] textures = (Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[]).OrderBy(t => t.name).ToArray();

			using(System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Thomas Fuller\Programming\Textures.txt")) {
				foreach(Texture2D t in textures) {
					if(t.name != null) {
						file.WriteLine(t.name);
					}
				}
			}
		}

		private static void FindTexture(string name) {
			if(Resources.FindObjectsOfTypeAll(typeof(Texture2D)) != null) {
				print("SirDargon: Texture found!");
			}
		}

		private static void GetStyles(String name) {
			try {
				GUISkin skin = (Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[]).First(s => s.name.Equals(name));
				GUIStyle[] styles = (new GUIStyle[] {
					skin.box, skin.button, skin.horizontalScrollbar, skin.horizontalScrollbarLeftButton, skin.horizontalScrollbarRightButton,
					skin.horizontalScrollbarThumb, skin.horizontalSlider, skin.horizontalSliderThumb, skin.label, skin.scrollView, skin.textArea, skin.textField, skin.verticalScrollbar,
					skin.verticalScrollbarDownButton, skin.verticalScrollbarThumb, skin.verticalScrollbarUpButton, skin.verticalSlider, skin.verticalSliderThumb, skin.window
					}).Concat(skin.customStyles).ToArray();

				using(System.IO.StreamWriter file =
				new System.IO.StreamWriter(@"C:\Users\Thomas Fuller\Programming\GUIStyles.txt")) {
					file.WriteLine(skin.name + ":");
					if(skin.font != null) {
						file.WriteLine(skin.font.name + ":");
					}
					file.WriteLine();

					for(int i = 0; i < styles.Length; i++) {
						GUIStyle s = styles[i];


						string[] names = new string[] { "Normal", "Hover", "Active", "Focused", "On Normal", "On Hover", "On Ative", "On Focused" };
						GUIStyleState[] states = new GUIStyleState[] { s.normal, s.hover, s.active, s.focused, s.onNormal, s.onHover, s.onActive, s.onFocused };

						file.WriteLine(s.name + ":");
						for(int j = 0; j < names.Length; j++) {
							file.WriteLine("\t" + names[j] + ":");
							if(states[j].background != null) {
								file.WriteLine("\t\tBackground: " + states[j].background.name + "\n\t\tText Color: " + states[j].textColor.ToString());
							}
						}

						names = new string[] { "Border", "Margin", "Padding", "Overflow" };
						RectOffset[] rects = new RectOffset[] { s.border, s.margin, s.padding, s.overflow };

						for(int j = 0; j < names.Length; j++) {
							if(rects[j] != null) {
								file.WriteLine("\t" + names[j] + ":\n\t\tLeft: " + rects[j].left + "\n\t\tRight: " + rects[j].right + "\n\t\tTop: " + rects[j].top + "\n\t\tBottom: " + rects[j].bottom);
							}
						}

						if(s.font != null) {
							file.WriteLine("\tFont: " + s.font.name);
						}
						file.WriteLine("\tFont Size: " + s.fontSize);
						file.WriteLine("\tFont Style: " + s.fontStyle.ToString());
						file.WriteLine("\tAlignment: " + s.alignment.ToString());
						file.WriteLine("\tWord Wrap: " + s.wordWrap);
						file.WriteLine("\tRich Text: " + s.richText);
						file.WriteLine("\tText Clipping: " + s.clipping.ToString());
						file.WriteLine("\tImage Position: " + s.imagePosition.ToString());
						file.WriteLine("\tContent Offset: " + s.contentOffset.x + ", " + s.contentOffset.y);
						file.WriteLine("\tFixed Width: " + s.fixedWidth);
						file.WriteLine("\tFixed Height: " + s.fixedHeight);
						file.WriteLine("\tStretch Width: " + s.stretchWidth);
						file.WriteLine("\tStretch Height: " + s.stretchHeight);
						file.WriteLine();
					}
				}
			}
			catch(Exception e) {
				print(e.StackTrace);
			}
		}

		private static void GetTextureSize(string name) {
			Texture2D texture = (Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[]).First(t => t.name.Equals(name));
			if(texture != null) {
				print("SirDargon: " + texture.width + " x " + texture.height);
			}
		}
	}
}
