using System;
using System.Collections.Generic;
using TwinCAT.TypeSystem;

namespace TwinCAT.Ads.TypeSystem
{
	public class ValuesChangedEventArgs : EventArgs
	{
		private readonly IDictionary<ISymbol, object> _symbols;
		private readonly DateTime _dateTime = DateTime.Now;

		public ValuesChangedEventArgs(IDictionary<ISymbol, object> symbols, DateTime timeStamp)
		{
			_symbols = symbols;
			_dateTime = timeStamp;
		}

		public IDictionary<ISymbol, object> Symbols => _symbols;
		public DateTime DateTime => _dateTime;
	}
}
