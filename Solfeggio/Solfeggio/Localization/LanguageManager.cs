using Solfeggio.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Solfeggio;

public class LanguageManager(Languages language) : ResourceManager
{
	private readonly Dictionary<string, string> _keyToValue = GetDictionary(language);

	public override string GetString(string key) =>
		_keyToValue.TryGetValue(key, out var value) ? value : default;

	public override string GetString(string key, CultureInfo culture) =>
		_keyToValue.TryGetValue(key, out var value) ? value : default;

	public static Dictionary<string, string> GetDictionary(Languages language) =>
		typeof(Localizator)
		.GetFields(BindingFlags.Static | BindingFlags.Public)
		.ToDictionary(i => i.Name, i => ((Translations)i.GetValue(default))[language])
		;
}
