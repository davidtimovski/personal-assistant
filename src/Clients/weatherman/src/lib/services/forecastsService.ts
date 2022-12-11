import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { alertState, forecast } from '$lib/stores';
import type { Forecast } from '$lib/models/forecast';
import Variables from '$lib/variables';

export class ForecastsService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Weatherman');
	private readonly localStorage = new LocalStorageUtil();

	get(culture: string) {
		try {
			const now = new Date();

			navigator.geolocation.getCurrentPosition(
				async (position) => {
					const latitude = Math.floor(position.coords.latitude * 100) / 100;
					const longitude = Math.floor(position.coords.longitude * 100) / 100;
					const temperatureUnit = this.localStorage.get(LocalStorageKeys.TemperatureUnit);
					const precipitationUnit = this.localStorage.get(LocalStorageKeys.PrecipitationUnit);
					const windSpeedUnit = this.localStorage.get(LocalStorageKeys.WindSpeedUnit);
					const time = DateHelper.adjustTimeZone(now).toISOString();

					const data = await this.httpProxy.ajax<Forecast>(
						`${Variables.urls.api}/forecasts?latitude=${latitude}&longitude=${longitude}&temperatureunit=${temperatureUnit}&precipitationunit=${precipitationUnit}&windspeedunit=${windSpeedUnit}&time=${time}`
					);
					data.lastRetrieved = now.valueOf();

					now.setMinutes(0);
					for (const hourly of data.hourly) {
						now.setHours(hourly.hour);
						hourly.timeString = DateHelper.formatHours(now, culture);
					}

					for (const nextDay of data.nextDays) {
						for (const hourly of nextDay.hourly) {
							now.setHours(hourly.hour);
							hourly.timeString = DateHelper.formatHours(now, culture);
						}
					}

					forecast.set(data);
				},
				() => {
					alertState.update((x) => {
						x.showError('unableToRetrieveLocation');
						return x;
					});
				}
			);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
