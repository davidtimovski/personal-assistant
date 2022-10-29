<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';

	import { DateHelper } from '../../../../shared2/utils/dateHelper';
	import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageKeys, LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { isOffline, authInfo, forecast, locale } from '$lib/stores';
	import { ForecastsService } from '$lib/services/forecastsService';
	import { WeatherCode } from '$lib/models/weatherCode';

	let imageUri: any;
	let currentTime = DateHelper.formatHoursMinutes(new Date(), $locale);
	let isNight = false;
	let windSpeedUnit: string;
	let precipitationUnit: string;
	const weatherDescriptionTr = new Map<WeatherCode, string>();
	const windSpeedUnitsTr = new Map<string, string>();
	const precipitationUnitsTr = new Map<string, string>();
	const unsubscriptions: Unsubscriber[] = [];

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersServiceBase;
	let forecastsService: ForecastsService;

	function sync() {
		startProgressBar();

		currentTime = DateHelper.formatHoursMinutes(new Date(), $locale);
		forecastsService.get();

		usersService.getProfileImageUri().then((uri) => {
			if (imageUri !== uri) {
				imageUri = uri;
			}
		});
	}

	function startProgressBar() {
		progressBarActive = true;
		progress = 10;

		progressIntervalId = window.setInterval(() => {
			if (progress < 85) {
				progress += 15;
			} else if (progressIntervalId) {
				window.clearInterval(progressIntervalId);
			}
		}, 500);

		progressBarVisible = true;
	}

	function finishProgressBar() {
		window.setTimeout(() => {
			progress = 100;
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	function getWeatherImage(weatherCode: WeatherCode) {
		switch (weatherCode) {
			case WeatherCode.ClearSky:
				if (isNight) {
					return 'moon.svg';
				}
				return 'sun.svg';
			case WeatherCode.MainlyClear:
				if (isNight) {
					return 'moon.svg';
				}
				return 'sun.svg';
			case WeatherCode.PartlyCloudy:
				if (isNight) {
					return 'cloud-moon.svg';
				}
				return 'cloud-sun.svg';
			case WeatherCode.Overcast:
				return 'cloudy.svg';
			case WeatherCode.Fog:
			case WeatherCode.DepositingRimeFog:
				return 'fog.svg';
			case WeatherCode.DrizzleLight:
			case WeatherCode.DrizzleLightFreezing:
				if (isNight) {
					return 'drizzle-light-moon.svg';
				}
				return 'drizzle-light-sun.svg';
			case WeatherCode.DrizzleModerate:
			case WeatherCode.DrizzleDense:
			case WeatherCode.DrizzleDenseFreezing:
				if (isNight) {
					return 'drizzle-heavy-moon.svg';
				}
				return 'drizzle-heavy-sun.svg';
			case WeatherCode.RainLight:
				return 'rain-light.svg';
			case WeatherCode.RainModerate:
			case WeatherCode.RainHeavy:
				return 'rain-heavy.svg';
			case WeatherCode.SnowLight:
				return 'snow-light.svg';
			case WeatherCode.SnowModerate:
			case WeatherCode.SnowHeavy:
			case WeatherCode.SnowGrains:
				return 'snow-heavy.svg';
			case WeatherCode.ShowerLight:
				if (isNight) {
					return 'shower-light-moon.svg';
				}
				return 'shower-light-sun.svg';
			case WeatherCode.ShowerModerate:
			case WeatherCode.ShowerViolent:
				if (isNight) {
					return 'shower-heavy-moon.svg';
				}
				return 'shower-heavy-sun.svg';
			case WeatherCode.SnowShowerLight:
				if (isNight) {
					return 'snow-light-moon.svg';
				}
				return 'snow-light-sun.svg';
			case WeatherCode.SnowShowerHeavy:
				return 'snow-heavy.svg';
			case WeatherCode.Thunderstorm:
				return 'thunderstorm.svg';
			case WeatherCode.ThunderstormWithHailLight:
				if (isNight) {
					return 'hail-light-moon.svg';
				}
				return 'hail-light-sun.svg';
			case WeatherCode.ThunderstormWithHailHeavy:
				if (isNight) {
					return 'hail-heavy-moon.svg';
				}
				return 'hail-heavy-sun.svg';
		}
	}

	onMount(() => {
		weatherDescriptionTr
			.set(WeatherCode.ClearSky, $t('index.clear'))
			.set(WeatherCode.MainlyClear, $t('index.mainlyClear'))
			.set(WeatherCode.PartlyCloudy, $t('index.partlyCloudy'))
			.set(WeatherCode.Overcast, $t('index.overcast'))
			.set(WeatherCode.Fog, $t('index.foggy'))
			.set(WeatherCode.DepositingRimeFog, $t('index.foggy'))
			.set(WeatherCode.DrizzleLight, $t('index.lightDrizzle'))
			.set(WeatherCode.DrizzleModerate, $t('index.moderateDrizzle'))
			.set(WeatherCode.DrizzleDense, $t('index.denseDrizzle'))
			.set(WeatherCode.DrizzleLightFreezing, $t('index.lightFreezingDrizzle'))
			.set(WeatherCode.DrizzleDenseFreezing, $t('index.denseFreezingDrizzle'))
			.set(WeatherCode.RainLight, $t('index.lightRain'))
			.set(WeatherCode.RainModerate, $t('index.moderateRain'))
			.set(WeatherCode.RainHeavy, $t('index.heavyRain'))
			.set(WeatherCode.SnowLight, $t('index.lightSnow'))
			.set(WeatherCode.SnowModerate, $t('index.moderateSnow'))
			.set(WeatherCode.SnowHeavy, $t('index.heavySnow'))
			.set(WeatherCode.SnowGrains, $t('index.snowGrains'))
			.set(WeatherCode.ShowerLight, $t('index.lightShower'))
			.set(WeatherCode.ShowerModerate, $t('index.moderateShower'))
			.set(WeatherCode.ShowerViolent, $t('index.violentShower'))
			.set(WeatherCode.SnowShowerLight, $t('index.lightSnowShower'))
			.set(WeatherCode.SnowShowerHeavy, $t('index.heavySnowShower'))
			.set(WeatherCode.Thunderstorm, $t('index.thunderstorm'))
			.set(WeatherCode.ThunderstormWithHailLight, $t('index.thunderstormWithLightHail'))
			.set(WeatherCode.ThunderstormWithHailHeavy, $t('index.thunderstormWithHeavyHail'));

		windSpeedUnitsTr.set('kmh', $t('index.kmh')).set('mph', $t('index.mph'));

		precipitationUnitsTr.set('mm', $t('index.mm')).set('inches', $t('index.inches'));

		localStorage = new LocalStorageUtil();
		usersService = new UsersServiceBase('Weatherman');
		forecastsService = new ForecastsService();

		imageUri = localStorage.get('profileImageUri');
		windSpeedUnit = <string>windSpeedUnitsTr.get(localStorage.get(LocalStorageKeys.WindSpeedUnit));
		precipitationUnit = <string>precipitationUnitsTr.get(localStorage.get(LocalStorageKeys.PrecipitationUnit));

		unsubscriptions.push(
			authInfo.subscribe((value) => {
				if (!value) {
					return;
				}

				if (usersService.profileImageUriIsStale()) {
					usersService.getProfileImageUri().then((uri) => {
						imageUri = uri;
					});
				}
			})
		);

		if ($forecast === null) {
			startProgressBar();
		}

		unsubscriptions.push(
			forecast.subscribe((x) => {
				if (!x) {
					return;
				}

				finishProgressBar();
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		usersService?.release();
		forecastsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			<a href="/menu" class="profile-image-container" title={$t('index.menu')} aria-label={$t('index.menu')}>
				<img src={imageUri} class="profile-image" width="40" height="40" alt="" />
			</a>

			<div class="page-title">{currentTime}</div>
			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={$isOffline || progressBarActive}
				title={$t('index.refresh')}
				aria-label={$t('index.refresh')}
			>
				<i class="fas fa-sync-alt" />
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {progress}%;" />
		</div>
	</div>

	<div class="content-wrap">
		{#if $forecast !== null}
			<div class="wrap">
				<div class="current-forecast">
					<div
						style="background-image: url({'/images/weather/' + getWeatherImage($forecast.weatherCode)});"
						class="current-weather-illustration"
						alt=""
					/>
					<div class="current-temp">{$forecast.temperature}°</div>
					<div class="current-weather">{weatherDescriptionTr.get($forecast.weatherCode)}</div>

					<table class="wind-and-precipitation">
						<tr class="wind">
							<td><i class="fa-solid fa-wind" /></td>
							<td>
								<span class="current-value">{$forecast.windSpeed}</span>
								<span>{windSpeedUnit}</span>
							</td>
						</tr>
						<tr class="precipitation">
							<td><i class="fa-solid fa-droplet" /></td>
							<td>
								<span class="current-value">{$forecast.precipitation}</span>
								<span>{precipitationUnit}</span>
							</td>
						</tr>
					</table>
				</div>

				<div class="hourly-forecast">
					<table>
						{#each $forecast.hourly as hourForecast}
							<tr>
								<td>{hourForecast.time}</td>
								<td>{hourForecast.temperature}°</td>
							</tr>
						{/each}
					</table>
				</div>
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.page-title {
		font-size: 22px;
		color: #774022;
	}

	.wrap {
		display: flex;
	}

	.current-forecast {
		flex: 1;
		padding: 0 10px;
	}

	.current-weather-illustration {
		width: 100%;
		height: 80px;
		background-repeat: no-repeat;
		background-size: cover;
		background-position: 0;
	}

	.current-temp {
		padding: 10px 0 35px;
		font-size: 90px;
		line-height: 70px;
		text-align: center;
	}

	.current-weather {
		margin-bottom: 20px;
		font-size: 1.4rem;
		line-height: 1.8rem;
	}

	.wind-and-precipitation {
		td {
			padding-bottom: 6px;
		}

		i {
			margin-right: 7px;
			font-size: 1.2rem;
		}

		.current-value {
			font-size: 1.5rem;
		}

		.wind i {
			color: #999;
		}
		.precipitation i {
			color: #5aacf1;
		}
	}

	.hourly-forecast {
		flex: 1;

		table {
			width: 100%;

			td {
				border-bottom: 1px solid #ddd;
				padding: 7px 10px;
				font-size: 18px;
			}
		}
	}

	@media screen and (min-width: 1200px) {
		.current-weather-illustration {
			height: 130px;
			background-position: 0;
		}
	}
</style>
