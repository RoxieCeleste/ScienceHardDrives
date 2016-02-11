using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceHardDrives {

	public class XferQueuePacket {
		public readonly IScienceDataContainer container;
		public readonly ScienceData data;
		public readonly ScienceHardDrive drive;

		public XferQueuePacket(ScienceHardDrive drive, IScienceDataContainer container, ScienceData data) {
			this.drive = drive;
			this.container = container;
			this.data = data;
		}
	}

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class QueueManager : MonoBehaviour {

		protected Dictionary<ScienceHardDrive, Dictionary<ScienceData, IScienceDataContainer>> xferQueues;
		public static QueueManager instance;

		#region Create/Destroy Methods

		public void Awake() {
			Print("QueueManager started.");

			instance = this;
			xferQueues = new Dictionary<ScienceHardDrive, Dictionary<ScienceData, IScienceDataContainer>>();
		}

		public void OnDestroy() {
			Print("QueueManager destroyed.");
		}

		#endregion

		#region Xfer Methods

		public IEnumerator TransferData(ScienceHardDrive drive) {
			if(drive.StartXfering()) {

				float secTotal = 0f;
				float secRemaining = 0f;
				ScreenMessage msg = null;
				ScienceData data = null;
				IScienceDataContainer container = null;
				KeyValuePair<ScienceData, IScienceDataContainer> kvp = new KeyValuePair<ScienceData, IScienceDataContainer>();

				while((kvp = QueueManager.instance.GetNextInXferQueue(drive)).Key != null) {
					data = kvp.Key;
					container = kvp.Value;

					if(data.dataAmount <= drive.capacity - drive.usedSpace) {
						secTotal = secRemaining = data.dataAmount / drive.xferRate;
						msg = new ScreenMessage("<color=#99ff00ff>Transfering " + data.title + " to " + drive.part.partInfo.title + ":  0%</color>", (float)secTotal + 5f, ScreenMessageStyle.UPPER_LEFT);

						ScreenMessages.PostScreenMessage(msg);
						while(secRemaining > 0 && Mathf.Round((float)drive.part.RequestResource("ElectricCharge", .1 * drive.powerUsage) * 10000) == .1 * drive.powerUsage * 10000) {
							yield return new WaitForSeconds(.1f);
							secRemaining -= .1f;
							msg.message = "<color=#99ff00ff>Transfering " + data.title + " to " + drive.part.partInfo.title + ": " + Mathf.Round(1000 * (secTotal - secRemaining) / secTotal) / 10 + "%</color>";
						}

						if(secRemaining > 0) {
							ScreenMessages.RemoveMessage(msg);
							ScreenMessages.PostScreenMessage("<color=#ff9900ff>Ran out of Electric Charge while transfering " + data.title + " to " + drive.part.partInfo.title + ".</color>", 5f, ScreenMessageStyle.UPPER_LEFT);
							QueueManager.instance.ClearXferQueue(drive);
						}
						else {
							ScreenMessages.RemoveMessage(msg);
							ScreenMessages.PostScreenMessage("<color=#99ff00ff>" + data.title + " transfered to " + drive.part.partInfo.title + ".</color>", 5f, ScreenMessageStyle.UPPER_LEFT);
							drive.AddData(data);
							container.DumpData(data);
						}
					}
					else {
						Print(drive.part.partInfo.title + " doesn't have the room for " + data.title);
					}

					QueueManager.instance.RemvoeFromXferQueue(drive, data);
				}

				yield return new WaitForSeconds(1f);
				drive.StopXfering();
			}
			else {
				Print(drive.part.partInfo.title + " is already transfering data.");
			}
		}

		public void QueueDataForXfer(ScienceHardDrive drive, IScienceDataContainer container, ScienceData data) {
			Dictionary<ScienceData, IScienceDataContainer> queue;

			if(!xferQueues.Any(kvp => kvp.Value.ContainsKey(data))) {
				if(xferQueues.TryGetValue(drive, out queue)) {
					float queueAmount = 0;

					foreach(ScienceData d in queue.Keys) {
						queueAmount += d.dataAmount;
					}

					if(data.dataAmount + queueAmount <= drive.freeSpace) {
						Print("Queue for " + drive.part.partInfo.title + " found and queuing " + data.title);
						queue.Add(data, container);
					}
					else {
						Print("Not enough room to queue " + data.title + " in the queue for " + drive.part.partInfo.title);
					}
				}
				else {
					if(data.dataAmount <= drive.freeSpace) {
						Print(drive.part.partInfo.title + " doesn't have a queue. Making one and queuing " + data.title);
						xferQueues.Add(drive, new Dictionary<ScienceData, IScienceDataContainer> { { data, container } });
					}
					else {
						Print("Not enough room to queue " + data.title + " in the queue for " + drive.part.partInfo.title);
					}
				}

				if(!drive.IsXfering()) {
					Print(drive.part.partInfo.title + " starting data transfer.");
					StartCoroutine(TransferData(drive));
				}
				else {
					Print(drive.part.partInfo.title + " is already tranfering data.");
				}
			}
			else {
				Print(data.title + " is already queued.");
			}
		}

		public KeyValuePair<ScienceData, IScienceDataContainer> GetNextInXferQueue(ScienceHardDrive drive) {
			Dictionary<ScienceData, IScienceDataContainer> queue;
			KeyValuePair<ScienceData, IScienceDataContainer> kvp = new KeyValuePair<ScienceData, IScienceDataContainer>();

			if(xferQueues.TryGetValue(drive, out queue)) {
				//if(queue.Count > 0) {
				kvp = queue.First();
				Print("Queue for " + drive.part.partInfo.title + " found. Next in queue is " + kvp.Key.title + ".");
				//}
			}
			else {
				Print("Queue for " + drive.part.partInfo.title + " is empty.");
			}

			return kvp;
		}

		public void RemvoeFromXferQueue(ScienceHardDrive drive, ScienceData data) {
			Dictionary<ScienceData, IScienceDataContainer> queue;

			if(xferQueues.TryGetValue(drive, out queue)) {
				if(queue.Remove(data)) {
					Print(data.title + " removed from the queue for " + drive.part.partInfo.title + ".");
				}
				else {
					Print(data.title + " doesn't exist in the queue for " + drive.part.partInfo.title + ".");
				}

				if(queue.Count == 0) {
					xferQueues.Remove(drive);
					Print("Queue for " + drive.part.partInfo.title + " is now empty. Removing it from the list of transfer queues.");
				}
			}
		}

		public void ClearXferQueue(ScienceHardDrive drive) {
			Print("Clearing queue for " + drive.part.partInfo.title + ".");
			xferQueues.Remove(drive);
		}

		#endregion

		#region Utility Methods
		private static void Print(string toPrint) {
			DargonUtils.Print("QM", toPrint);
		}
		#endregion
	}
}
