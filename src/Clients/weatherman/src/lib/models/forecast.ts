export class Forecast {
	constructor(
		public temperature: number,
		public precipitation: number,
		public windSpeed: number,
		public weatherCode: number,
		public hourly: ForecastHourly[]
	) {}
}

export class ForecastHourly {
	constructor(
		public time: string,
		public temperature: number,
		public precipitation: number,
		public windSpeed: number,
		public weatherCode: number
	) {}
}
