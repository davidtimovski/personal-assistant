import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { alertState, forecast } from '$lib/stores';
import { Forecast, HourlyForecast } from '$lib/models/forecast';
import Variables from '$lib/variables';

export class ForecastsService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Weatherman');
	private readonly localStorage = new LocalStorageUtil();

	setPlaceholder(now: Date, language: string, culture: string) {
		const hours = [];
		const hourCounter = new Date(now.getFullYear(), now.getMonth(), now.getDate(), now.getHours(), 0, 0, 0);
		for (let i = 0; i < 24; i++) {
			hourCounter.setHours(hourCounter.getHours() + 1);
			hours.push(
				new HourlyForecast(now.getHours(), DateHelper.formatHours(hourCounter, culture), null, null, null, null)
			);
		}

		const placeholder = new Forecast(null, null, null, null, null, null, hours, []);
		placeholder.weekDays = this.getWeekDays(now, language);

		forecast.set(placeholder);
	}

	get(now: Date, language: string, culture: string) {
		try {
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
					data.initialized = true;
					data.lastRetrieved = now.valueOf();
					data.weekDays = this.getWeekDays(now, language);

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

	private getWeekDays(currentDate: Date, language: string) {
		const dateCounter = new Date();
		const weekDays = [
			{
				date: DateHelper.format(currentDate),
				weekDay: DateHelper.formatWeekdayShort(dateCounter, language)
			}
		];
		for (let i = 0; i < 5; i++) {
			dateCounter.setDate(dateCounter.getDate() + 1);

			weekDays.push({
				date: DateHelper.format(dateCounter),
				weekDay: DateHelper.formatWeekdayShort(dateCounter, language)
			});
		}

		return weekDays;
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
