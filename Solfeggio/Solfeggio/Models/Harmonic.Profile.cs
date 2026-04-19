using Ace;

using System.Collections.Generic;
using System.Linq;

using Solfeggio.Presenters;

namespace Solfeggio.Models;

public partial class Harmonic
{
	[DataContract]
	public class Profile : AProfile, IExposable
	{
		private static readonly MusicalPresenter presenter = Store.Get<MusicalPresenter>();

		[DataMember]
		public SmartSet<Harmonic> Harmonics { get; set; } = [];

		public Profile() => Expose();

		public void Expose()
		{
			this[Context.Set.Create].Executed += args => CreateFor(Harmonics, args.Parameter as Harmonic).Use(Harmonics.Add);
			this[Context.Set.Delete].Executed += args => args.Parameter.To<Harmonic>().Use(Harmonics.Remove);

			this[Context.Get("Loop")].Executed += args => Harmonics.ForEach(h => h.PhaseMode.Is(PhaseMode.Loop));
			this[Context.Get("Flow")].Executed += args => Harmonics.ForEach(h => h.PhaseMode.Is(PhaseMode.Flow));
			this[Context.Get("Mute")].Executed += args => Harmonics.ForEach(h => h.IsEnabled.Is(false));
			this[Context.Get("Loud")].Executed += args => Harmonics.ForEach(h => h.IsEnabled.Is(true));

			this[Context.Get("Loop")].CanExecute += args => args.CanExecute = Harmonics.Any(h => h.PhaseMode.IsNot(PhaseMode.Loop));
			this[Context.Get("Flow")].CanExecute += args => args.CanExecute = Harmonics.Any(h => h.PhaseMode.IsNot(PhaseMode.Flow));
			this[Context.Get("Mute")].CanExecute += args => args.CanExecute = Harmonics.Any(h => h.IsEnabled.Is(true));
			this[Context.Get("Loud")].CanExecute += args => args.CanExecute = Harmonics.Any(h => h.IsEnabled.Is(false));

			this[Context.Get("Delete")].Executed += args => Harmonics.ToList().ForEach(Harmonics.Remove);
		}

		private static Harmonic CreateFor(SmartSet<Harmonic> harmonics, Harmonic harmonic) => new()
		{
			Frequency = harmonic.Is() || harmonics.LastOrDefault().Is(out harmonic)
				? harmonic.Frequency + (harmonics.Count + 1)
				: presenter.Music.ActivePitchStandard
		};

		public float[] GenerateSignalSample(int length, double rate, bool isStatic) =>
			GenerateSignalSample(Harmonics.ToList(), length, rate, isStatic); /* Harmonics may be modified during enumeration */

		private static float[] GenerateSignalSample(IEnumerable<Harmonic> harmonics, int length, double rate, bool gobalLoop)
		{
			var signalSample = new float[length];
			var harmonicSamples = harmonics
				.Where(h => h.IsEnabled && h.Frequency > 0d)
				.Select(h => h.EnumerateBins(rate, gobalLoop)
				.Select(d => (float)d).Take(length).ToArray())
				.ToArray()
				;

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
