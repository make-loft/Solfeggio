using Ace;
using Rainbow;
using System.Collections.Generic;

using static System.Math;
using static Rainbow.HarmonicFuncs;

namespace Solfeggio.Models
{
	public enum PhaseMode { Flow, Loop }

	[DataContract]
	public partial class Harmonic : ContextObject, IExposable
	{
		public Harmonic() => Expose();

		public void Expose()
		{
			this[() => PhaseMode].Changed += (o, e) => Context.Get("Loop").EvokeCanExecuteChanged();
			this[() => PhaseMode].Changed += (o, e) => Context.Get("Flow").EvokeCanExecuteChanged();
			this[() => IsEnabled].Changed += (o, e) => Context.Get("Mute").EvokeCanExecuteChanged();
			this[() => IsEnabled].Changed += (o, e) => Context.Get("Loud").EvokeCanExecuteChanged();
		}

		public delegate double Basis(double v);
		[DataMember] public Basis[] BasisFuncs { get; } = { Cos, Sin, Triangle, Sawtooth, Rectangle };

		Basis _basisFunc = Cos;
		[DataMember] public Basis BasisFunc
		{
			get => _basisFunc;
			set => value.To(out _basisFunc).Notify(this);
		}

		double _magnitude = 0.3d, _frequency = 440d, _phase = 0d, _gap = 0d;

		[DataMember] public double Magnitude
		{
			get => _magnitude;
			set => value.To(out _magnitude).Notify(this);
		}
		[DataMember] public double Frequency
		{
			get => _frequency;
			set => value.To(out _frequency).Notify(this);
		}
		[DataMember] public double Phase
		{
			get => _phase;
			set => value.To(out _phase).Notify(this);
		}
		[DataMember] public double Gap
		{
			get => _gap;
			set => value.To(out _gap).Notify(this);
		}

		PhaseMode _phaseMode = PhaseMode.Flow;
		[DataMember] public PhaseMode PhaseMode
		{
			get => _phaseMode;
			set => value.To(out _phaseMode).Notify(this);
		}

		bool _isEnabled = true;
		[DataMember] public bool IsEnabled
		{
			get => _isEnabled;
			set => value.To(out _isEnabled).Notify(this);
		}

		private double _offset;

		public IEnumerable<double> EnumerateBins(double sampleRate, bool globalLoop = false)
		{
			var step = _frequency * Pi.Double / sampleRate;
			_offset = _phaseMode.Is(PhaseMode.Loop) || globalLoop ? -step : _offset;
			while (true)
			{
				_offset += step;
				var value = _offset + _phase;
				if (_gap.Is(0d))
					yield return _magnitude * _basisFunc(value);
				else
				{
					var hit = (int)(Abs(value / Pi.Double) % Gap) == 0d;
					yield return hit ^ _gap > 0d ? 0d : _magnitude * BasisFunc(value);
				}
			}
		}
	}
}
