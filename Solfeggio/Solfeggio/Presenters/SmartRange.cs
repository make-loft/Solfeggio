using Ace;

namespace Solfeggio.Presenters
{
	[DataContract]
	public class SmartRange<T> : SmartObject where T : struct
	{
		T _from, _till;

		[DataMember]
		public T Till
		{
			get => _till;
			set => value.To(out _till).Notify(this).Notify(this, nameof(Length));
		}

		[DataMember]
		public T From
		{
			get => _from;
			set => value.To(out _from).Notify(this).Notify(this, nameof(Length));
		}

		public virtual T Length { get; }

		public void Deconstruct(out T from, out T till)
		{
			from = _from;
			till = _till;
		}
	}

	[DataContract]
	public class SmartRange : SmartRange<double>
	{
		public override double Length => Till - From;

		public static SmartRange Create(double from, double till) => new()
		{
			From = from,
			Till = till,
		};
	}
}
