using Ace;
using Solfeggio.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.Models
{
	public partial class Harmonic
	{
		[DataContract]
		public class Profile : ContextObject, IExposable
		{
			private static readonly MusicalPresenter presenter = Store.Get<MusicalPresenter>();

			[DataMember]
			public string Title
			{
				get => Get(() => Title, DateTime.Now.ToString());
				set => Set(() => Title, value);
			}

			[DataMember]
			public SmartSet<Harmonic> Harmonics { get; set; } = new()
			{
				new() {Frequency = presenter.Music.ActivePitchStandard + 0},
				new() {Frequency = presenter.Music.ActivePitchStandard + 2},
				new() {Frequency = presenter.Music.ActivePitchStandard + 5},
			};

			public Profile() => Expose();

			public void Expose()
			{
				this[Context.Set.Create].Executed += (o, e) => CreateFor(Harmonics).Use(Harmonics.Add);
				this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<Harmonic>().Use(Harmonics.Remove);
			}

			private static Harmonic CreateFor(SmartSet<Harmonic> harmonics) => new()
			{
				Frequency = harmonics.LastOrDefault().Is(out var harmonic)
					? harmonic.Frequency + (harmonics.Count + 1)
					: presenter.Music.ActivePitchStandard
			};

			public double[] GenerateSignalSample(int length, double rate, bool isStatic) =>
				GenerateSignalSample(Harmonics.ToArray(), length, rate, isStatic); /* Harmonics may be modified during enumeration */

			private static double[] GenerateSignalSample(IEnumerable<Harmonic> harmonics, int length, double rate, bool gobalLoop)
			{
				var signalSample = new double[length];
				var harmonicSamples = harmonics.
					Where(h => h.IsEnabled).
					Select(h => h.EnumerateBins(rate, gobalLoop).Take(length).ToArray()).
					ToArray();

				foreach (var harmonicSample in harmonicSamples)
				{
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