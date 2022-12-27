<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../shared2/utils/dateHelper';
	import { ValidationResult } from '../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { alertState } from '$lib/stores';
	import { ProgressService } from '$lib/services/progressService';
	import type { EditAmountX2Progress, EditProgress } from '$lib/models/editProgress';
	import type { Exercise, ExerciseAmountX2 } from '$lib/models/exercise';

	export let exercise: Exercise;
	export let progress: EditProgress;

	let exerciseModel = <ExerciseAmountX2>exercise;
	let progressModel = <EditAmountX2Progress>progress;
	const maxDate = DateHelper.format(new Date());
	let saveButtonIsLoading = false;

	let progressService: ProgressService;

	$: canSave = true;

	async function dateChanged() {
		goto(`/progress/${exercise.id}?date=${progressModel.date}`);

		progressModel = <EditAmountX2Progress>await progressService.get(exercise.id, progressModel.date);
	}

	function validate(): ValidationResult {
		const result = new ValidationResult();

		// if (!ValidationUtil.between(<number>amount, 0, amountTo)) {
		// 	result.fail('amount');
		// }

		return result;
	}

	async function save() {
		const result = validate();
		if (result.valid) {
			await progressService.createAmountX2(progressModel);

			alertState.update((x) => {
				x.showSuccess('progress.saveSuccessful');
				return x;
			});
			goto('/');
		}
	}

	onMount(() => {
		progressService = new ProgressService();
	});

	onDestroy(() => {
		progressService?.release();
	});
</script>

<form on:submit|preventDefault={save}>
	<div class="form-control inline">
		<label for="date">{$t('date')}</label>
		<input type="date" id="date" bind:value={progressModel.date} on:change={dateChanged} max={maxDate} required />
	</div>

	<hr class="date-separator" />

	<table>
		<thead>
			<tr>
				<th>Set</th>
				<th>{exerciseModel.amount1Unit}</th>
				<th>{exerciseModel.amount2Unit}</th>
			</tr>
		</thead>

		<tbody>
			{#each progressModel.sets as set}
				<tr>
					<td>{set.set}</td>
					<td><input type="number" bind:value={set.amount1} min="0" /></td>
					<td><input type="number" bind:value={set.amount2} min="0" /></td>
				</tr>
			{/each}
		</tbody>
	</table>
</form>

<hr />

<div class="save-delete-wrap">
	<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
		<span class="button-loader" class:loading={saveButtonIsLoading}>
			<i class="fas fa-circle-notch fa-spin" />
		</span>
		<span>{$t('save')}</span>
	</button>

	<a href="/" class="button secondary-button">{$t('back')}</a>
</div>

<style lang="scss">
	.date-separator {
		margin: 15px 0;
	}

	table {
		margin-top: 25px;
	}

	th,
	td {
		padding: 5px;
		text-align: center;
	}
</style>
