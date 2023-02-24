using Ace;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class TapeViewModel : ContextObject
	{
#if !NETSTANDARD
		public System.Windows.Media.Media3D.ProjectionCamera Camera
		{
			get => Get(() => Camera);
			set => Set(() => Camera, value);
		}
#endif
		[DataMember] public double Radius
		{
			get => Get(() => Radius, 1d);
			set => Set(() => Radius, value);
		}
		[DataMember] public double Depth
		{
			get => Get(() => Depth, 32d);
			set => Set(() => Depth, value);
		}
		[DataMember] public double Thin
		{
			get => Get(() => Thin, 0.04d);
			set => Set(() => Thin, value);
		}
		[DataMember] public double Approximation
		{
			get => Get(() => Approximation, 1d);
			set => Set(() => Approximation, value);
		}
		[DataMember] public bool Joystick
		{
			get => Get(() => Joystick, true);
			set => Set(() => Joystick, value);
		}
	}
}
