namespace Solfeggio.Localization;

public readonly struct Translations(
	string english = default,
	string russian = default,
	string belorussian = default)
{
	public string English { get; } = english;
	public string Russian { get; } = russian;
	public string Belorussian { get; } = belorussian;

	public string this[Languages language] => language switch
	{
		Languages.English => English,
		Languages.Russian => Russian,
		Languages.Belorussian => Belorussian,
		Languages.Default => English,
		_ => English,
	};
}