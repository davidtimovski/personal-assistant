<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { DateHelper } from '../../../../../Core/shared2/utils/dateHelper';
	import { user } from '$lib/stores';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';

	export let month: number | null;
	export let year: number | null;
	export let disabled: boolean;

	let monthOptions: SelectOption[] | null = null;
	let yearOptions: number[] | null = null;

	const now = new Date();
	let currentMonth = now.getMonth();
	let currentYear = now.getFullYear();

	function yearSet() {
		let startingMonthOption = 0;

		if (year === currentYear) {
			startingMonthOption = currentMonth;

			if (<number>month < currentMonth) {
				month = currentMonth;
			}
		}

		monthOptions = [];
		for (let i = startingMonthOption; i < 12; i++) {
			const month = new Date(1, i, 1);
			const option = new SelectOption(i, DateHelper.getLongMonth(month, $user.language));
			monthOptions.push(option);
		}
	}

	onMount(() => {
		if (month === null) {
			month = currentMonth;
		}
		if (year === null) {
			year = currentYear;
		}

		yearOptions = [currentYear, currentYear + 1];

		yearSet();
	});
</script>

<div class="month-selector">
	<select bind:value={month} {disabled} id="month-selector" class="month-select">
		{#if monthOptions}
			{#each monthOptions as monthOption}
				<option value={monthOption.id}>{monthOption.name}</option>
			{/each}
		{/if}
	</select>

	<select bind:value={year} on:change={yearSet} {disabled} class="year-select">
		{#if yearOptions}
			{#each yearOptions as yearOption}
				<option value={yearOption}>{yearOption}</option>
			{/each}
		{/if}
	</select>
</div>

<style lang="scss">
	.month-selector {
		display: flex;

		.month-select {
			width: 150px;
			margin-right: 10px;
		}

		.year-select {
			width: 100px;
		}
	}

	@media screen and (min-width: 1200px) {
		.month-selector {
			.month-select {
				width: 170px;
			}
			.year-select {
				width: 120px;
			}
		}
	}
</style>
