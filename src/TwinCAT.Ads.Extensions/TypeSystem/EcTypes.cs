namespace TwinCAT.Ads.Extensions.TypeSystem
{
	/// <summary>
	/// Specifies the addressing type for EtherCAT physical read/write operations.
	/// </summary>
	public enum EcAddressingType : byte
	{
		/// <summary>
		/// Fixed addressing using the configured station address.
		/// </summary>
		Fixed = 0,

		/// <summary>
		/// Auto-increment addressing using the slave position in the ring.
		/// </summary>
		AutoIncrement = 1,

		/// <summary>
		/// Broadcast addressing targeting all slaves.
		/// </summary>
		Broadcast = 2
	}
}
