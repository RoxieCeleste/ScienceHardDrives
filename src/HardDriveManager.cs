using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceHardDrives {

	public class QueuePacket {
		protected readonly IScienceDataContainer _container;
		protected readonly ScienceData _data;
		protected readonly ScienceHardDrive _drive;

		public IScienceDataContainer container {
			get {
				return _container;

			}
		}

		public ScienceData data {
			get {
				return _data;
			}
		}

		public ScienceHardDrive drive {
			get {
				return _drive;
			}
		}

		public QueuePacket(ScienceHardDrive drive, IScienceDataContainer container, ScienceData data) {
			_container = container;
			_data = data;
			_drive = drive;
		}


	}

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class HardDriveManager : MonoBehaviour {

		public static HardDriveManager instance;

		#region Fields

		protected Rect windowPos = new Rect();

		protected bool _isVisible;

		private Vector2 HDDScrollPos;
		private Vector2 SCScrollPos;

		private Vector2 HDDDataScrollPos;
		private Vector2 SCDataScrollPos;

		private ScienceHardDrive[] drives;
		private Part[] containers;

		private ScienceHardDrive _selectedDrive;
		private Part _selectedContainer;

		private bool xferNonrerunnable = false;
		private QueuePacket queuePacket = null;

		private Dictionary<ScienceHardDrive, Dictionary<ScienceData, IScienceDataContainer>> queues = new Dictionary<ScienceHardDrive, Dictionary<ScienceData, IScienceDataContainer>>();

		private GUISkin[] skin_array;
		private int skinInt;

		private Texture2D[] texs;
		private Vector2 texScroll;

		#endregion

		#region Properties

		protected bool isVisible {
			get {
				return _isVisible;
			}
			set {
				_isVisible = value;
			}
		}

		private ScienceHardDrive selectedDrive {
			get {
				return _selectedDrive;
			}
			set {
				_selectedDrive = value;
			}
		}

		private Part selectedContainer {
			get {
				return _selectedContainer;
			}
			set {
				_selectedContainer = value;
			}
		}

		#endregion

		#region Create/Destroy

		public void Awake() {
			instance = this;
			isVisible = false;
			RenderingManager.AddToPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Add(CheckVessel);
			GameEvents.onVesselWasModified.Add(CheckVessel);
			//skin_array = Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[];
			skinInt = 0;
			texs = (Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[]).Where(t => t.name.IndexOf("resultsdialog") >= 0).OrderBy(t => t.name).ToArray();
		}

		public void OnDestroy() {
			instance = null;
			RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			GameEvents.onVesselChange.Add(CheckVessel);
			GameEvents.onVesselWasModified.Remove(CheckVessel);
		}

		#endregion

		#region GUI

		private void OnDraw() {
			if(isVisible) {
				GUI.skin = null;//skin_array[skinInt];
				//windowPos = GUILayout.Window(170263, windowPos, OnWindow, "Hard Drive Manager");
				GUI.skin = null;
			}
		}

		private void OnWindow(int indowId) {
			GUILayout.BeginVertical();

			GUIStyle style = new GUIStyle((Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[]).First(s => s.name.Equals("GameSkin")).window);
			style.normal.background = Texture2D.whiteTexture;

			/*if(GUILayout.Button("Skin")) {
				if(++skinInt >= skin_array.Length) {
					skinInt = 0;
				}
				print(skin_array[skinInt].name);
			}*/

			texScroll = GUILayout.BeginScrollView(texScroll, GUILayout.Height(500f), GUILayout.Width(700f));
			GUILayout.BeginHorizontal(style);
			foreach(Texture2D t in texs) {
				GUILayout.BeginVertical();

				GUILayout.Label(t);
				GUILayout.Label(t.name);

				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndScrollView();


			GUILayout.BeginHorizontal(GUILayout.Width(700f), GUILayout.Height(250f));

			GUILayout.BeginVertical(GUILayout.Width(350f));
			GUILayout.Label("Hard Drives");

			HDDScrollPos = GUILayout.BeginScrollView(HDDScrollPos, GUILayout.Height(100f));
			foreach(ScienceHardDrive d in drives) {
				if(GUILayout.Button(d.part.partInfo.title + " \t" + d.freeSpace + "/" + d.capacity + " Mits Free")) {
					if(selectedDrive == d) {
						selectedDrive = null;
					}
					else {
						selectedDrive = d;
					}
				}
			}
			GUILayout.EndScrollView();

			GUILayout.Label(selectedDrive != null ? selectedDrive.part.partInfo.title : "No Hard Drive Selected");

			HDDDataScrollPos = GUILayout.BeginScrollView(HDDDataScrollPos);
			if(selectedDrive != null) {
				foreach(ScienceData d in selectedDrive.GetData()) {
					if(GUILayout.Button(d.title + ": \t" + d.dataAmount + " Mits")) {
						selectedDrive.ReviewDataItem(d);
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.Width(350f));
			GUILayout.Label("Science Containers");

			SCScrollPos = GUILayout.BeginScrollView(SCScrollPos, GUILayout.Height(100f));
			foreach(Part p in containers) {
				if(GUILayout.Button(p.partInfo.title)) {
					if(selectedContainer == p) {
						selectedContainer = null;
					}
					else {
						selectedContainer = p;
					}
				}
			}
			GUILayout.EndScrollView();

			GUILayout.Label(selectedContainer != null ? selectedContainer.partInfo.title : "No Part Selected");

			SCDataScrollPos = GUILayout.BeginScrollView(SCDataScrollPos);

			if(selectedContainer != null) {
				foreach(IScienceDataContainer c in selectedContainer.FindModulesImplementing<IScienceDataContainer>()) {
					foreach(ScienceData d in c.GetData()) {
						GUILayout.BeginHorizontal();

						if(GUILayout.Button(d.title + ": \t" + d.dataAmount + " Mits")) {
							c.ReviewDataItem(d);
						}

						if(GUILayout.Button("Xfer") && selectedDrive != null) {
							if(c.IsRerunnable() || xferNonrerunnable) {
								queueData(selectedDrive, c, d);
							}
							else {
								queuePacket = new QueuePacket(selectedDrive, c, d);

								DialogOption<bool>[] dialogOptions = new DialogOption<bool>[2];
								dialogOptions[0] = new DialogOption<bool>("Transfer Anyways", new Callback<bool>(promptCallBack), true);
								dialogOptions[1] = new DialogOption<bool>("Abort Transfer", new Callback<bool>(promptCallBack), false);
								PopupDialog.SpawnPopupDialog(new MultiOptionDialog("Transfering science from a nonrerunnable experiment will cause it to become inoperable.", "Warning", HighLogic.Skin, dialogOptions), false, HighLogic.Skin);
							}

						}

						GUILayout.EndHorizontal();
					}
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}

		#endregion

		#region Queue Methods

		private void queueData(ScienceHardDrive drive, IScienceDataContainer container, ScienceData data) {
			Dictionary<ScienceData, IScienceDataContainer> queue;

			if(!queues.Any(kvp => kvp.Value.ContainsKey(data))) {
				if(queues.TryGetValue(drive, out queue)) {
					float queueAmount = 0;

					foreach(ScienceData d in queue.Keys) {
						queueAmount += d.dataAmount;
					}

					if(data.dataAmount + queueAmount <= drive.freeSpace) {
						print("Queue for " + drive.part.partInfo.title + " found and queuing " + data.title);
						queue.Add(data, container);
					}
					else {
						print("Not enough room to queue: " + data.title);
					}
				}
				else {
					if(data.dataAmount <= drive.freeSpace) {
						print(drive.part.partInfo.title + " doesn't have a queue. Making one and queuing " + data.title);
						queues.Add(drive, (queue = new Dictionary<ScienceData, IScienceDataContainer> { { data, container } }));
					}
					else {
						print("Not enough room to queue: " + data.title);
					}
				}

				if(!drive.isXfering()) {
					print(drive.part.partInfo.title + " starting data transfer");
					StartCoroutine(drive.transferData());
				}
				else {
					print(drive.part.partInfo.title + " is already tranfering data");
				}
			}
			else {
				print(data.title + " is already queued");
			}
		}

		public KeyValuePair<ScienceData, IScienceDataContainer> getNextInQueue(ScienceHardDrive drive) {
			Dictionary<ScienceData, IScienceDataContainer> queue;
			KeyValuePair<ScienceData, IScienceDataContainer> kvp = new KeyValuePair<ScienceData, IScienceDataContainer>();

			if(queues.TryGetValue(drive, out queue)) {
				if(queue.Count > 0) {
					kvp = queue.First();
				}
			}

			return kvp;
		}

		public void removeFromQueue(ScienceHardDrive drive, ScienceData data) {
			Dictionary<ScienceData, IScienceDataContainer> queue;

			if(queues.TryGetValue(drive, out queue)) {
				queue.Remove(data);
				if(queue.Count == 0) {
					queues.Remove(drive);
				}
			}
		}

		public void clearQueue(ScienceHardDrive drive) {
			queues.Remove(drive);
		}

		#endregion

		#region Other Methods

		public void CheckVessel(Vessel vessel) {
			drives = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ScienceHardDrive>().OrderBy(d => d.part.partInfo.title).ToArray();
			containers = FlightGlobals.ActiveVessel.Parts.Where(p => p.FindModulesImplementing<IScienceDataContainer>().Count > 0).OrderBy(p => p.partInfo.title).ToArray();
			if(drives.Count() > 0) {
				isVisible = true;
			}
			else {
				isVisible = false;
			}
			selectedContainer = null;
			selectedDrive = null;
		}

		private void promptCallBack(bool xfer) {
			if(xfer) {
				queueData(queuePacket.drive, queuePacket.container, queuePacket.data);
			}
		}

		#endregion
	}
}
