namespace Solfeggio.Localization.Data;

public static class Errors
{
	public static class Range
	{
		public const string English = @"The input value is out of the allowed range.";
		public const string Russian = @"Вводимое значение находится за пределами допустимого диапазона.";
	}

	public static class Format
	{
		public const string English = @"The value format is not recognized.";
		public const string Russian = @"Формат значения не удалось распознать.";
	}

	public static class Unidentified
	{
		public const string English = @"An unidentified error occured.";
		public const string Russian = @"Произошла неопознанная ошибка.";
	}
}
