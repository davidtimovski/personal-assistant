<script lang="ts">
	import { WeatherCode } from '$lib/models/weatherCode';
	import { TimeOfDay } from '$lib/models/forecast';

	import CloudMoon from '$lib/components/illustrations/CloudMoon.svelte';
	import CloudSun from '$lib/components/illustrations/CloudSun.svelte';
	import Cloudy from '$lib/components/illustrations/Cloudy.svelte';
	import DrizzleHeavyMoon from '$lib/components/illustrations/DrizzleHeavyMoon.svelte';
	import DrizzleHeavySun from '$lib/components/illustrations/DrizzleHeavySun.svelte';
	import DrizzleLightMoon from '$lib/components/illustrations/DrizzleLightMoon.svelte';
	import DrizzleLightSun from '$lib/components/illustrations/DrizzleLightSun.svelte';
	import Fog from '$lib/components/illustrations/Fog.svelte';
	import HailHeavy from '$lib/components/illustrations/HailHeavy.svelte';
	import HailLightMoon from '$lib/components/illustrations/HailLightMoon.svelte';
	import HailLightSun from '$lib/components/illustrations/HailLightSun.svelte';
	import Moon from '$lib/components/illustrations/Moon.svelte';
	import RainHeavy from '$lib/components/illustrations/RainHeavy.svelte';
	import RainLight from '$lib/components/illustrations/RainLight.svelte';
	import ShowerHeavyMoon from '$lib/components/illustrations/ShowerHeavyMoon.svelte';
	import ShowerHeavySun from '$lib/components/illustrations/ShowerHeavySun.svelte';
	import ShowerLightMoon from '$lib/components/illustrations/ShowerLightMoon.svelte';
	import ShowerLightSun from '$lib/components/illustrations/ShowerLightSun.svelte';
	import SnowHeavy from '$lib/components/illustrations/SnowHeavy.svelte';
	import SnowLightMoon from '$lib/components/illustrations/SnowLightMoon.svelte';
	import SnowLightSun from '$lib/components/illustrations/SnowLightSun.svelte';
	import Sun from '$lib/components/illustrations/Sun.svelte';
	import SunLow from '$lib/components/illustrations/SunLow.svelte';
	import SunLower from '$lib/components/illustrations/SunLower.svelte';
	import Thunderstorm from '$lib/components/illustrations/Thunderstorm.svelte';

	let { weatherCode, timeOfDay }: { weatherCode: WeatherCode | null; timeOfDay: TimeOfDay | null } = $props();
</script>

{#if weatherCode === WeatherCode.ClearSky || weatherCode === WeatherCode.MainlyClear}
	{#if timeOfDay === TimeOfDay.Night}
		<Moon />
	{:else if timeOfDay === TimeOfDay.Day}
		<Sun />
	{:else if timeOfDay === TimeOfDay.SunLow}
		<SunLow />
	{:else if timeOfDay === TimeOfDay.SunLower}
		<SunLower />
	{/if}
{:else if weatherCode === WeatherCode.PartlyCloudy}
	{#if timeOfDay === TimeOfDay.Night}
		<CloudMoon />
	{:else}
		<CloudSun />
	{/if}
{:else if weatherCode === WeatherCode.Overcast}
	<Cloudy />
{:else if weatherCode === WeatherCode.Fog || weatherCode === WeatherCode.DepositingRimeFog}
	<Fog />
{:else if weatherCode === WeatherCode.DrizzleLight || weatherCode === WeatherCode.DrizzleLightFreezing}
	{#if timeOfDay !== TimeOfDay.Day}
		<DrizzleLightMoon />
	{:else}
		<DrizzleLightSun />
	{/if}
{:else if weatherCode === WeatherCode.DrizzleModerate || weatherCode === WeatherCode.DrizzleDense || weatherCode === WeatherCode.DrizzleDenseFreezing}
	{#if timeOfDay !== TimeOfDay.Day}
		<DrizzleHeavyMoon />
	{:else}
		<DrizzleHeavySun />
	{/if}
{:else if weatherCode === WeatherCode.RainLight || weatherCode === WeatherCode.FreezingRainLight}
	<RainLight />
{:else if weatherCode === WeatherCode.RainModerate || weatherCode === WeatherCode.RainHeavy || weatherCode === WeatherCode.FreezingRainHeavy}
	<RainHeavy />
{:else if weatherCode === WeatherCode.SnowLight}
	{#if timeOfDay !== TimeOfDay.Day}
		<SnowLightMoon />
	{:else}
		<SnowLightSun />
	{/if}
{:else if weatherCode === WeatherCode.SnowModerate || weatherCode === WeatherCode.SnowHeavy || weatherCode === WeatherCode.SnowGrains}
	<SnowHeavy />
{:else if weatherCode === WeatherCode.ShowerLight}
	{#if timeOfDay !== TimeOfDay.Day}
		<ShowerLightMoon />
	{:else}
		<ShowerLightSun />
	{/if}
{:else if weatherCode === WeatherCode.ShowerModerate || weatherCode === WeatherCode.ShowerViolent}
	{#if timeOfDay !== TimeOfDay.Day}
		<ShowerHeavyMoon />
	{:else}
		<ShowerHeavySun />
	{/if}
{:else if weatherCode === WeatherCode.SnowShowerLight}
	{#if timeOfDay !== TimeOfDay.Day}
		<SnowLightMoon />
	{:else}
		<SnowLightSun />
	{/if}
{:else if weatherCode === WeatherCode.SnowShowerHeavy}
	<SnowHeavy />
{:else if weatherCode === WeatherCode.Thunderstorm}
	<Thunderstorm />
{:else if weatherCode === WeatherCode.ThunderstormWithHailLight}
	{#if timeOfDay !== TimeOfDay.Day}
		<HailLightMoon />
	{:else}
		<HailLightSun />
	{/if}
{:else if weatherCode === WeatherCode.ThunderstormWithHailHeavy}
	<HailHeavy />
{/if}
