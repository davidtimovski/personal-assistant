<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { ForecastsService } from '$lib/services/forecastsService';
	import { TimeOfDay, type DailyForecast } from '$lib/models/forecast';
	import { WeatherCode } from '$lib/models/weatherCode';

	import Illustration from '$lib/components/Illustration.svelte';

	export let forecast: DailyForecast;

	let weatherDescription = '';
	let precipitationUnit = '';
	const weatherDescriptionTr = new Map<WeatherCode, string>();
	const precipitationUnitsTr = new Map<string, string>();

	$: maxTempColor =
		forecast.temperatureMax !== null ? ForecastsService.getTempColor(forecast.temperatureMax) : 'inherit';
	$: minTempColor =
		forecast.temperatureMax !== null ? ForecastsService.getTempColor(forecast.temperatureMin) : 'inherit';

	let localStorage: LocalStorageUtil;

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

		precipitationUnitsTr.set('mm', $t('weather.mm')).set('inches', $t('weather.inches'));

		localStorage = new LocalStorageUtil();

		precipitationUnit = <string>precipitationUnitsTr.get(localStorage.get(LocalStorageKeys.PrecipitationUnit));

		weatherDescription = <string>weatherDescriptionTr.get(forecast.weatherCode);
	});
</script>

<div class="wrap">
	<div class="current-forecast">
		<div class="current-illustration">
			<Illustration weatherCode={forecast.weatherCode} timeOfDay={TimeOfDay.Day} />
		</div>

		<div class="current-temp">
			<span style="color: {maxTempColor}">{forecast.temperatureMax}°</span> /
			<span style="color: {minTempColor}">{forecast.temperatureMin}°</span>
		</div>

		<div class="weather-description">{weatherDescription}</div>

		<table class="precipitation">
			<tr class="precipitation">
				<td><i class="fa-solid fa-droplet" /></td>
				<td>
					<span class="current-value">{forecast.precipitation}</span>
					<span>{precipitationUnit}</span>
				</td>
			</tr>
		</table>
	</div>

	<div class="gutter" />

	<div class="hourly-forecast">
		<table>
			{#each forecast.hourly as hourForecast}
				<tr>
					<td>{hourForecast.timeString} {$t('h')}</td>
					<td class="hourly-illustration">
						<Illustration weatherCode={hourForecast.weatherCode} timeOfDay={hourForecast.timeOfDay} />
					</td>
					<td>{hourForecast.temperature}°</td>
				</tr>
			{/each}
		</table>
	</div>
</div>

<style lang="scss">
	.wrap {
		display: flex;
	}

	.current-forecast {
		flex: 1;
	}

	.current-illustration {
		padding: 0 20px;
	}

	.current-temp {
		padding: 10px 0 0 5px;
		font-size: 38px;
		line-height: 48px;
		text-align: center;
	}

	.weather-description {
		margin: 25px 0 20px;
		font-size: 1.4rem;
		line-height: 1.8rem;
	}

	.precipitation {
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

	@media (prefers-color-scheme: light) {
		.hourly-forecast table td {
			border-color: #ddd;
		}
	}

	@media (prefers-color-scheme: dark) {
		.hourly-forecast table td {
			border-color: #777;
		}
	}
</style>
