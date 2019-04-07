using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Resources;
using System.Globalization;

namespace Solfeggio.Languages
{
	using o = Dictionary<LanguageCodes, string>;
	using static LanguageCodes;

	public enum LanguageCodes { English, Russian, Belorussian };

	public static class Localizator
	{
		private static o _(
			[CallerMemberName]
			string eng = default,
			string rus = default,
			string bel = default) => new o
		{
			{ English, eng },
			{ Russian, rus ?? eng },
			{ Belorussian, bel ?? rus ?? eng },
		};

		public static o Сalibration			= _("Сalibration",		"Калибровка",		"Каліброўка");
		public static o Visualization		= _("Visualization",	"Визуализация",		"Візуалізацыя");
		public static o Advanced			= _("Advanced",			"Расширенные");
		public static o Telemetry			= _("Telemetry",		"Телеметрия");
		public static o Sensitive			= _("Sensitive",		"Чувствительность");
		public static o Level				= _("Level",			"Уровень");
		public static o Short				= _("Short",			"Кратко");
		public static o Long				= _("Long",				"Долго");
		public static o Speed				= _("Speed",			"Скорость");
		public static o Accurate			= _("Accurate",			"Точность");
		public static o Manually			= _("Manually",			"Вручную");
		public static o Automatically		= _("Automatically",	"Автоматичеки");
		public static o Whisper				= _("Whisper",			"Шёпот");
		public static o Singing				= _("Singing",			"Пение");
		public static o Frame				= _("Frame",			"Кадр");
		public static o Framing				= _("Framing",			"Кадрирование");
		public static o Duration			= _("Duration",			"Длительность");
		public static o Wave				= _("Wave",				"Волна");
		public static o Language			= _("Language",			"Язык");
		public static o Device				= _("Device",			"Устройство");
		public static o Window				= _("Window",			"Окно");
		public static o Dominants			= _("Dominants",		"Доминанты");
		public static o Notes				= _("Notes",			"Ноты");
		public static o Sample				= _("Sample",			"Сэмпл");
        public static o Scaling				= _("Scaling",			"Масштабирование");
        public static o Frequency			= _("Frequency",		"Частота");
        public static o Magnitude			= _("Magnitude",		"Амплитуда");
        public static o Phase				= _("Phase",			"Фаза");
        public static o Bandwidth			= _("Bandwidth",		"Полоса пропускания");
        public static o Notation			= _("Notation",		    "Нотация");
        public static o Threshold			= _("Threshold",		"Порог");
        public static o Value				= _("Value",			"Значение");
        public static o Limit				= _("Limit",			"Предел");
		public static o Lower				= _("Lower",			"Нижний");
        public static o Upper				= _("Upper",			"Верхний");
        public static o Length				= _("Length",			"Длина");

        public static o MusicalStandards	= _("MusicalStandards", "Музыкальные стандарты");
		public static o PitchStandard		= _("Pitch Standard",	"Частотный стандарт");
		public static o LowFrequency		= _("Low Frequency",	"Нижняя частота");
		public static o TopFrequency		= _("Top Frequency",	"Верхняя частота");
		public static o SampleRate			= _("Sample Rate",		"Частота дискретизации");
		public static o NotesGrid			= _("Notes Grid",		"Нотная сетка");
		public static o DiscreteGrid		= _("Discrete Grid",	"Дискретная сетка");

		public static o PhaseSpectrum			= _("Phase Spectrum",	    "Фазовый спектр");
		public static o MagnitudeSpectrum		= _("Magnitude Spectrum",	"Амплитудный спектр");
        public static o NumericFormatting        = _("Numeric Formatting",  "Числовое форматирование");


        public static Dictionary<string, o> GetBaseDictionary() =>
			typeof(Localizator).GetFields(BindingFlags.Static | BindingFlags.Public).
			ToDictionary(m => m.Name, m => (o)typeof(Localizator).GetField(m.Name).GetValue(null));

		public static Dictionary<string, string> GetDictionary(LanguageCodes code) =>
			GetBaseDictionary().ToDictionary(p => p.Key, p => p.Value[code]);
	}

	public class LanguageManager : ResourceManager
	{
		public LanguageManager(LanguageCodes code) => _keyToValue = Localizator.GetDictionary(code);

		private readonly Dictionary<string, string> _keyToValue;
		public override string GetString(string key) =>	_keyToValue.TryGetValue(key, out var value) ? value : default;
		public override string GetString(string key, CultureInfo culture) => _keyToValue.TryGetValue(key, out var value) ? value : default;
	}
}
