using Ace;
using System.Collections.Generic;

namespace Solfeggio.Palettes.Languages
{
	class English : ALanguage<English>
	{
		public override Dictionary<string, string> GetDictionary() => new Dictionary<string, string>
		{
			{ "Advanced", "Advanced" },
			{ "Сalibration", "Сalibration" },
			{ "Visualization", "Visualization" },
			{ "Frame", "Frame" },
			{ "Duration", "Duration" },
		};
	}
}
