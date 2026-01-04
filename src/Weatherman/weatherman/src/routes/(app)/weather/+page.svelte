<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { tweened } from 'svelte/motion';
	import { cubicOut } from 'svelte/easing';

	import { DateHelper } from '../../../../../../Core/shared2/utils/dateHelper';
	import { UsersServiceBase } from '../../../../../../Core/shared2/services/usersServiceBase';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageKeys, LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { isOffline, user, forecast } from '$lib/stores';
	import { ForecastsService } from '$lib/services/forecastsService';
	import { WeatherCode } from '$lib/models/weatherCode';

	import Illustration from '$lib/components/Illustration.svelte';
	import NextDayForecast from '$lib/components/NextDayForecast.svelte';

	let now = new Date();
	let selectedDate = $state('');
	let currentTime = $state('');
	let currentDate = $state(selectedDate);
	let weatherDescription = $state('');
	let windSpeedUnit = $state('');
	let precipitationUnit = $state('');
	const weatherDescriptionTr = new Map<WeatherCode, string>();
	const windSpeedUnitsTr = new Map<string, string>();
	const precipitationUnitsTr = new Map<string, string>();
	const unsubscriptions: Unsubscriber[] = [];

	let currentTempColor = $derived($forecast.temperature !== null ? ForecastsService.getTempColor(<number>$forecast.temperature) : 'inherit');

	// Progress bar
	let progressBarActive = $state(false);
	const progress = tweened(0, {
		duration: 500,
		easing: cubicOut
	});
	let progressIntervalId: number | undefined;
	let progressBarVisible = $state(false);

	let localStorage: LocalStorageUtil;
	let usersService: UsersServiceBase;
	let forecastsService: ForecastsService;

	function setCurrentTime() {
		now = new Date();
		selectedDate = DateHelper.format(now);
		currentTime = DateHelper.formatHoursMinutes(now, $user.culture);
		currentDate = selectedDate;
	}

	function sync() {
		startProgressBar();

		setCurrentTime();
		forecastsService.get(now, $user.language, $user.culture);
	}

	function startProgressBar() {
		progressBarActive = true;
		progress.set(10);

		progressIntervalId = window.setInterval(() => {
			if ($progress < 85) {
				progress.update((x) => {
					x += 15;
					return x;
				});
			} else if (progressIntervalId) {
				window.clearInterval(progressIntervalId);
			}
		}, 500);

		progressBarVisible = true;
	}

	function finishProgressBar() {
		window.clearInterval(progressIntervalId);
		window.setTimeout(() => {
			progress.set(100);
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	function selectDate(date: string) {
		selectedDate = date;
	}

	onMount(() => {
		weatherDescriptionTr
			.set(WeatherCode.ClearSky, $t('weather.clear'))
			.set(WeatherCode.MainlyClear, $t('weather.mainlyClear'))
			.set(WeatherCode.PartlyCloudy, $t('weather.partlyCloudy'))
			.set(WeatherCode.Overcast, $t('weather.overcast'))
			.set(WeatherCode.Fog, $t('weather.foggy'))
			.set(WeatherCode.DepositingRimeFog, $t('weather.foggy'))
			.set(WeatherCode.DrizzleLight, $t('weather.lightDrizzle'))
			.set(WeatherCode.DrizzleModerate, $t('weather.moderateDrizzle'))
			.set(WeatherCode.DrizzleDense, $t('weather.denseDrizzle'))
			.set(WeatherCode.DrizzleLightFreezing, $t('weather.lightFreezingDrizzle'))
			.set(WeatherCode.DrizzleDenseFreezing, $t('weather.denseFreezingDrizzle'))
			.set(WeatherCode.RainLight, $t('weather.lightRain'))
			.set(WeatherCode.RainModerate, $t('weather.moderateRain'))
			.set(WeatherCode.RainHeavy, $t('weather.heavyRain'))
			.set(WeatherCode.FreezingRainLight, $t('weather.lightFreezingRain'))
			.set(WeatherCode.FreezingRainHeavy, $t('weather.heavyFreezingRain'))
			.set(WeatherCode.SnowLight, $t('weather.lightSnow'))
			.set(WeatherCode.SnowModerate, $t('weather.moderateSnow'))
			.set(WeatherCode.SnowHeavy, $t('weather.heavySnow'))
			.set(WeatherCode.SnowGrains, $t('weather.snowGrains'))
			.set(WeatherCode.ShowerLight, $t('weather.lightShower'))
			.set(WeatherCode.ShowerModerate, $t('weather.moderateShower'))
			.set(WeatherCode.ShowerViolent, $t('weather.violentShower'))
			.set(WeatherCode.SnowShowerLight, $t('weather.lightSnowShower'))
			.set(WeatherCode.SnowShowerHeavy, $t('weather.heavySnowShower'))
			.set(WeatherCode.Thunderstorm, $t('weather.thunderstorm'))
			.set(WeatherCode.ThunderstormWithHailLight, $t('weather.thunderstormWithLightHail'))
			.set(WeatherCode.ThunderstormWithHailHeavy, $t('weather.thunderstormWithHeavyHail'));

		windSpeedUnitsTr.set('kmh', $t('weather.kmh')).set('mph', $t('weather.mph'));

		precipitationUnitsTr.set('mm', $t('weather.mm')).set('inches', $t('weather.inches'));

		localStorage = new LocalStorageUtil();
		usersService = new UsersServiceBase('Weatherman');
		forecastsService = new ForecastsService();

		windSpeedUnit = <string>windSpeedUnitsTr.get(localStorage.get(LocalStorageKeys.WindSpeedUnit));
		precipitationUnit = <string>precipitationUnitsTr.get(localStorage.get(LocalStorageKeys.PrecipitationUnit));

		if ($forecast === null) {
			startProgressBar();
		}

		unsubscriptions.push(
			user.subscribe((x) => {
				if (!x.email) {
					return;
				}

				setCurrentTime();
			})
		);

		unsubscriptions.push(
			forecast.subscribe((x) => {
				if (!x.initialized) {
					return;
				}

				weatherDescription = <string>weatherDescriptionTr.get(<WeatherCode>x.weatherCode);

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
			<a href="/menu" class="profile-image-container" title={$t('weather.menu')} aria-label={$t('weather.menu')}>
				<img src={$user.imageUri} class="profile-image" width="40" height="40" alt="" />
			</a>

			<div class="page-title">{currentTime}</div>
			<button
				type="button"
				onclick={sync}
				class="sync-button"
				disabled={$isOffline || progressBarActive}
				title={$t('weather.refresh')}
				aria-label={$t('weather.refresh')}
			>
				<i class="fas fa-sync-alt"></i>
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {$progress}%;"></div>
		</div>
	</div>

	<div class="content-wrap">
		<div class="days">
			{#each $forecast.weekDays as weekDay}
				<button type="button" onclick={() => selectDate(weekDay.date)} class="week-day" class:selected={selectedDate === weekDay.date}
					>{weekDay.weekDay}</button
				>
			{/each}
		</div>

		{#if $forecast !== null}
			<div class="tab" class:visible={selectedDate === currentDate}>
				<div class="wrap">
					<div class="current-forecast">
						{#if $forecast.initialized}
							<div class="current-illustration">
								<Illustration weatherCode={$forecast.weatherCode} timeOfDay={$forecast.timeOfDay} />
							</div>

							<div class="current-temp" style="color: {currentTempColor}">{$forecast.temperature}°</div>

							{#if $forecast.temperature !== $forecast.apparentTemperature}
								<div class="current-apparent-temp">{$t('weather.feelsLike')} {$forecast.apparentTemperature}°</div>
							{/if}

							<div class="weather-description">{weatherDescription}</div>

							<table class="wind-and-precipitation">
								<tbody>
									<tr class="wind">
										<td><i class="fa-solid fa-wind"></i></td>
										<td>
											<span class="current-value">{$forecast.windSpeed}</span>
											<span>{windSpeedUnit}</span>
										</td>
									</tr>
									<tr class="precipitation">
										<td><i class="fa-solid fa-droplet"></i></td>
										<td>
											<span class="current-value">{$forecast.precipitation}</span>
											<span>{precipitationUnit}</span>
										</td>
									</tr>
								</tbody>
							</table>
						{:else if $forecast.hourly.length > 0}
							<div class="loader-wrap">
								<div class="double-circle-loading">
									<div class="double-bounce1"></div>
									<div class="double-bounce2"></div>
								</div>
							</div>
						{/if}
					</div>

					<div class="gutter"></div>

					<div class="hourly-forecast">
						<table>
							<tbody>
								{#each $forecast.hourly as hourForecast}
									<tr>
										<td>{hourForecast.timeString} {$t('h')}</td>
										<td class="hourly-illustration">
											{#if hourForecast.timeOfDay !== null}
												<Illustration weatherCode={hourForecast.weatherCode} timeOfDay={hourForecast.timeOfDay} />
											{/if}
										</td>
										<td>
											{#if hourForecast.temperature !== null}
												<span>{hourForecast.temperature}°</span>
											{/if}
										</td>
									</tr>
								{/each}
							</tbody>
						</table>
					</div>
				</div>
			</div>

			{#each $forecast.nextDays as nextDay}
				<div class="tab" class:visible={nextDay.date === selectedDate}>
					<NextDayForecast forecast={nextDay} />
				</div>
			{/each}
		{/if}
	</div>
</section>

<style lang="scss">
	.page-title {
		font-size: 22px;
	}

	.days {
		display: flex;
		gap: 8px;
		margin-bottom: 30px;

		.week-day {
			flex: 1;
			border: none;
			border-radius: 4px;
			outline: none;
			padding: 4px 6px;
			transition: background var(--transition);

			&.selected {
				background: var(--primary-color);
				color: #fff;
			}
		}
	}

	.wrap {
		display: flex;
	}

	.current-forecast {
		flex: 1;

		.loader-wrap {
			padding-top: 60px;
		}
	}

	.current-illustration {
		padding: 0 20px;
	}

	.current-temp {
		padding: 10px 0 0 5px;
		font-size: 90px;
		line-height: 100px;
		text-align: center;
	}
	.current-apparent-temp {
		padding: 10px 0;
		font-size: 1.1rem;
		line-height: 1.3rem;
		text-align: center;
	}

	.weather-description {
		margin: 25px 0 20px;
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

	.gutter {
		flex: 0.1;
	}

	.hourly-forecast {
		flex: 1;

		table {
			width: 100%;

			td {
				border-bottom: 1px solid;
				padding: 4px 5px;
				font-size: 18px;
				white-space: nowrap;

				&:nth-child(2) {
					text-align: center;
				}

				&:nth-child(3) {
					text-align: right;
				}

				&.hourly-illustration {
					padding: 1px 5px;
				}
			}

			tr:last-child td {
				border-bottom: none;
			}
		}
	}

	.tab {
		display: none;

		&.visible {
			display: block;
		}
	}

	@media screen and (min-width: 800px) {
		.days .week-day {
			padding: 6px;
		}
	}

	@media (prefers-color-scheme: light) {
		.page-title {
			color: #774022;
		}

		.days .week-day {
			background: #eee;
		}

		.hourly-forecast table td {
			border-color: #ddd;
		}
	}

	@media (prefers-color-scheme: dark) {
		.page-title {
			color: #bf7421;
		}

		.days .week-day {
			background: #444;
			color: #fff;
		}

		.hourly-forecast table td {
			border-color: #777;
		}
	}
</style>
