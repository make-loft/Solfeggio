using Ace;
using Rainbow;
using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.Models
{
	public partial class Harmonic
	{
		[DataContract]
		public class Profile : ContextObject, IExposable
		{
			[DataMember] public string Title { get; set; } = "Profile Name";

			[DataMember]
			public SmartSet<Harmonic> Harmonics { get; set; } = new SmartSet<Harmonic>
			{
				new Harmonic {Frequency = 220d},
				new Harmonic {Frequency = 440d}
			};

			public Profile() => Expose();

			public void Expose()
			{
				this[Context.Set.Add].Executed += (o, e) => new Harmonic().Use(Harmonics.Add);
				this[Context.Set.Remove].Executed += (o, e) => e.Parameter.To<Harmonic>().Use(Harmonics.Remove);
			}

			public Complex[] GenerateSignalSample(int length, double rate, bool isStatic) =>
				GenerateSignalSample(Harmonics, length, rate, isStatic);

			private static Complex[] GenerateSignalSample(IEnumerable<Harmonic> harmonics, int length, double rate, bool isStatic)
			{
				var signalSample = new Complex[length];
				var harmonicSamples = harmonics.
					Where(h => h.IsEnabled).
					Select(h => h.EnumerateBins(rate, isStatic).Take(length).ToArray()).
					ToArray();

				/* harmonicSamples may be modified during enumeration */
				for (var j = 0; j < harmonicSamples.Length; j++)
				{
					var harmonicSample = harmonicSamples[j];
					for (var i = 0; i < length; i++)
					{
						signalSample[i] += harmonicSample[i];
					}
				}

				return signalSample;
			}
		}
	}
}
 