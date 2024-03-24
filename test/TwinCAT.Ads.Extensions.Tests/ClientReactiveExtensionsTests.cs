using TwinCAT.Ads.Extensions.Reactive;
using TwinCAT.TypeSystem;
using static TwinCAT.Ads.Extensions.Tests.Globals;

namespace TwinCAT.Ads.Extensions.Tests
{
	[TestClass]
	public class ClientReactiveExtensionsTests
	{
		[TestMethod]
		public async Task TestPollValues_SymbolsNull()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				IList<ISymbol> symbols = null;

				var exception = Assert.ThrowsException<ArgumentNullException>(() => {
					adsClient.PollValues(symbols, TimeSpan.FromSeconds(1));
				});
			}
		}

		[TestMethod]
		public async Task TestPollValues_ClientNotConnect()
		{
			using (AdsClient adsClient = new AdsClient())
			{
				IList<ISymbol> symbols = new List<ISymbol>();

				var exception = Assert.ThrowsException<ClientNotConnectedException>(() => {
					adsClient.PollValues(symbols, TimeSpan.FromSeconds(1));
				});
			}
		}
	}
}
