using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceHardDrives {

	class VesselSettings : VesselModule {

		#region Fields

		protected List<IScienceDataContainer> containers;
		protected List<IScienceDataContainer> viewing;

		protected IScienceDataContainer selectedContainer;
		protected ScienceData selectedData;

		#endregion

		#region VesselModule Methods

		public void Awake() {
			GameEvents.onVesselWasModified.Add(OnVesselWasModified);
			viewing = new List<IScienceDataContainer>();
			selectedContainer = null;
			selectedData = null;

			UpdateContainerLists();
		}

		#endregion

		#region Event Methods

		public void OnVesselWasModified(Vessel vessel) {
			if(this.GetComponent<Vessel>() == vessel) {
				UpdateContainerLists();
			}
		}

		#endregion

		#region Other Methods

		protected void UpdateContainerLists() {
			containers = this.GetComponent<Vessel>().FindPartModulesImplementing<IScienceDataContainer>().ToList();

			viewing = viewing.Intersect(containers).ToList();

			if(selectedContainer == null || !containers.Contains(selectedContainer)) {
				selectedData = null;
				selectedContainer = null;
			}
		}

		public void SelectContainer(IScienceDataContainer container) {
			if(container == null || containers.Contains(container)) {
				selectedContainer = container;
				selectedData = null;
			}
		}

		public void SelectData(IScienceDataContainer container, ScienceData data) {
			if(data == null || container.GetData().Contains(data)) {
				selectedData = data;
				selectedContainer = null;
			}
		}

		public void ToggleViewing(IScienceDataContainer container) {
			if(container != null && containers.Contains(container)) {
				if(viewing.Contains(container)) {
					viewing.Remove(container);
				}
				else {
					viewing.Add(container);
				}
			}
		}

		#endregion
	}
}
