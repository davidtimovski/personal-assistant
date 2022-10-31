import type { WeatherCode } from '$lib/models/weatherCode';

export class Forecast {
	constructor(
		public temperature: number,
		public apparentTemperature: number,
		public precipitation: number,
		public windSpeed: number,
		public weatherCode: WeatherCode,
		public isNight: boolean,
		public daily: ForecastDaily[],
		public hourly: ForecastHourly[],
		public illustrationSrc: string
	) {}
}

export class ForecastDaily {
	constructor(
		public weatherCode: WeatherCode,
		public temperatureMax: number,
		public temperatureMin: number,
		public illustrationSrc: string
	) {}
}

export class ForecastHourly {
	constructor(
		public hour: number,
		public temperature: number,
		public apparentTemperature: number,
		public precipitation: number,
		public windSpeed: number,
		public weatherCode: WeatherCode,
		public isNight: boolean,
		public timeString: string,
		public illustrationSrc: string
	) {}
}
