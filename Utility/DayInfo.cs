using System;

namespace Utility
{
	[Serializable]
	public class DayInfo
	{
		public DateTime date;

		public int deathCasesTotal { get; }
		public int infectedCasesTotal { get; }
		public int recoveredCasesTotal { get; }

		public int deathCases { get; }
		public int infectedCases { get; }
		public int recoveredCases { get; }

		public DayInfo? prev;
		public DayInfo? next;


		public DayInfo(ref DayInfo prev, int deathCasesTotal, int infectedCasesTotal, int recoveredCasesTotal, DateTime date)
		{
			this.prev = prev ?? new DayInfo();
			this.prev.next = this;
			this.date = date;
			this.deathCasesTotal = deathCasesTotal;
			this.infectedCasesTotal = infectedCasesTotal;
			this.recoveredCasesTotal = recoveredCasesTotal;

			this.deathCases = Math.Max(deathCasesTotal - this.prev.deathCasesTotal, 0);
			this.infectedCases = Math.Max(infectedCasesTotal - this.prev.infectedCasesTotal, 0);
			this.recoveredCases = Math.Max(recoveredCasesTotal - this.prev.recoveredCasesTotal, 0);
		}

		public DayInfo()
		{
			this.deathCasesTotal = this.infectedCasesTotal = this.recoveredCasesTotal = 0;
			this.deathCases = this.infectedCases = this.recoveredCases = 0;
		}
	}
}