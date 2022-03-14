using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Solfeggio
{
	using static LanguageCodes;
	using o = Dictionary<LanguageCodes, string>;

	public enum LanguageCodes { English, Russian, Belorussian };

	public static class Localizator
	{
		private static o _(
			[CallerMemberName]
			string eng = default,
			string rus = default,
			string bel = default) => new()
		{
			{ English, eng },
			{ Russian, rus ?? eng },
			{ Belorussian, bel ?? rus ?? eng },
		};

		public static o Solfeggio = _("Solfeggio", "Сольфеджио");
		public static o Сalibration = _("Сalibration", "Калибровка", "Каліброўка");
		public static o Visualization = _("Visualization", "Визуализация", "Візуалізацыя");
		public static o Advanced = _("Advanced", "Расширенные");
		public static o Telemetry = _("Telemetry", "Телеметрия");
		public static o Sensitive = _("Sensitive", "Чувствительность");
		public static o Level = _("Level", "Уровень");
		public static o Short = _("Short", "Кратко");
		public static o Long = _("Long", "Долго");
		public static o Speed = _("Speed", "Скорость");
		public static o Accurate = _("Accurate", "Точность");
		public static o Manually = _("Manually", "Вручную");
		public static o Automatically = _("Automatically", "Автоматичеки");
		public static o Whisper = _("Whisper", "Шёпот");
		public static o Singing = _("Singing", "Пение");
		public static o Frame = _("Frame", "Кадр");
		public static o Framing = _("Framing", "Кадрирование");
		public static o Duration = _("Duration", "Длительность");
		public static o Wave = _("Wave", "Волна");
		public static o Language = _("Language", "Язык");
		public static o Device = _("Device", "Устройство");
		public static o Window = _("Window", "Окно");
		public static o Harmonics = _("Harmonics", "Гармоники");
		public static o Notes = _("Notes", "Ноты");
		public static o Sample = _("Sample", "Сэмпл");
		public static o Scaling = _("Scaling", "Масштабирование");
		public static o Frequency = _("Frequency", "Частота");
		public static o Magnitude = _("Magnitude", "Амплитуда");
		public static o Phase = _("Phase", "Фаза");
		public static o Bandwidth = _("Bandwidth", "Полоса пропускания");
		public static o Notation = _("Notation", "Нотация");
		public static o Threshold = _("Threshold", "Порог");
		public static o Value = _("Value", "Значение");
		public static o Limit = _("Limit", "Предел");
		public static o Lower = _("Lower", "Нижний");
		public static o Upper = _("Upper", "Верхний");
		public static o Length = _("Length", "Длина");
		public static o Spectrum = _("Spectrum", "Спектр");
		public static o Spectrogram = _("Spectrogram", "Спектрограмма");
		public static o Histogram = _("Histogram", "Гистограмма");
		public static o Geometry = _("Geometry", "Геометрия");
		public static o Flower = _("Flower", "Цветок");
		public static o Piano = _("Piano", "Пианино");
		public static o Offset = _("Offset", "Смещение");
		public static o Scale = _("Scale", "Масштаб");
		public static o Profile = _("Profile", "Профиль");
		public static o Generator = _("Generator", "Генератор");
		public static o Agreement = _("Agreement", "Соглашение");
		public static o Create = _("Create", "Создать");
		public static o Delete = _("Delete", "Удалить");
		public static o Basis = _("Basis", "Базис");
		public static o Mode = _("Mode", "Режим");
		public static o Title = _("Title", "Заголовок");
		public static o Command = _("Command", "Команда");
		public static o Action = _("Action", "Действие");
		public static o Flow = _("Flow", "Поток");
		public static o Loop = _("Loop", "Петля");
		public static o Sound = _("Sound", "Звук");
		public static o Signal = _("Signal", "Сигнал");
		public static o Input = _("Input", "Ввод");
		public static o Output = _("Output", "Вывод");
		public static o Monitor = _("Monitor", "Монитор");
		public static o Screen = _("Screen", "Экран");
		public static o Brush = _("Brush", "Кисть");
		public static o Color = _("Color", "Цвет");
		public static o Gradient = _("Gradient", "Градиент");
		public static o Center = _("Center", "Центр");
		public static o Radius = _("Radius", "Радиус");
		public static o Linear = _("Linear", "Линейный");
		public static o Radial = _("Radial", "Радиальный");
		public static o Solid = _("Solid", "Сплошной");
		public static o Stops = _("Stops", "Остановки");
		public static o Theme = _("Theme", "Тема");
		public static o From = _("From", "От");
		public static o Till = _("Till", "До");
		public static o Fill = _("Fill", "Заливка");
		public static o Stroke = _("Stroke", "Обводка");
		public static o Background = _("Background", "Фон");
		public static o Thickness = _("Thickness", "Толщина");
		public static o Rectangle = _("Rectangle", "Прямоугольник");
		public static o State = _("State", "Состояние");
		public static o Index = _("Index", "Индекс");
		public static o Grid = _("Grid", "Сетка");
		public static o Base = _("Base", "База");
		public static o Gap = _("Gap", "Зазор");
		public static o Raw = _("Raw", "Сырой");
		public static o Top = _("Top", "Вершина");
		public static o Music = _("Music", "Музыка");
		public static o Standard = _("Standard", "Стандарт");
		public static o Visibility = _("Visibility", "Видимость");
		public static o Splitter = _("Splitter", "Разделитель");
		public static o Soundless = _("Soundless", "Беззвучный");
		public static o Sounding = _("Sounding", "Звучащий");
		public static o Palette = _("Palette", "Палитра");
		public static o Reset = _("Reset", "Сброс");
		public static o Rate = _("Rate", "Мера");
		public static o Buffers = _("Buffers", "Буферы");
		public static o Format = _("Format", "Формат");
		public static o Numeric = _("Numeric", "Числовой");
		public static o Projection = _("Projection", "Проекция");
		public static o Perspective = _("Perspective", "Перспектива");
		public static o Orthographic = _("Orthographic", "Ортография");
		public static o Depth = _("Depth", "Глубина");
		public static o Thin = _("Thin", "Утончение");
		public static o Angle = _("Angle", "Угол");
		public static o Tape = _("Tape", "Лента");

		public static o Pcs = _("Pcs", "шт.");
		public static o Hz = _("Hz", "Гц");
		public static o ms = _("ms", "мс");

		public static o FFT = _("FFT", "БПФ");
		public static o PMI = _("PMI", "ФАИ");

		public static o FullTone = _("Full Tone", "Целотон");
		public static o HalfTone = _("Half Size", "Полутон");
		public static o PianoKey = _("Piano Key", "Клавиша");
		public static o FrameSize = _("Frame Size", "Размер кадра");
		public static o LowFrequency = _("Low Frequency", "Нижняя частота");
		public static o TopFrequency = _("Top Frequency", "Верхняя частота");
		public static o SampleRate = _("Sample Rate", "Частота дискретизации");
		
		public static o NumericFormatting = _("Numeric Formatting", "Числовое форматирование");

		public static o AgreementMessage = _(Messages.Agreement.English, Messages.Agreement.Russian);
		public static o MadeByMessage = _(Messages.MadeBy.English, Messages.MadeBy.Russian);
		public static o ReadyToHelpMessage = _(Messages.ReadyToHelpMessage.English, Messages.ReadyToHelpMessage.Russian);
		public static o ReadyToHelpLink = _(Messages.ReadyToHelpLink.English, Messages.ReadyToHelpLink.Russian);
		public static o ExpirationMessage = _(Messages.ExpirationMessage.English, Messages.ExpirationMessage.Russian);
		public static o HomeLink = _(Messages.HomeLink.English, Messages.HomeLink.Russian);

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
		public override string GetString(string key) => _keyToValue.TryGetValue(key, out var value) ? value : default;
		public override string GetString(string key, CultureInfo culture) => _keyToValue.TryGetValue(key, out var value) ? value : default;
	}

	public static class Messages
	{
		public static class Agreement
		{
			public const string English =
				@"Solfeggio application is free for educational and non-commercial purposes.

If you use this program on regular commertial fit or just really like it and want to support developing,
please, buy a paid version or make voluntary donation of any amount.

Your help is needed and priceless!";

			public const string Russian =
				@"Приложение Solfeggio бесплатно для образовательных и некоммерческих целей.

Если вы используете эту программу на регулярной коммерческой основе
или просто она вам действительно нравится и есть желание поддержать разработку,
пожалуйста, купите платную версию или сделайте добровольное пожертвование на любую сумму.

Ваша помощь необходима и бесценна!";
		}

		public static class MadeBy
		{
			public const string English = @"Made by Makeloft Studio";
			public const string Russian = @"Сделано студией Makeloft";
		}

		public static class ReadyToHelpMessage
		{
			public const string English = @"Ready to help!";
			public const string Russian = @"Готов помочь!";
		}

		public static class ReadyToHelpLink
		{
			public const string English = @"http://makeloft.xyz/workroom/solfeggio";
			public const string Russian = @"http://makeloft.xyz/ru/workroom/solfeggio";
		}

		public static class ExpirationMessage
		{
			public const string English = @"Your version has been expired.
Please, download the newest release.";
			public const string Russian = @"Ваша версия устарела.
Пожалуйста, загрузите новейший релиз.";
		}

		public static class HomeLink
		{
			public const string English = @"http://makeloft.xyz/workroom/solfeggio";
			public const string Russian = @"http://makeloft.xyz/ru/workroom/solfeggio";
		}
	}
}
