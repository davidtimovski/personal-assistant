import type { WeatherCode } from '$lib/models/weatherCode';

export class Forecast {
	initialized = false;
	lastRetrieved: number | null = null;
	weekDays: { date: string; weekDay: string }[] = [];

	constructor(
		public weatherCode: WeatherCode | null,
		public temperature: number | null,
		public apparentTemperature: number | null,
		public precipitation: number | null,
		public windSpeed: number | null,
		public timeOfDay: TimeOfDay | null,
		public hourly: HourlyForecast[],
		public nextDays: DailyForecast[]
	) {}
}

export class DailyForecast {
	constructor(
		public date: string,
		public weatherCode: WeatherCode,
		public temperatureMax: number,
		public temperatureMin: number,
		public precipitation: number,
		public hourly: HourlyForecast[]
	) {}
}

export class HourlyForecast {
	constructor(
		public hour: number,
		public timeString: string,
		public weatherCode: WeatherCode | null,
		public temperature: number | null,
		public apparentTemperature: number | null,
		public timeOfDay: TimeOfDay | null
	) {}
}

export enum TimeOfDay {
	Day,
	SunLow,
	SunLower,
	Night
}
