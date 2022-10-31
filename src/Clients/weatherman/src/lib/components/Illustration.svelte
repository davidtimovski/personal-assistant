<script lang="ts">
	import { WeatherCode } from '$lib/models/weatherCode';

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
	import Thunderstorm from '$lib/components/illustrations/Thunderstorm.svelte';

	export let weatherCode: WeatherCode;
	export let isNight: boolean;
</script>

{#if weatherCode === WeatherCode.ClearSky || weatherCode === WeatherCode.MainlyClear}
	{#if isNight}
		<Moon />
	{:else}
		<Sun />
	{/if}
{:else if weatherCode === WeatherCode.PartlyCloudy}
	{#if isNight}
		<CloudMoon />
	{:else}
		<CloudSun />
	{/if}
{:else if weatherCode === WeatherCode.Overcast}
	<Cloudy />
{:else if weatherCode === WeatherCode.Fog || weatherCode === WeatherCode.DepositingRimeFog}
	<Fog />
{:else if weatherCode === WeatherCode.DrizzleLight || weatherCode === WeatherCode.DrizzleLightFreezing}
	{#if isNight}
		<DrizzleLightMoon />
	{:else}
		<DrizzleLightSun />
	{/if}
{:else if weatherCode === WeatherCode.DrizzleModerate || weatherCode === WeatherCode.DrizzleDense || weatherCode === WeatherCode.DrizzleDenseFreezing}
	{#if isNight}
		<DrizzleHeavyMoon />
	{:else}
		<DrizzleHeavySun />
	{/if}
{:else if weatherCode === WeatherCode.RainLight}
	<RainLight />
{:else if weatherCode === WeatherCode.RainModerate || weatherCode === WeatherCode.RainHeavy}
	<RainHeavy />
{:else if weatherCode === WeatherCode.SnowLight}
	{#if isNight}
		<SnowLightMoon />
	{:else}
		<SnowLightSun />
	{/if}
{:else if weatherCode === WeatherCode.SnowModerate || weatherCode === WeatherCode.SnowHeavy || weatherCode === WeatherCode.SnowGrains}
	<SnowHeavy />
{:else if weatherCode === WeatherCode.ShowerLight}
	{#if isNight}
		<ShowerLightMoon />
	{:else}
		<ShowerLightSun />
	{/if}
{:else if weatherCode === WeatherCode.ShowerModerate || weatherCode === WeatherCode.ShowerViolent}
	{#if isNight}
		<ShowerHeavyMoon />
	{:else}
		<ShowerHeavySun />
	{/if}
{:else if weatherCode === WeatherCode.SnowShowerLight}
	{#if isNight}
		<SnowLightMoon />
	{:else}
		<SnowLightSun />
	{/if}
{:else if weatherCode === WeatherCode.SnowShowerHeavy}
	<SnowHeavy />
{:else if weatherCode === WeatherCode.Thunderstorm}
	<Thunderstorm />
{:else if weatherCode === WeatherCode.ThunderstormWithHailLight}
	{#if isNight}
		<HailLightMoon />
	{:else}
		<HailLightSun />
	{/if}
{:else if weatherCode === WeatherCode.ThunderstormWithHailHeavy}
	<HailHeavy />
{/if}
