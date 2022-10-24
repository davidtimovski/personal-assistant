import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import Variables from '$lib/variables';
import type { Forecast } from '$lib/models/forecast';

export class ForecastsService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Weatherman');

	get(
		latitude: number,
		longitude: number,
		temperatureUnit: string,
		precipitationUnit: string,
		windSpeedUnit: string
	): Promise<Forecast> {
		const date = new Date();
		return this.httpProxy.ajax<Forecast>(
			`${Variables.urls.api}/api/forecasts?latitude=${latitude}&longitude=${longitude}&temperatureunit=${temperatureUnit}&precipitationunit=${precipitationUnit}&windspeedunit=${windSpeedUnit}&date=2022-10-23T21:27:00`
		);
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
