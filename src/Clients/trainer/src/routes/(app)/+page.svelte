<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { tweened } from 'svelte/motion';
	import { cubicOut } from 'svelte/easing';

	import { t } from '$lib/localization/i18n';
	import { isOffline, user } from '$lib/stores';

	const exercises = [
		{
			id: 1,
			name: 'Running'
		}
	];

	// Progress bar
	let progressBarActive = false;
	const progress = tweened(0, {
		duration: 500,
		easing: cubicOut
	});
	let progressIntervalId: number | undefined;
	let progressBarVisible = false;

	function sync() {
		startProgressBar();
	}

	function startProgressBar() {
		progressBarActive = true;
		progress.set(10);

		progressIntervalId = window.setInterval(() => {
			if ($progress < 85) {
				progress.update((x) => {
					x += 15;
					return x;
				});
			} else if (progressIntervalId) {
				window.clearInterval(progressIntervalId);
			}
		}, 500);

		progressBarVisible = true;
	}

	function finishProgressBar() {
		window.clearInterval(progressIntervalId);
		window.setTimeout(() => {
			progress.set(100);
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	onMount(() => {
		// if ($forecast === null) {
		// 	startProgressBar();
		// }
	});

	onDestroy(() => {});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			<a href="/menu" class="profile-image-container" title={$t('index.menu')} aria-label={$t('index.menu')}>
				<img src={$user.imageUri} class="profile-image" width="40" height="40" alt="" />
			</a>

			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={$isOffline || progressBarActive}
				title={$t('index.refresh')}
				aria-label={$t('index.refresh')}
			>
				<i class="fas fa-sync-alt" />
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {$progress}%;" />
		</div>
	</div>

	<div class="content-wrap">
		{#each exercises as exercise}
			<a href="/progress/0?exerciseId={exercise.id}">{exercise.name}</a>
			<br />
		{/each}
	</div>
</section>

<style lang="scss">
</style>
