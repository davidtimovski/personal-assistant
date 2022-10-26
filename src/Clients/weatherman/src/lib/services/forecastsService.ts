import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { forecast } from '$lib/stores';
import type { Forecast } from '$lib/models/forecast';
import Variables from '$lib/variables';

export class ForecastsService {
	private readonly localStorage = new LocalStorageUtil();
	private readonly httpProxy = new HttpProxy();

	get() {
		navigator.geolocation.getCurrentPosition(async (position) => {
			const latitude = Math.floor(position.coords.latitude * 100) / 100;
			const longitude = Math.floor(position.coords.longitude * 100) / 100;
			const temperatureUnit = this.localStorage.get(LocalStorageKeys.TemperatureUnit);
			const precipitationUnit = this.localStorage.get(LocalStorageKeys.PrecipitationUnit);
			const windSpeedUnit = this.localStorage.get(LocalStorageKeys.WindSpeedUnit);
			const date = DateHelper.formatISO(new Date());

			const data = await this.httpProxy.ajax<Forecast>(
				`${Variables.urls.api}/api/forecasts?latitude=${latitude}&longitude=${longitude}&temperatureunit=${temperatureUnit}&precipitationunit=${precipitationUnit}&windspeedunit=${windSpeedUnit}&date=${date}`
			);
			forecast.set(data);
		});
	}

	release() {
		this.httpProxy.release();
	}
}
