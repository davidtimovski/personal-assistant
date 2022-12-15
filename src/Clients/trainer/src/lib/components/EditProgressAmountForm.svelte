<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';

	import { ValidationResult } from '../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { ProgressService } from '$lib/services/progressService';
	import { EditAmountProgressEntry, AmountSets } from '$lib/models/editProgressEntry';

	export let id: number;
	export let exerciseId: number;
	export let sets: number;
	export let amountUnit: string | null = null;

	let date: string;
	let entries = new Array<AmountSets>();
	let saveButtonIsLoading = false;

	for (let i = 1; i <= sets; i++) {
		entries.push(new AmountSets(i, 0));
	}

	let progressService: ProgressService;

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
			const createModel = new EditAmountProgressEntry(id, exerciseId, date, entries);
			await progressService.createAmount(createModel);
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
	{#each entries as entry}
		<div>
			<input type="number" bind:value={entry.set} readonly />
			<input type="number" bind:value={entry.amount} min="0" />
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
