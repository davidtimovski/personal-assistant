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
		public hourly: ForecastHourly[]
	) {}
}

export class ForecastDaily {
	constructor(public weatherCode: WeatherCode, public temperatureMax: number, public temperatureMin: number) {}
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
		public timeString: string
	) {}
}
