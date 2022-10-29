import type { WeatherCode } from '$lib/models/weatherCode';

export class Forecast {
	constructor(
		public temperature: number,
		public precipitation: number,
		public windSpeed: number,
		public weatherCode: WeatherCode,
		public hourly: ForecastHourly[]
	) {}
}

export class ForecastHourly {
	constructor(
		public time: string,
		public temperature: number,
		public precipitation: number,
		public windSpeed: number,
		public weatherCode: WeatherCode
	) {}
}
