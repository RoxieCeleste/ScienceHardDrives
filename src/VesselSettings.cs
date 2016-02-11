using System.Collections.Generic;
using System.Linq;

namespace ScienceHardDrives {

	class VesselSettings : VesselModule {

		#region Fields
		protected Vessel vessel;

		protected List<IScienceDataContainer> _expanded;
		protected List<IScienceDataContainer> _containers;

		protected IScienceDataContainer _selectedContainer;
		protected ScienceData _selectedData;

		#endregion

		#region Properties

		public List<IScienceDataContainer> expanded {
			get {
				return new List<IScienceDataContainer>(_expanded);
			}
		}

		public List<IScienceDataContainer> containers {
			get {
				return new List<IScienceDataContainer>(_containers);
			}
		}

		public IScienceDataContainer selectedContainer {
			set {
				SelectContainer(value);
			}
			get {
				return _selectedContainer;
			}
		}

		public ScienceData selectedData {
			set {
				SelectData(value);
			}
			get {
				return _selectedData;
			}
		}

		#endregion

		#region Create/Destroy Methods

		public void Awake() {
			vessel = this.GetComponent<Vessel>();

			Print("VesselSettings for " + vessel.vesselName + " started.");

			_expanded = new List<IScienceDataContainer>();
			_selectedContainer = null;
			_selectedData = null;

			UpdateContainerLists();
			GameEvents.onVesselWasModified.Add(OnVesselWasModified);
		}

		public void OnDestroy() {
			GameEvents.onVesselWasModified.Remove(OnVesselWasModified);

			Print("DargonUtils for " + vessel.vesselName + " destroyed.");
		}

		#endregion

		#region Event Methods

		public void OnVesselWasModified(Vessel vessel) {
			if(this.vessel == vessel) {
				Print(this.vessel.vesselName + " was modified.");
				UpdateContainerLists();
			}
		}

		#endregion

		#region Other Methods

		protected void UpdateContainerLists() {
			Print("Updateing vessel's container lists.");

			_containers = vessel.FindPartModulesImplementing<IScienceDataContainer>().OrderBy(c => ((PartModule)c).part.partInfo.title).ToList();
			_expanded = _expanded.Intersect(_containers).ToList();

			if(_selectedContainer == null || !_containers.Contains(_selectedContainer)) {
				selectedContainer = null;
			}

			if(_selectedData == null || _containers.Any(c => ((PartModule)c).part.flightID == _selectedData.container)) {
				selectedData = null;
			}
		}

		public bool SelectContainer(IScienceDataContainer container) {
			bool b = false;
			if(container != null) {
				if(_containers.Contains(container)) {
					if(_selectedContainer != container) {
						_selectedContainer = container;
						_selectedData = null;
						b = true;
						Print(((PartModule)container).part.partInfo.title + " was selected.");
					}
					/*else {
						_selectedContainer = null;
						Print(((PartModule)container).part.partInfo.title + " was deselected.");
					}*/
				}
				else {
					Print("The container doesn't exist in the avaliable containers.");
				}
			}
			else {
				_selectedContainer = null;
				Print("No container (null) was selected.");
			}
			return b;
		}

		public bool SelectData(ScienceData data) {
			bool b = false;
			if(data != null) {
				if(_containers.Any(c => c.GetData().Contains(data))) {
					if(_selectedData != data) {
						_selectedContainer = null;
						_selectedData = data;
						ExpandContainer(containers.First(c => ((PartModule)c).part.flightID == data.container));
						b = true;
						Print(data.title + " was selected.");
					}
					/*else {
						_selectedData = null;
						Print(data.title + " was deselected.");
					}*/
				}
				else {
					Print("The data doesn't exist in any of the avaliable containers.");
				}
			}
			else {
				_selectedData = null;
				Print("No data (null) was selected.");
			}
			return b;
		}

		public void ExpandOrCollapseContainer(IScienceDataContainer container) {
			if(container != null && _containers.Contains(container)) {
				if(_expanded.Contains(container)) {
					Print(((PartModule)container).part.partInfo.title + "'s ScienceData collapsed.");
					_expanded.Remove(container);
				}
				else {
					Print(((PartModule)container).part.partInfo.title + "'s ScienceData expanded.");
					_expanded.Add(container);
				}
			}
		}

		public void ExpandContainer(IScienceDataContainer container) {
			if(container != null && _containers.Contains(container)) {
				if(!_expanded.Contains(container)) {
					Print(((PartModule)container).part.partInfo.title + "'s ScienceData expanded.");
					_expanded.Add(container);
				}
			}
		}

		public void CollapseContainer(IScienceDataContainer container) {
			if(container != null && _containers.Contains(container)) {
				if(_expanded.Contains(container)) {
					Print(((PartModule)container).part.partInfo.title + "'s ScienceData collapsed.");
					_expanded.Remove(container);
				}
			}
		}

		#endregion

		#region Utiliy Methods

		private static void Print(string toPrint) {
			DargonUtils.Print("VS", toPrint);
		}

		#endregion
	}
}