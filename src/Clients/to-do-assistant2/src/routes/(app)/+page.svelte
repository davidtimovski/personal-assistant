<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { t } from '$lib/localization/i18n';
	import { loggedInUser, lists } from '$lib/stores';
	import { UsersService } from '$lib/services/usersService';
	import { ListsService } from '$lib/services/listsService';
	import type { ListModel } from '$lib/models/viewmodels/listModel';
	import type { ListIcon } from '$lib/models/viewmodels/listIcon';

	let imageUri: any;
	let computedLists: ListModel[] | null = null;
	let regularLists: ListModel[] | null = null;
	let iconOptions = ListsService.getIconOptions();
	let editedId: number | undefined;
	//let isReordering = false;
	let computedListNameLookup = new Map<string, string>();
	let menuButtonIsLoading = false;
	let connTracker = {
		isOnline: true
	};

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersService;
	let listsService: ListsService;

	function goToMenu() {
		menuButtonIsLoading = true;
		goto('/menu');
	}

	function sync() {
		listsService.getAll();

		usersService.getProfileImageUri().then((uri) => {
			if (imageUri !== uri) {
				imageUri = uri;
			}
		});
	}

	function setTaskCompletion(listId: number, isCompleted: boolean) {
		const list = <ListModel>(<ListModel[]>regularLists).find((x) => x.id === listId);

		if (isCompleted) {
			list.uncompletedTaskCount--;
		} else {
			list.uncompletedTaskCount++;
		}
	}

	function getClassFromIcon(icon: string): string {
		return (<ListIcon>iconOptions.find((x) => x.icon === icon)).cssClass;
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
		computedListNameLookup.set('high-priority', $t('highPriority'));

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		usersService = new UsersService();
		listsService = new ListsService();

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

		if ($lists.length === 0) {
			startProgressBar();
		}

		lists.subscribe((l) => {
			if (l.length === 0) {
				return;
			}

			computedLists = ListsService.getComputedForHomeScreen($lists, computedListNameLookup);
			regularLists = ListsService.getForHomeScreen($lists);

			finishProgressBar();
		});
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			{#if menuButtonIsLoading}
				<span class="menu-loader">
					<i class="fas fa-circle-notch fa-spin" />
				</span>
			{:else}
				<div
					on:click={goToMenu}
					class="profile-image-container"
					role="button"
					title={$t('index.menu')}
					aria-label={$t('index.menu')}
				>
					<img src={imageUri} class="profile-image" width="40" height="40" alt={$t('profilePicture')} />
				</div>
			{/if}
			<!-- 
			<label
				class="lists-reorder-toggle lists"
				class:checked={isReordering}
				title={$t('index.reorderLists')}
				aria-label={$t('index.reorderLists')}
			>
				<input type="checkbox" bind:checked={isReordering} />
				<i class="fas fa-random" />
			</label> -->
			<div class="page-title">
				<span />
			</div>
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

	<div class="content-wrap lists">
		<div class="to-do-lists-wrap">
			{#if !computedLists || !regularLists}
				<div>
					<div class="to-do-list-placeholder">&nbsp;</div>
					<div class="to-do-list-placeholder">&nbsp;</div>
					<div class="to-do-list-placeholder">&nbsp;</div>
					<div class="to-do-list-placeholder">&nbsp;</div>
					<div class="to-do-list-placeholder">&nbsp;</div>
				</div>
			{:else}
				<div class="au-stagger">
					{#each computedLists as list}
						<a class="to-do-list computed-list" href="/computedList?type{list.computedListType}">
							<i class="icon {list.computedListIconClass}" />
							<span class="name">{list.name}</span>
						</a>
					{/each}

					{#each regularLists as list}
						<a
							class="to-do-list"
							class:empty={list.isEmpty}
							class:highlighted={list.id === editedId}
							class:is-shared={list.sharingState !== 0 && list.sharingState !== 1}
							class:pending-share={list.sharingState === 1}
							href="/list/{list.id}"
						>
							<i class="icon {getClassFromIcon(list.icon)}" />
							<!-- <span class="sort-handle" title={$t('dragToReorder')} aria-label={$t('dragToReorder')}>
								<i class="reorder-icon fas fa-hand-paper" />
							</span> -->
							<span class="name">{list.name}</span>
							<i class="fas fa-users shared-icon" title={$t('index.shared')} aria-label={$t('index.shared')} />
							<i
								class="fas fa-user-clock shared-icon"
								title={$t('index.pendingAccept')}
								aria-label={$t('index.pendingAccept')}
							/>
						</a>
					{/each}
				</div>
			{/if}
		</div>

		<div class="centering-wrap">
			<a href="/editList/0" class="new-button" title={$t('index.newList')} aria-label={$t('index.newList')}>
				<i class="fas fa-plus" />
			</a>
		</div>
	</div>
</section>

<style lang="scss">
	// .lists-reorder-toggle {
	// 	display: flex;
	// 	justify-content: center;
	// 	align-items: center;
	// 	width: 45px;
	// 	height: 50px;
	// 	margin-left: 15px;
	// 	font-size: 25px;
	// 	text-decoration: none;
	// 	color: var(--faded-color);
	// 	cursor: pointer;

	// 	&.checked {
	// 		color: var(--primary-color);
	// 	}

	// 	input {
	// 		display: none;
	// 	}

	// 	&:hover {
	// 		color: var(--primary-color-dark);
	// 	}
	// }

	.content-wrap.lists {
		padding-top: 15px;
	}

	.to-do-lists-wrap {
		margin-bottom: 30px;

		// &.reordering .to-do-list:not(.computed-list) .icon {
		// 	display: none;
		// }
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

		// .reorder-icon {
		// 	display: none;
		// 	width: 45px;
		// 	line-height: 45px;
		// 	text-align: center;
		// 	color: var(--primary-color);
		// }

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

			//.reorder-icon,
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
	// .reordering .to-do-list:not(.computed-list) .reorder-icon {
	// 	display: inline-block !important;
	// }

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
