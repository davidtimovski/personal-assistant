<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { PageData } from './$types';
	import { page } from '$app/stores';

	import { t } from '$lib/localization/i18n';
	import { ExercisesService } from '$lib/services/exercisesService';
	import { ProgressService } from '$lib/services/progressService';
	import { ExerciseType, type Exercise } from '$lib/models/exercise';
	import type { EditProgress } from '$lib/models/editProgress';

	import EditProgressAmountForm from '$lib/components/EditProgressAmountForm.svelte';
	import EditProgressAmountX2Form from '$lib/components/EditProgressAmountX2Form.svelte';

	export let data: PageData;

	let exercisesService: ExercisesService;
	let progressService: ProgressService;

	let exercise: Exercise | null = null;
	let progress: EditProgress | null = null;

	onMount(async () => {
		const date = <string>$page.url.searchParams.get('date');

		exercisesService = new ExercisesService();
		progressService = new ProgressService();

		exercise = await exercisesService.get(data.id);
		progress = await progressService.get(data.id, date);
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
		<a href="/dashboard" class="back-button">
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
		{:else if exercise.ofType === ExerciseType.AmountX2}
			<EditProgressAmountX2Form {exercise} {progress} />
		{/if}
	</div>
</section>
