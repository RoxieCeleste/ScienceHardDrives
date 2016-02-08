using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceHardDrives{
	class QueueManager {
		public class QueuePacket {
			public readonly IScienceDataContainer container;
			public readonly ScienceData data;
			public readonly ScienceHardDrive drive;

			public QueuePacket(ScienceHardDrive drive, IScienceDataContainer container, ScienceData data) {
				this.drive = drive;
				this.container = container;
				this.data = data;
			}
		}

	}
}
