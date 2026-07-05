namespace TwinCAT.Ads.Extensions.Tests
{
	/// <summary>
	/// Base class for tests that require a reachable TwinCAT system service.
	/// Each test is skipped (reported <c>Inconclusive</c>) when no target is
	/// configured/reachable, so the suite is a no-op unless a real TwinCAT system
	/// is available. Configure the target via the <c>ADS_TEST_TARGET</c>
	/// environment variable (see <see cref="Globals"/>).
	/// </summary>
	public abstract class IntegrationTest
	{
		[TestInitialize]
		public void RequireReachableTarget()
		{
			Globals.RequireTarget();
		}
	}
}
