<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { TimeOfDay, type DailyForecast } from '$lib/models/forecast';
	import { WeatherCode } from '$lib/models/weatherCode';

	import Illustration from '$lib/components/Illustration.svelte';

	export let forecast: DailyForecast;

	let weatherDescription = '';
	let precipitationUnit = '';
	const weatherDescriptionTr = new Map<WeatherCode, string>();
	const precipitationUnitsTr = new Map<string, string>();

	let localStorage: LocalStorageUtil;

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

		precipitationUnitsTr.set('mm', $t('index.mm')).set('inches', $t('index.inches'));

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

		<div class="current-temp">{forecast.temperatureMax}° / {forecast.temperatureMin}°</div>

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
					<td>{hourForecast.timeString}</td>
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
				border-bottom: 1px solid #ddd;
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
</style>
