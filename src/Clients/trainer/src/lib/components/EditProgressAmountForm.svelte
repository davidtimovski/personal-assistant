<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../shared2/utils/dateHelper';
	import { ValidationResult } from '../../../../shared2/utils/validationUtils';

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
		goto(`/progress/${progressModel.date}?exerciseId=${exercise.id}`);

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

	{#each progressModel.sets as set}
		<div>
			<span>Set {set.set}</span>

			<div>
				<labeL for="amount">{exerciseModel.amountUnit}</labeL>
				<input type="number" id="amount" bind:value={set.amount} min="0" />
			</div>
		</div>
	{/each}
</form>

<hr />

<div class="save-delete-wrap">
	<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
		<span class="button-loader" class:loading={saveButtonIsLoading}>
			<i class="fas fa-circle-notch fa-spin" />
		</span>
		<span>Save</span>
	</button>

	<a href="/" class="button secondary-button">{$t('cancel')}</a>
</div>

<style lang="scss">
</style>
