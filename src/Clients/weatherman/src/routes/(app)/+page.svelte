<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';

	import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { authInfo } from '$lib/stores';

	let imageUri: any;
	let connTracker = {
		isOnline: true
	};
	const unsubscriptions: Unsubscriber[] = [];

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersServiceBase;

	function sync() {
		startProgressBar();

		//forecastsService.getAll();

		usersService.getProfileImageUri().then((uri) => {
			if (imageUri !== uri) {
				imageUri = uri;
			}
		});
	}

	function startProgressBar() {
		progressBarActive = true;
		progress = 10;

		progressIntervalId = window.setInterval(() => {
			if (progress < 85) {
				progress += 15;
			} else if (progressIntervalId) {
				window.clearInterval(progressIntervalId);
			}
		}, 500);

		progressBarVisible = true;
	}

	function finishProgressBar() {
		window.setTimeout(() => {
			progress = 100;
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	onMount(() => {
		localStorage = new LocalStorageUtil();
		usersService = new UsersServiceBase('Weatherman');

		unsubscriptions.push(
			authInfo.subscribe((value) => {
				if (!value) {
					return;
				}

				if (usersService.profileImageUriIsStale()) {
					usersService.getProfileImageUri().then((uri) => {
						imageUri = uri;
					});
				} else {
					imageUri = localStorage.get('profileImageUri');
				}
			})
		);

		// if ($lists.length === 0) {
		// 	startProgressBar();
		// }

		unsubscriptions
			.push
			// lists.subscribe((l) => {
			// 	if (l.length === 0) {
			// 		return;
			// 	}

			// 	setListsFromState();
			// 	finishProgressBar();
			// })
			();
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		usersService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			<a href="/menu" class="profile-image-container" title={$t('index.menu')} aria-label={$t('index.menu')}>
				<img src={imageUri} class="profile-image" width="40" height="40" alt={$t('profilePicture')} />
			</a>

			<div class="page-title" />
			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={!connTracker.isOnline || progressBarActive}
				title={$t('index.refresh')}
				aria-label={$t('index.refresh')}
			>
				<i class="fas fa-sync-alt" />
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {progress}%;" />
		</div>
	</div>

	<div class="content-wrap" />
</section>

<style lang="scss">
</style>
