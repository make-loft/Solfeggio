using Ace;

namespace Solfeggio.Presenters
{
	[DataContract]
	public class SmartRange<T> : SmartObject where T : struct
	{
		T _lower, _upper;

		[DataMember]
		public T Upper
		{
			get => _upper;
			set => value.To(out _upper).Notify(this).Notify(this, nameof(Length));
		}

		[DataMember]
		public T Lower
		{
			get => _lower;
			set => value.To(out _lower).Notify(this).Notify(this, nameof(Length));
		}

		public virtual T Length { get; }

		public void Deconstruct(out T lower, out T upper)
		{
			lower = _lower;
			upper = _upper;
		}
	}

	[DataContract]
	public class SmartRange : SmartRange<double>
	{
		public override double Length => Upper - Lower;

		public static SmartRange Create(double lower, double upper) => new()
		{
			Lower = lower,
			Upper = upper
		};
	}
}
