<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';

	import DoubleRadio from '$lib/components/DoubleRadio.svelte';

	let localStorage: LocalStorageUtil;

	let temperatureUnit: string | null = null;
	let precipitationUnit: string | null = null;
	let windSpeedUnit: string | null = null;

	function temperatureUnitChanged() {
		localStorage.set(LocalStorageKeys.TemperatureUnit, temperatureUnit);
	}

	function precipitationUnitChanged() {
		localStorage.set(LocalStorageKeys.PrecipitationUnit, precipitationUnit);
	}

	function windSpeedUnitChanged() {
		localStorage.set(LocalStorageKeys.WindSpeedUnit, windSpeedUnit);
	}

	onMount(() => {
		localStorage = new LocalStorageUtil();

		temperatureUnit = localStorage.get(LocalStorageKeys.TemperatureUnit);
		precipitationUnit = localStorage.get(LocalStorageKeys.PrecipitationUnit);
		windSpeedUnit = localStorage.get(LocalStorageKeys.WindSpeedUnit);
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-sliders-h" />
		</div>
		<div class="page-title">{$t('preferences.preferences')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form>
			<div class="form-control-group">
				<div class="setting-descriptor">{$t('preferences.temperatureUnit')}</div>
				<DoubleRadio
					name="temperatureUnitToggle"
					leftLabelKey="preferences.celsius"
					rightLabelKey="preferences.fahrenheit"
					leftValue="celsius"
					rightValue="fahrenheit"
					bind:value={temperatureUnit}
					on:change={temperatureUnitChanged}
				/>
			</div>

			<div class="form-control-group">
				<div class="setting-descriptor">{$t('preferences.precipitationUnit')}</div>
				<DoubleRadio
					name="precipitationUnitToggle"
					leftLabelKey="preferences.millimeters"
					rightLabelKey="preferences.inches"
					leftValue="mm"
					rightValue="inch"
					bind:value={precipitationUnit}
					on:change={precipitationUnitChanged}
				/>
			</div>

			<div class="form-control-group">
				<div class="setting-descriptor">{$t('preferences.windSpeedUnit')}</div>
				<DoubleRadio
					name="windSpeedUnitToggle"
					leftLabelKey="preferences.kmh"
					rightLabelKey="preferences.mph"
					leftValue="kmh"
					rightValue="mph"
					bind:value={windSpeedUnit}
					on:change={windSpeedUnitChanged}
				/>
			</div>
		</form>
	</div>
</section>
