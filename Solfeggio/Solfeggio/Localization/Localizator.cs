using Solfeggio.Localization.Data;
using System.Runtime.CompilerServices;

namespace Solfeggio.Localization;

using _ = Translations;

public static partial class Localizator
{
	private static _ _(
		[CallerMemberName]
		string eng = default,
		string rus = default,
		string bel = default)
		=> new
		(
			english: eng,
			russian: rus ?? eng,
			belorussian: bel ?? rus ?? eng
		);

	public static _ Solfeggio = _("Solfeggio", "Сольфеджио");
	public static _ Сalibration = _("Сalibration", "Калибровка", "Каліброўка");
	public static _ Visualization = _("Visualization", "Визуализация", "Візуалізацыя");
	public static _ Adaptation = _("Adaptation", "Адаптация");
	public static _ Advanced = _("Advanced", "Расширенные");
	public static _ Telemetry = _("Telemetry", "Телеметрия");
	public static _ Sensitive = _("Sensitive", "Чувствительность");
	public static _ Level = _("Level", "Уровень");
	public static _ Short = _("Short", "Кратко");
	public static _ Long = _("Long", "Долго");
	public static _ Speed = _("Speed", "Скорость");
	public static _ Manually = _("Manually", "Вручную");
	public static _ Automatically = _("Automatically", "Автоматичеки");
	public static _ Whisper = _("Whisper", "Шёпот");
	public static _ Singing = _("Singing", "Пение");
	public static _ Frame = _("Frame", "Кадр");
	public static _ Size = _("Size", "Размер");
	public static _ Duration = _("Duration", "Длительность");
	public static _ Resolution = _("Resolution", "Разрешение");
	public static _ Accuracy = _("Accuracy", "Точность");
	public static _ Step = _("Step", "Шаг");
	public static _ Wave = _("Wave", "Волна");
	public static _ Language = _("Language", "Язык");
	public static _ Device = _("Device", "Устройство");
	public static _ Window = _("Window", "Окно");
	public static _ Harmonics = _("Harmonics", "Гармоники");
	public static _ Notes = _("Notes", "Ноты");
	public static _ Sample = _("Sample", "Сэмпл");
	public static _ Scaling = _("Scaling", "Масштабирование");
	public static _ Frequency = _("Frequency", "Частота");
	public static _ Magnitude = _("Magnitude", "Амплитуда");
	public static _ Phase = _("Phase", "Фаза");
	public static _ Scope = _("Scope", "Рамки");
	public static _ Bandwidth = _("Bandwidth", "Полоса пропускания");
	public static _ Notation = _("Notation", "Нотация");
	public static _ Threshold = _("Threshold", "Порог");
	public static _ Value = _("Value", "Значение");
	public static _ Limit = _("Limit", "Предел");
	public static _ Lower = _("Lower", "Нижний");
	public static _ Upper = _("Upper", "Верхний");
	public static _ Length = _("Length", "Длина");
	public static _ Spectrum = _("Spectrum", "Спектр");
	public static _ Spectrogram = _("Spectrogram", "Спектрограмма");
	public static _ Histogram = _("Histogram", "Гистограмма");
	public static _ Geometry = _("Geometry", "Геометрия");
	public static _ Spiral = _("Spiral", "Спираль");
	public static _ Flower = _("Flower", "Цветок");
	public static _ Piano = _("Piano", "Пианино");
	public static _ Offset = _("Offset", "Смещение");
	public static _ Scale = _("Scale", "Масштаб");
	public static _ Profile = _("Profile", "Профиль");
	public static _ Generator = _("Generator", "Генератор");
	public static _ Agreement = _("Agreement", "Соглашение");
	public static _ Create = _("Create", "Создать");
	public static _ Delete = _("Delete", "Удалить");
	public static _ Copy = _("Copy", "Копировать");
	public static _ Load = _("Load", "Загрузить");
	public static _ Open = _("Open", "Открыть");
	public static _ Save = _("Save", "Сохранить");
	public static _ Basis = _("Basis", "Базис");
	public static _ Mode = _("Mode", "Режим");
	public static _ Title = _("Title", "Заголовок");
	public static _ Command = _("Command", "Команда");
	public static _ Action = _("Action", "Действие");
	public static _ Menu = _("Menu", "Меню");
	public static _ Each = _("Each", "Каждый");
	public static _ Mute = _("Mute", "Немой");
	public static _ Loud = _("Loud", "Громкий");
	public static _ Flow = _("Flow", "Поток");
	public static _ Loop = _("Loop", "Петля");
	public static _ Sound = _("Sound", "Звук");
	public static _ Signal = _("Signal", "Сигнал");
	public static _ Input = _("Input", "Ввод");
	public static _ Output = _("Output", "Вывод");
	public static _ Monitor = _("Monitor", "Монитор");
	public static _ Screen = _("Screen", "Экран");
	public static _ Brush = _("Brush", "Кисть");
	public static _ Color = _("Color", "Цвет");
	public static _ Gradient = _("Gradient", "Градиент");
	public static _ Center = _("Center", "Центр");
	public static _ Radius = _("Radius", "Радиус");
	public static _ Linear = _("Linear", "Линейный");
	public static _ Radial = _("Radial", "Радиальный");
	public static _ Solid = _("Solid", "Сплошной");
	public static _ Stops = _("Stops", "Остановки");
	public static _ Theme = _("Theme", "Тема");
	public static _ From = _("From", "От");
	public static _ Till = _("Till", "До");
	public static _ Fill = _("Fill", "Заливка");
	public static _ Stroke = _("Stroke", "Обводка");
	public static _ Topmost = _("Topmost", "Поверх");
	public static _ Visible = _("Visible", "Видимый");
	public static _ Snapshot = _("Snapshot", "Снимок");
	public static _ Background = _("Background", "Фон");
	public static _ Thickness = _("Thickness", "Толщина");
	public static _ Rectangle = _("Rectangle", "Прямоугольник");
	public static _ State = _("State", "Состояние");
	public static _ Index = _("Index", "Индекс");
	public static _ Grid = _("Grid", "Сетка");
	public static _ Base = _("Base", "База");
	public static _ Gap = _("Gap", "Зазор");
	public static _ Raw = _("Raw", "Сырой");
	public static _ Peak = _("Peak", "Пик");
	public static _ Music = _("Music", "Музыка");
	public static _ Standard = _("Standard", "Стандарт");
	public static _ Visibility = _("Visibility", "Видимость");
	public static _ Splitter = _("Splitter", "Разделитель");
	public static _ Soundless = _("Soundless", "Беззвучный");
	public static _ Sounding = _("Sounding", "Звучащий");
	public static _ Palette = _("Palette", "Палитра");
	public static _ Reset = _("Reset", "Сброс");
	public static _ Rate = _("Rate", "Мера");
	public static _ Buffers = _("Buffers", "Буферы");
	public static _ Format = _("Format", "Формат");
	public static _ Numeric = _("Numeric", "Числовой");
	public static _ Projection = _("Projection", "Проекция");
	public static _ Perspective = _("Perspective", "Перспектива");
	public static _ Orthographic = _("Orthographic", "Ортография");
	public static _ Approximation = _("Approximation", "Апроксимация");
	public static _ Joystick = _("Joystick", "Джойстик");
	public static _ Camera = _("Camera", "Камера");
	public static _ Depth = _("Depth", "Глубина");
	public static _ Thin = _("Thin", "Утончение");
	public static _ Angle = _("Angle", "Угол");
	public static _ Tape = _("Tape", "Лента");
	public static _ Note = _("Note", "Нота");
	public static _ Ethalon = _("Ethalon", "Эталон");
	public static _ Vocal = _("Vocal", "Вокал");
	public static _ Tuning = _("Tuning", "Тюнинг");
	public static _ Camertone = _("Camertone", "Камертон");
	public static _ Resonance = _("Resonance", "Резонанс");
	public static _ Harmony = _("Harmony", "Гармония");
	public static _ Fantasy = _("Fantasy", "Фантазия");
	public static _ Relax = _("Relax", "Релакс");
	public static _ Lace = _("Lace", "Кружево");
	public static _ Speaker = _("Speaker", "Динамик");
	public static _ Microphone = _("Microphone", "Микрофон");
	public static _ Dies = _("Dies", "Диез");
	public static _ Bemole = _("Bemole", "Бемоль");
	public static _ Combined = _("Combined", "Комбинировано");
	public static _ Range = _("Range", "Диапазон");
	public static _ Error = _("Error", "Ошибка");
	public static _ Oops = _("Oops", "Упс");
	public static _ Ok = _("Ok", "Ок");

