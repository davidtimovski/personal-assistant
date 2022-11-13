import type { WeatherCode } from '$lib/models/weatherCode';

export class Forecast {
	lastRetrieved: number | null = null;

	constructor(
		public weatherCode: WeatherCode,
		public temperature: number,
		public apparentTemperature: number,
		public precipitation: number,
		public windSpeed: number,
		public timeOfDay: TimeOfDay,
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
		public weatherCode: WeatherCode,
		public temperature: number,
		public apparentTemperature: number,
		public timeOfDay: TimeOfDay,
		public timeString: string
	) {}
}

export enum TimeOfDay {
	Day,
	SunLow,
	SunLower,
	Night
}
