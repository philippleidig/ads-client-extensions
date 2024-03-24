using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.SumCommand;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace TwinCAT.Ads.Extensions.Reactive
{
	public static class AdsClientExtensions
    {
		public static IObservable<IDictionary<ISymbol, object>> PollValues(this IAdsConnection connection, IList<ISymbol> symbols, IObservable<Unit> trigger)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			if (symbols == null) throw new ArgumentNullException(nameof(symbols));

			if (!connection.IsConnected) throw new ClientNotConnectedException(connection);

			var sumCommand = new SumSymbolRead(connection, symbols);

			Func<Unit, IDictionary<ISymbol, object>> selector = (Unit o) => {
				var sumResult = sumCommand.Read();

				return sumResult.Zip(symbols, (value, symbol) => (value, symbol))
								.ToDictionary(x => x.symbol, x => x.value);
			};

			return trigger.Select(selector);
		}

		public static IObservable<IDictionary<ISymbol, object>> PollValues(this IAdsConnection connection, IList<ISymbol> symbols, TimeSpan period)
		{
			return connection.PollValues(symbols, from e in Observable.Interval(period).StartWith(new long[] { -1L }) select Unit.Default);
		}

		public static IObservable<ValuesChangedEventArgs> PollValuesAnnotated(this IAdsConnection connection, IList<ISymbol> symbols, TimeSpan period)
		{
			return from o in connection.PollValues(symbols, period) select new ValuesChangedEventArgs(o, DateTime.Now);
		}
	}
}
