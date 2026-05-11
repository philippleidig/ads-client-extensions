namespace TwinCAT.Ads.Extensions.TypeSystem
{
	/// <summary>
	/// Contains information about a discovered EtherCAT master device.
	/// </summary>
	public class EtherCatMasterInfo
	{
		/// <summary>
		/// The routable AmsNetId of the EtherCAT master.
		/// Combined from the target's AmsNetId (first 4 bytes) and the master's local route suffix (last 2 bytes).
		/// </summary>
		public AmsNetId AmsNetId { get; set; }

		/// <summary>
		/// The device type of the EtherCAT master.
		/// </summary>
		public ushort DeviceType { get; set; }

		/// <summary>
		/// The name of the EtherCAT master device.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Returns a string representation of the EtherCAT master.
		/// </summary>
		public override string ToString()
		{
			return Name + " " + AmsNetId.ToString();
		}
	}
}
