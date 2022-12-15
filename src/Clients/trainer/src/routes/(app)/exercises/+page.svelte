<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import EmptyListMessage from '../../../../../shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { ExercisesService } from '$lib/services/exercisesService';
	import type { Exercise } from '$lib/models/exercise';

	let exercises: Exercise[] | null = null;
	let editedId: number | undefined;

	let exercisesService: ExercisesService;

	onMount(async () => {
		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		exercisesService = new ExercisesService();

		exercises = await exercisesService.getAll();
	});

	onDestroy(() => {
		exercisesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-inbox" />
		</div>
		<div class="page-title">{$t('exercises.exercises')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !exercises}
				<div class="double-circle-loading">
					<div class="double-bounce1" />
					<div class="double-bounce2" />
				</div>
			{:else if exercises.length > 0}
				<div class="exercises-container">
					{#each exercises as exercise}
						<a href="/editExercise/{exercise.id}" class="exercise" class:highlighted={editedId === exercise.id}
							>{exercise.name}</a
						>
					{/each}
				</div>
			{:else}
				<EmptyListMessage messageKey="exercises.emptyListMessage" />
			{/if}
		</div>

		<div class="centering-wrap">
			<button
				type="button"
				on:click={() => goto('/editExercise/0')}
				class="new-button"
				title={$t('exercises.newExercise')}
				aria-label={$t('exercises.newExercise')}
			>
				<i class="fas fa-plus" />
			</button>
		</div>
	</div>
</section>

<style lang="scss">
	.exercises-container {
		display: grid;
		grid-template-columns: 1fr 1fr;
		column-gap: 15px;
		row-gap: 10px;
	}

	.exercise {
		border-radius: 6px;
		padding: 15px;
		text-align: center;
		text-decoration: none;

		&.highlighted {
			animation: highlight ease-in 2.5s;
		}
		@keyframes highlight {
			0% {
				background: #8c8;
			}
		}
	}

	//.category-wrap {
	// 	display: flex;
	// }

	// .subcategory-wrap {
	// 	display: flex;
	// 	padding-left: 30px;
	// }

	// .category {
	// 	display: flex;
	// 	justify-content: space-between;
	// 	width: 100%;
	// 	background: #f4f1f8;
	// 	border-radius: var(--border-radius);
	// 	margin-bottom: 10px;
	// 	padding: 9px 15px;
	// 	color: var(--regular-color);
	// 	line-height: 27px;
	// 	text-decoration: none;
	// 	user-select: none;

	// 	&:hover {
	// 		color: var(--primary-color-dark);
	// 	}

	// 	.name {
	// 		padding-right: 15px;
	// 	}

	// 	&:last-child {
	// 		margin-bottom: 0;
	// 	}
	// }

	// .category-indicator {
	// 	margin-left: 15px;
	// 	font-size: 1.2rem;
	// 	color: var(--faded-color);
	// }

	// .sync {
	// 	line-height: 45px;
	// 	font-size: 1.2rem;
	// 	color: var(--faded-color);

	// 	i {
	// 		margin-left: 15px;
	// 	}
	// }

	@media (prefers-color-scheme: light) {
		.exercise {
			background: #eee;
		}
	}

	@media (prefers-color-scheme: dark) {
		.exercise {
			background: #555;
			color: #fff;
		}
	}
</style>
