<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { UsersService } from '../../../../shared2/services/usersService';
	import { DateHelper } from '../../../../shared2/utils/dateHelper';

	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { t } from '$lib/localization/i18n';
	import { loggedInUser } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';

	let imageUri: any;
	let menuButtonIsLoading = false;
	let connTracker = {
		isOnline: true
	};
	let dataLoaded = false;

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersService;

	function goToMenu() {
		menuButtonIsLoading = true;
		goto('/menu');
	}

	function sync() {
		//syncStatus.set(AppEvents.ReSync);

		usersService.getProfileImageUri().then((uri) => {
			if (imageUri !== uri) {
				imageUri = uri;
			}
		});
	}

	function startProgressBar() {
		dataLoaded = false;
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
		usersService = new UsersService('ToDoAssistant');

		loggedInUser.subscribe((value) => {
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
		});

		// syncStatus.subscribe((value) => {
		// 	if (value === AppEvents.SyncStarted) {
		// 		startProgressBar();
		// 	} else if (value === AppEvents.SyncFinished) {

		// 	}
		// });
	});
</script>

<section class="container" />

<style lang="scss">
	.lists-reorder-toggle {
		display: flex;
		justify-content: center;
		align-items: center;
		width: 45px;
		height: 50px;
		margin-left: 15px;
		font-size: 25px;
		text-decoration: none;
		color: var(--faded-color);
		cursor: pointer;

		&.checked {
			color: var(--primary-color);
		}

		input {
			display: none;
		}

		&:hover {
			color: var(--primary-color-dark);
		}
	}

	.content-wrap.lists {
		padding-top: 15px;
	}

	.to-do-lists-wrap {
		margin-bottom: 30px;

		&.reordering .to-do-list:not(.computed-list) .icon {
			display: none;
		}
	}

	.to-do-list {
		position: relative;
		display: flex;
		justify-content: flex-start;
		background: #e9f4ff;
		border-radius: 6px;
		margin: 12px 0;
		text-decoration: none;
		user-select: none;

		&:first-child {
			margin-top: 0;
		}

		&:last-child {
			margin-bottom: 0;
		}

		&:hover {
			background: #e1f2ff;
		}

		.icon {
			min-width: 37px;
			height: 37px;
			background: #fff;
			border-radius: 6px;
			margin: 4px;
			line-height: 37px;
			text-align: center;
			font-size: 22px;
			color: var(--primary-color-dark);
		}

		.reorder-icon {
			display: none;
			width: 45px;
			line-height: 45px;
			text-align: center;
			color: var(--primary-color);
		}

		.shared-icon {
			display: none;
			position: absolute;
			top: 11px;
			right: 12px;
			font-size: 24px;
			color: var(--primary-color);
		}

		&.computed-list {
			margin-bottom: 25px;

			i {
				color: var(--danger-color);
			}
		}

		&.empty {
			background: #f4f4f4;

			.icon {
				color: #999;
			}

			.reorder-icon,
			.shared-icon {
				color: #aaa;
			}

			.name {
				color: #4a4a4a;
			}
		}

		&.is-shared,
		&.pending-share {
			padding-right: 50px;
		}

		&.is-shared .fa-users {
			display: inline;
		}
		&.pending-share .fa-user-clock {
			display: inline;
		}

		.name {
			padding: 8px 10px;
			line-height: 27px;
			font-size: 1.1rem;
			color: var(--regular-color);
		}
	}

	.sort-handle {
		cursor: grab;
		cursor: -webkit-grab;
	}
	.reordering .to-do-list:not(.computed-list) .reorder-icon {
		display: inline-block !important;
	}

	.to-do-list-placeholder {
		background: #e9f4ff;
		border-radius: var(--border-radius);
		padding: 9px 10px;
		margin: 12px 0;
		line-height: 27px;
		user-select: none;

		&:first-child {
			margin-top: 0;
		}

		&:last-child {
			margin-bottom: 0;
		}

		&:nth-child(2) {
			opacity: 0.85;
		}

		&:nth-child(3) {
			opacity: 0.65;
		}

		&:nth-child(4) {
			opacity: 0.55;
		}

		&:nth-child(5) {
			opacity: 0.35;
		}
	}
</style>
