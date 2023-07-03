<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';
	import { page } from '$app/stores';

	import { t } from '$lib/localization/i18n';
	import { ExercisesService } from '$lib/services/exercisesService';
	import { Exercise, ExerciseType } from '$lib/models/exercise';

	import EditExerciseAmountForm from '$lib/components/EditExerciseAmountForm.svelte';
	import EditExerciseAmountX2Form from '$lib/components/EditExerciseAmountX2Form.svelte';

	export let data: PageData;

	const isNew = data.id === 0;

	let model: Exercise;
	let exerciseType: ExerciseType | null = null;
	let name = '';

	let exercisesService: ExercisesService;

	async function newExercise(type: ExerciseType) {
		model = new Exercise(0, '', 1, type);
		exerciseType = type;
		await goto(`/editExercise/${data.id}?type=${type}`);
	}

	async function cancel() {
		if (isNew) {
			exerciseType = null;
			await goto(`/editExercise/${data.id}`);
		} else {
			await goto('/exercises');
		}
	}

	onMount(async () => {
		const type = $page.url.searchParams.get('type');
		if (type) {
			exerciseType = parseInt(type, 10);
		}

		exercisesService = new ExercisesService();

		if (!isNew) {
			model = await exercisesService.get(data.id);
			name = model.name;
			exerciseType = model.ofType;
		}
	});

	onDestroy(() => {
		exercisesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			{#if isNew}
				<span>{$t('editExercise.newExercise')}</span>
			{:else}
				<span>{$t('editExercise.edit')}</span>&nbsp;<span class="colored-text">{name}</span>
			{/if}
		</div>
		<a href="/exercises" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if isNew && exerciseType === null}
			<div>
				<button on:click={async () => await newExercise(ExerciseType.Amount)}>Amount</button>
				<button on:click={async () => await newExercise(ExerciseType.AmountX2)}>Amount X2</button>
			</div>
		{/if}

		{#if exerciseType !== null}
			{#if exerciseType === ExerciseType.Amount}
				<EditExerciseAmountForm {model} on:cancel={cancel} />
			{:else if exerciseType === ExerciseType.AmountX2}
				<EditExerciseAmountX2Form {model} on:cancel={cancel} />
			{/if}
		{/if}
	</div>
</section>
