<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import { createEventDispatcher } from 'svelte';

	import { ValidationResult } from '../../../../../Core/shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { alertState } from '$lib/stores';
	import { ExercisesService } from '$lib/services/exercisesService';
	import { CreateAmountX2Exercise, UpdateAmountX2Exercise } from '$lib/models/editExercise';
	import type { Exercise, ExerciseAmountX2 } from '$lib/models/exercise';

	export let model: Exercise;

	let amountModel = <ExerciseAmountX2>model;
	let nameInput: HTMLInputElement;
	let saveButtonText: string;
	let saveButtonIsLoading = false;

	const dispatch = createEventDispatcher();

	let exercisesService: ExercisesService;

	$: canSave = true;

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
			if (amountModel.id === 0) {
				const newId = await exercisesService.createAmountX2(
					new CreateAmountX2Exercise(amountModel.name, amountModel.sets, amountModel.amount1Unit, amountModel.amount2Unit)
				);

				alertState.update((x) => {
					x.showSuccess('editExercise.createSuccessful');
					return x;
				});
				goto('/exercises?edited=' + newId);
			} else {
				await exercisesService.updateAmountX2(
					new UpdateAmountX2Exercise(amountModel.id, amountModel.name, amountModel.sets, amountModel.amount1Unit, amountModel.amount2Unit)
				);

				alertState.update((x) => {
					x.showSuccess('editExercise.saveSuccessful');
					return x;
				});
				goto('/exercises?edited=' + model.id);
			}
		}
	}

	function cancel() {
		dispatch('cancel');
	}

	onMount(async () => {
		exercisesService = new ExercisesService();

		if (model.id === 0) {
			saveButtonText = $t('create');

			nameInput.focus();
		} else {
			saveButtonText = $t('save');
		}
	});

	onDestroy(() => {
		exercisesService?.release();
	});
</script>

<form on:submit|preventDefault={save}>
	<div class="form-control">
		<input
			type="text"
			id="name"
			bind:this={nameInput}
			bind:value={amountModel.name}
			maxlength="50"
			placeholder={$t('name')}
			aria-label={$t('name')}
		/>
	</div>

	<div class="form-control inline">
		<label for="sets">{$t('sets')}</label>
		<select id="sets" bind:value={amountModel.sets}>
			<option value={1}>1</option>
			<option value={2}>2</option>
			<option value={3}>3</option>
			<option value={4}>4</option>
			<option value={5}>5</option>
			<option value={6}>6</option>
			<option value={7}>7</option>
			<option value={8}>8</option>
			<option value={9}>9</option>
			<option value={10}>10</option>
		</select>
	</div>

	<div class="form-control inline">
		<label for="amount-unit1">{$t('editExercise.amount1Unit')}</label>
		<input type="text" id="amount-unit1" bind:value={amountModel.amount1Unit} maxlength="20" />
	</div>

	<div class="form-control inline">
		<label for="amount-unit2">{$t('editExercise.amount2Unit')}</label>
		<input type="text" id="amount-unit2" bind:value={amountModel.amount2Unit} maxlength="20" />
	</div>
</form>

<hr />

<div class="save-delete-wrap">
	<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
		<span class="button-loader" class:loading={saveButtonIsLoading}>
			<i class="fas fa-circle-notch fa-spin" />
		</span>
		<span>{saveButtonText}</span>
	</button>

	<button type="button" on:click={cancel} class="button secondary-button">{$t('cancel')}</button>
</div>

<style lang="scss">
</style>
