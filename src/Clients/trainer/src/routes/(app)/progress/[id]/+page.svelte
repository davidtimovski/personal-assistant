<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { PageData } from './$types';
	import { page } from '$app/stores';

	import { t } from '$lib/localization/i18n';
	import { ExercisesService } from '$lib/services/exercisesService';
	import { ProgressService } from '$lib/services/progressService';
	import { ExerciseType, type Exercise } from '$lib/models/exercise';
	import type { EditProgress } from '$lib/models/editProgress';

	import EditProgressAmountForm from '$lib/components/EditProgressAmountForm.svelte';

	export let data: PageData;

	let exercisesService: ExercisesService;
	let progressService: ProgressService;

	let exercise: Exercise | null = null;
	let progress: EditProgress | null = null;

	onMount(async () => {
		const exerciseId = parseInt(<string>$page.url.searchParams.get('exerciseId'), 10);

		exercisesService = new ExercisesService();
		progressService = new ProgressService();

		exercise = await exercisesService.get(exerciseId);
		progress = await progressService.get(exerciseId, data.date);
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
			<!-- <span>{$t(isNew ? 'editUpcomingExpense.newUpcomingExpense' : 'editUpcomingExpense.editUpcomingExpense')}</span> -->
		</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !exercise || !progress}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else if exercise.ofType === ExerciseType.Amount}
			<EditProgressAmountForm {exercise} {progress} />
		{/if}
	</div>
</section>