	public static _ English = _("English", "Английский");
	public static _ Russian = _("Russian", "Русский");

	public static _ Pcs = _("Pcs", "шт");
	public static _ Rad = _("rad", "рад");
	public static _ Hz = _("Hz", "Гц");
	public static _ ms = _("ms", "мс");

	public static _ FFT = _("FFT", "БПФ");
	public static _ PMI = _("PMI", "ФАИ");

	public static _ FullTone = _("Full Tone", "Целотон");
	public static _ HalfTone = _("Half Tone", "Полутон");
	public static _ PianoKey = _("Piano Key", "Клавиша");
	public static _ FrameSize = _("Frame Size", "Размер кадра");
	public static _ LowFrequency = _("Low Frequency", "Нижняя частота");
	public static _ TopFrequency = _("Top Frequency", "Верхняя частота");
	public static _ SampleRate = _("Sample Rate", "Частота дискретизации");

	public static _ NumericFormatting = _("Numeric Formatting", "Числовое форматирование");

	public static _ MadeByMessage = _(Messages.MadeBy.English, Messages.MadeBy.Russian);
	public static _ AgreementMessage = _(Messages.Agreement.English, Messages.Agreement.Russian);
	public static _ ExpirationMessage = _(Messages.Expiration.English, Messages.Expiration.Russian);
	public static _ ReadyToHelpMessage = _(Messages.ReadyToHelp.English, Messages.ReadyToHelp.Russian);
	public static _ MicrophoneAccessMessage = _(Messages.MicrophoneAccess.English, Messages.MicrophoneAccess.Russian);

	public static _ HomeLink = _(Links.Home.English, Links.Home.Russian);
	public static _ ReadyToHelpLink = _(Links.ReadyToHelp.English, Links.ReadyToHelp.Russian);

	public static _ UnidentifiedError = _(Errors.Unidentified.English, Errors.Unidentified.Russian);
	public static _ FormatError = _(Errors.Format.English, Errors.Format.Russian);
	public static _ RangeError = _(Errors.Range.English, Errors.Range.Russian);
}
