import type { WeatherCode } from '$lib/models/weatherCode';

export class Forecast {
	constructor(
		public weatherCode: WeatherCode,
		public temperature: number,
		public apparentTemperature: number,
		public precipitation: number,
		public windSpeed: number,
		public isNight: boolean,
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
		public hourly: HourlyForecast[]
	) {}
}

export class HourlyForecast {
	constructor(
		public hour: number,
		public weatherCode: WeatherCode,
		public temperature: number,
		public apparentTemperature: number,
		public precipitation: number,
		public windSpeed: number,
		public isNight: boolean,
		public timeString: string
	) {}
}
