import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { alertState, forecast } from '$lib/stores';
import type { Forecast } from '$lib/models/forecast';
import Variables from '$lib/variables';
import { WeatherCode } from '$lib/models/weatherCode';

export class ForecastsService {
	private readonly localStorage = new LocalStorageUtil();
	private readonly httpProxy = new HttpProxy();

	get(locale: string) {
		const now = new Date();

		navigator.geolocation.getCurrentPosition(
			async (position) => {
				const latitude = Math.floor(position.coords.latitude * 100) / 100;
				const longitude = Math.floor(position.coords.longitude * 100) / 100;
				const temperatureUnit = this.localStorage.get(LocalStorageKeys.TemperatureUnit);
				const precipitationUnit = this.localStorage.get(LocalStorageKeys.PrecipitationUnit);
				const windSpeedUnit = this.localStorage.get(LocalStorageKeys.WindSpeedUnit);
				const dateString = DateHelper.formatISO(now);

				const data = await this.httpProxy.ajax<Forecast>(
					`${Variables.urls.api}/api/forecasts?latitude=${latitude}&longitude=${longitude}&temperatureunit=${temperatureUnit}&precipitationunit=${precipitationUnit}&windspeedunit=${windSpeedUnit}&date=${dateString}`
				);

				//data.illustrationSrc = this.getIllustrationSrc(data.weatherCode, data.isNight);

				now.setMinutes(0);
				for (const hourly of data.hourly) {
					now.setHours(hourly.hour);

					hourly.timeString = DateHelper.formatHours(now, locale);
					//hourly.illustrationSrc = this.getIllustrationSrc(hourly.weatherCode, hourly.isNight);
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
	}

	// private getIllustrationSrc(weatherCode: WeatherCode, isNight: boolean) {
	// 	switch (weatherCode) {
	// 		case WeatherCode.ClearSky:
	// 		case WeatherCode.MainlyClear:
	// 			if (isNight) {
	// 				return '/images/weather/moon.svg';
	// 			}
	// 			return '/images/weather/sun.svg';
	// 		case WeatherCode.PartlyCloudy:
	// 			if (isNight) {
	// 				return '/images/weather/cloud-moon.svg';
	// 			}
	// 			return '/images/weather/cloud-sun.svg';
	// 		case WeatherCode.Overcast:
	// 			return '/images/weather/cloudy.svg';
	// 		case WeatherCode.Fog:
	// 		case WeatherCode.DepositingRimeFog:
	// 			return '/images/weather/fog.svg';
	// 		case WeatherCode.DrizzleLight:
	// 		case WeatherCode.DrizzleLightFreezing:
	// 			if (isNight) {
	// 				return '/images/weather/drizzle-light-moon.svg';
	// 			}
	// 			return '/images/weather/drizzle-light-sun.svg';
	// 		case WeatherCode.DrizzleModerate:
	// 		case WeatherCode.DrizzleDense:
	// 		case WeatherCode.DrizzleDenseFreezing:
	// 			if (isNight) {
	// 				return '/images/weather/drizzle-heavy-moon.svg';
	// 			}
	// 			return '/images/weather/drizzle-heavy-sun.svg';
	// 		case WeatherCode.RainLight:
	// 			return '/images/weather/rain-light.svg';
	// 		case WeatherCode.RainModerate:
	// 		case WeatherCode.RainHeavy:
	// 			return '/images/weather/rain-heavy.svg';
	// 		case WeatherCode.SnowLight:
	// 			if (isNight) {
	// 				return '/images/weather/snow-light-moon.svg';
	// 			}
	// 			return '/images/weather/snow-light-sun.svg';
	// 		case WeatherCode.SnowModerate:
	// 		case WeatherCode.SnowHeavy:
	// 		case WeatherCode.SnowGrains:
	// 			return '/images/weather/snow-heavy.svg';
	// 		case WeatherCode.ShowerLight:
	// 			if (isNight) {
	// 				return '/images/weather/shower-light-moon.svg';
	// 			}
	// 			return '/images/weather/shower-light-sun.svg';
	// 		case WeatherCode.ShowerModerate:
	// 		case WeatherCode.ShowerViolent:
	// 			if (isNight) {
	// 				return '/images/weather/shower-heavy-moon.svg';
	// 			}
	// 			return '/images/weather/shower-heavy-sun.svg';
	// 		case WeatherCode.SnowShowerLight:
	// 			if (isNight) {
	// 				return '/images/weather/snow-light-moon.svg';
	// 			}
	// 			return '/images/weather/snow-light-sun.svg';
	// 		case WeatherCode.SnowShowerHeavy:
	// 			return '/images/weather/snow-heavy.svg';
	// 		case WeatherCode.Thunderstorm:
	// 			return '/images/weather/thunderstorm.svg';
	// 		case WeatherCode.ThunderstormWithHailLight:
	// 			if (isNight) {
	// 				return '/images/weather/hail-light-moon.svg';
	// 			}
	// 			return '/images/weather/hail-light-sun.svg';
	// 		case WeatherCode.ThunderstormWithHailHeavy:
	// 			if (isNight) {
	// 				return '/images/weather/hail-heavy-moon.svg';
	// 			}
	// 			return '/images/weather/hail-heavy-sun.svg';
	// 	}
	// }

	release() {
		this.httpProxy.release();
	}
}
