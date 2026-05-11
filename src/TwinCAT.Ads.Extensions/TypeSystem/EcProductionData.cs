namespace TwinCAT.Ads.Extensions.TypeSystem
{
	/// <summary>
	/// Contains production data read from an EtherCAT slave's EEPROM at address 0x0020.
	/// </summary>
	public class EcProductionData
	{
		/// <summary>
		/// Firmware version of the EtherCAT slave (EEPROM word 0).
		/// </summary>
		public ushort FirmwareVersion { get; set; }

		/// <summary>
		/// Hardware version of the EtherCAT slave (EEPROM word 1).
		/// </summary>
		public ushort HardwareVersion { get; set; }

		/// <summary>
		/// Production date decoded from EEPROM word 2.
		/// </summary>
		public EcProductionDate ProductionDate { get; set; }
	}

	/// <summary>
	/// Represents a production date encoded in an EtherCAT slave's EEPROM.
	/// The date is stored as a 16-bit value with year (bits 0-6), day of week (bits 7-9),
	/// and calendar week (bits 10-15).
	/// </summary>
	public class EcProductionDate
	{
		/// <summary>
		/// Production year (2000-2099). A value of 0 indicates unknown or invalid.
		/// </summary>
		public int Year { get; set; }

		/// <summary>
		/// Calendar week of production (1-52).
		/// </summary>
		public int CalendarWeek { get; set; }

		/// <summary>
		/// Day of the week of production (0-6).
		/// </summary>
		public int DayOfWeek { get; set; }

		/// <summary>
		/// Returns a string representation of the production date.
		/// </summary>
		public override string ToString()
		{
			if (Year == 0)
				return "Unknown";

			return $"{Year}-W{CalendarWeek:D2}-{DayOfWeek}";
		}
	}
}
