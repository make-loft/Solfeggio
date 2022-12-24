using Ace;
using Solfeggio.Presenters;
using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.Models
{
	public partial class Harmonic
	{
		[DataContract]
		public class Profile : AProfile, IExposable
		{
			private static readonly MusicalPresenter presenter = Store.Get<MusicalPresenter>();

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
				this[Context.Set.Create].Executed += (o, e) => CreateFor(Harmonics, e.Parameter as Harmonic).Use(Harmonics.Add);
				this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<Harmonic>().Use(Harmonics.Remove);

				this[Context.Get("Loop")].Executed += (o, e) => Harmonics.ForEach(h => h.PhaseMode.Is(PhaseMode.Loop));
				this[Context.Get("Flow")].Executed += (o, e) => Harmonics.ForEach(h => h.PhaseMode.Is(PhaseMode.Flow));
				this[Context.Get("Mute")].Executed += (o, e) => Harmonics.ForEach(h => h.IsEnabled.Is(false));
				this[Context.Get("Loud")].Executed += (o, e) => Harmonics.ForEach(h => h.IsEnabled.Is(true));

				this[Context.Get("Loop")].CanExecute += (o, e) => e.CanExecute = Harmonics.Any(h => h.PhaseMode.IsNot(PhaseMode.Loop));
				this[Context.Get("Flow")].CanExecute += (o, e) => e.CanExecute = Harmonics.Any(h => h.PhaseMode.IsNot(PhaseMode.Flow));
				this[Context.Get("Mute")].CanExecute += (o, e) => e.CanExecute = Harmonics.Any(h => h.IsEnabled.Is(true));
				this[Context.Get("Loud")].CanExecute += (o, e) => e.CanExecute = Harmonics.Any(h => h.IsEnabled.Is(false));

				this[Context.Get("Delete")].Executed += (o, e) => Harmonics.ToArray().ForEach(Harmonics.Remove);
			}

			private static Harmonic CreateFor(SmartSet<Harmonic> harmonics, Harmonic harmonic) => new()
			{
				Frequency = harmonic.Is() || harmonics.LastOrDefault().Is(out harmonic)
					? harmonic.Frequency + (harmonics.Count + 1)
					: presenter.Music.ActivePitchStandard
			};

			public double[] GenerateSignalSample(int length, double rate, bool isStatic) =>
				GenerateSignalSample(Harmonics.ToArray(), length, rate, isStatic); /* Harmonics may be modified during enumeration */

			private static double[] GenerateSignalSample(IEnumerable<Harmonic> harmonics, int length, double rate, bool gobalLoop)
			{
				var signalSample = new double[length];
				var harmonicSamples = harmonics.
					Where(h => h.IsEnabled && h.Frequency > 0d).
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