﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Solfeggio
{
	using o = Dictionary<LanguageCodes, string>;

	public enum LanguageCodes { Default, English, Russian, Belorussian };

	public static class Localizator
	{
		private static o _(
			[CallerMemberName]
			string eng = default,
			string rus = default,
			string bel = default) => new()
		{
			{ LanguageCodes.English, eng },
			{ LanguageCodes.Russian, rus ?? eng },
			{ LanguageCodes.Belorussian, bel ?? rus ?? eng },
		};

		public static o Solfeggio = _("Solfeggio", "Сольфеджио");
		public static o Сalibration = _("Сalibration", "Калибровка", "Каліброўка");
		public static o Visualization = _("Visualization", "Визуализация", "Візуалізацыя");
		public static o Adaptation = _("Adaptation", "Адаптация");
		public static o Advanced = _("Advanced", "Расширенные");
		public static o Telemetry = _("Telemetry", "Телеметрия");
		public static o Sensitive = _("Sensitive", "Чувствительность");
		public static o Level = _("Level", "Уровень");
		public static o Short = _("Short", "Кратко");
		public static o Long = _("Long", "Долго");
		public static o Speed = _("Speed", "Скорость");
		public static o Manually = _("Manually", "Вручную");
		public static o Automatically = _("Automatically", "Автоматичеки");
		public static o Whisper = _("Whisper", "Шёпот");
		public static o Singing = _("Singing", "Пение");
		public static o Frame = _("Frame", "Кадр");
		public static o Size = _("Size", "Размер");
		public static o Duration = _("Duration", "Длительность");
		public static o Resolution = _("Resolution", "Разрешение");
		public static o Accuracy = _("Accuracy", "Точность");
		public static o Step = _("Step", "Шаг");
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
		public static o Scope = _("Scope", "Рамки");
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
		public static o Spiral = _("Spiral", "Спираль");
		public static o Flower = _("Flower", "Цветок");
		public static o Piano = _("Piano", "Пианино");
		public static o Offset = _("Offset", "Смещение");
		public static o Scale = _("Scale", "Масштаб");
		public static o Profile = _("Profile", "Профиль");
		public static o Generator = _("Generator", "Генератор");
		public static o Agreement = _("Agreement", "Соглашение");
		public static o Create = _("Create", "Создать");
		public static o Delete = _("Delete", "Удалить");
		public static o Copy = _("Copy", "Копировать");
		public static o Load = _("Load", "Загрузить");
		public static o Open = _("Open", "Открыть");
		public static o Save = _("Save", "Сохранить");
		public static o Basis = _("Basis", "Базис");
		public static o Mode = _("Mode", "Режим");
		public static o Title = _("Title", "Заголовок");
		public static o Command = _("Command", "Команда");
		public static o Action = _("Action", "Действие");
		public static o Menu = _("Menu", "Меню");
		public static o Each = _("Each", "Каждый");
		public static o Mute = _("Mute", "Немой");
		public static o Loud = _("Loud", "Громкий");
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
		public static o Topmost = _("Topmost", "Поверх");
		public static o Visible = _("Visible", "Видимый");
		public static o Snapshot = _("Snapshot", "Снимок");
		public static o Background = _("Background", "Фон");
		public static o Thickness = _("Thickness", "Толщина");
		public static o Rectangle = _("Rectangle", "Прямоугольник");
		public static o State = _("State", "Состояние");
		public static o Index = _("Index", "Индекс");
		public static o Grid = _("Grid", "Сетка");
		public static o Base = _("Base", "База");
		public static o Gap = _("Gap", "Зазор");
		public static o Raw = _("Raw", "Сырой");
		public static o Peak = _("Peak", "Пик");
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
		public static o Approximation = _("Approximation", "Апроксимация");
		public static o Joystick = _("Joystick", "Джойстик");
		public static o Camera = _("Camera", "Камера");
		public static o Depth = _("Depth", "Глубина");
		public static o Thin = _("Thin", "Утончение");
		public static o Angle = _("Angle", "Угол");
		public static o Tape = _("Tape", "Лента");
		public static o Note = _("Note", "Нота");
		public static o Ethalon = _("Ethalon", "Эталон");
		public static o Vocal = _("Vocal", "Вокал");
		public static o Tuning = _("Tuning", "Тюнинг");
		public static o Camertone = _("Camertone", "Камертон");
		public static o Resonance = _("Resonance", "Резонанс");
		public static o Harmony = _("Harmony", "Гармония");
		public static o Fantasy = _("Fantasy", "Фантазия");
		public static o Relax = _("Relax", "Релакс");
		public static o Speaker = _("Speaker", "Динамик");
		public static o Microphone = _("Microphone", "Микрофон");
		public static o Dies = _("Dies", "Диез");
		public static o Bemole = _("Bemole", "Бемоль");
		public static o Combined = _("Combined", "Комбинировано");
		public static o Range = _("Range", "Диапазон");
		public static o Oops = _("Oops", "Упс");
		public static o Ok = _("Ok", "Ок");

		public static o English = _("English", "Английский");
		public static o Russian = _("Russian", "Русский");

		public static o Pcs = _("Pcs", "шт");
		public static o Rad = _("rad", "рад");
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
		public static o MicrophoneAccessErrorMessage = _(Messages.MicrophoneAccessErrorMessage.English, Messages.MicrophoneAccessErrorMessage.Russian);

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
@"This application is free for educational and non-commercial purposes.

If you use the program on regular commertial fit or just really like it,
please, write a review or make voluntary donation of any amount 
to support developing.

Your help is priceless!";

			public const string Russian =
@"Это приложение бесплатно для образовательных и некоммерческих целей.

Если вы используете данную программу на регулярной 
коммерческой основе или просто она вам действительно нравится,
пожалуйста, напишите отзыв или сделайте добровольное 
пожертвование на любую сумму, чтобы поддержать разработку.

Ваша помощь бесценна!";
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
			public const string English = @"https://gitlab.com/Makeloft-Studio/Solfeggio/-/wikis/Thank-for-Care!";
			public const string Russian = @"https://gitlab.com/Makeloft-Studio/Solfeggio/-/wikis/Спасибо-за-заботу!";
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

		public static class MicrophoneAccessErrorMessage
		{
			public const string English =
				"Access to the microphone required for proper work of the app.\n\n" +
				"Please, grant the permission at system settings or try to reinstall the program.";
			public const string Russian =
				"Доступ к микрофону необходим для надлежащей работы приложения.\n\n" +
				"Пожалуйста, предоставьте разрешение в системных настройках или попробуйте переустановить программу.";
		}

	}
}
