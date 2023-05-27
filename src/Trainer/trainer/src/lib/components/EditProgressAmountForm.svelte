<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../../Core/shared2/utils/dateHelper';
	import { ValidationResult } from '../../../../../Core/shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { alertState } from '$lib/stores';
	import { ProgressService } from '$lib/services/progressService';
	import type { EditAmountProgress, EditProgress } from '$lib/models/editProgress';
	import type { Exercise, ExerciseAmount } from '$lib/models/exercise';

	export let exercise: Exercise;
	export let progress: EditProgress;

	let exerciseModel = <ExerciseAmount>exercise;
	let progressModel = <EditAmountProgress>progress;
	const maxDate = DateHelper.format(new Date());
	let saveButtonIsLoading = false;

	let progressService: ProgressService;

	$: canSave = true;

	async function dateChanged() {
		goto(`/progress/${exercise.id}?date=${progressModel.date}`);

		progressModel = <EditAmountProgress>await progressService.get(exercise.id, progressModel.date);
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
			await progressService.createAmount(progressModel);

			alertState.update((x) => {
				x.showSuccess('progress.saveSuccessful');
				return x;
			});
			goto('/dashboard');
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
				<th>{exerciseModel.amountUnit}</th>
			</tr>
		</thead>

		<tbody>
			{#each progressModel.sets as set}
				<tr>
					<td>{set.set}</td>
					<td><input type="number" id="amount" bind:value={set.amount} min="0" /></td>
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

	<a href="/dashboard" class="button secondary-button">{$t('back')}</a>
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
