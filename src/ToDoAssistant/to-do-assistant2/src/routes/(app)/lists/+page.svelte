<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { tweened } from 'svelte/motion';
	import { cubicOut } from 'svelte/easing';
	import { page } from '$app/stores';

	import EmptyListMessage from '../../../../../../Core/shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { isOffline, user, state, remoteEvents } from '$lib/stores';
	import { UsersService } from '$lib/services/usersService';
	import { DerivedLists, ListsService } from '$lib/services/listsService';
	import { TasksService } from '$lib/services/tasksService';
	import type { ListModel } from '$lib/models/viewmodels/listModel';
	import type { ListIcon } from '$lib/models/viewmodels/listIcon';
	import { RemoteEventType } from '$lib/models/remoteEvents';

	let derivedLists: ListModel[] | null = null;
	let regularLists: ListModel[] | null = null;
	let iconOptions = ListsService.getIconOptions();
	let editedId: number | undefined;
	const derivedListNameLookup = new Map<string, string>();
	const unsubscriptions: Unsubscriber[] = [];

	// Progress bar
	let progressBarActive = false;
	const progress = tweened(0, {
		duration: 500,
		easing: cubicOut
	});
	let progressIntervalId: number | undefined;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersService;
	let listsService: ListsService;
	let tasksService: TasksService;

	function sync() {
		startProgressBar();

		listsService.getAll();
	}

	function setListsFromState() {
		if ($state.lists === null) {
			throw new Error('Lists not loaded yet');
		}

		derivedLists = ListsService.getDerivedForHomeScreen($state.lists, derivedListNameLookup);
		regularLists = ListsService.getForHomeScreen($state.lists);
	}

	function getClassFromIcon(icon: string): string {
		return (<ListIcon>iconOptions.find((x) => x.icon === icon)).cssClass;
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
		progress.set(100);
		window.setTimeout(() => {
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	onMount(() => {
		derivedListNameLookup.set(DerivedLists.HighPriority, $t('highPriority'));
		derivedListNameLookup.set(DerivedLists.StaleTasks, $t('staleTasks'));

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		usersService = new UsersService();
		listsService = new ListsService();
		tasksService = new TasksService();

		if ($state.lists === null) {
			startProgressBar();
		}

		unsubscriptions.push(
			state.subscribe((s) => {
				if (s.lists === null) {
					return;
				}

				setListsFromState();

				if (!s.fromCache) {
					finishProgressBar();
				}
			})
		);

		unsubscriptions.push(
			remoteEvents.subscribe((e) => {
				if ($state.lists === null) {
					return;
				}

				if (e.type === RemoteEventType.TaskCompletedRemotely) {
					tasksService.completeLocal(e.data.id, e.data.listId, $state.lists);
				} else if (e.type === RemoteEventType.TaskUncompletedRemotely) {
					tasksService.uncompleteLocal(e.data.id, e.data.listId, $state.lists);
				} else if (e.type === RemoteEventType.TaskDeletedRemotely) {
					tasksService.deleteLocal(e.data.id, e.data.listId, $state.lists);
				}

				setListsFromState();
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		usersService?.release();
		listsService?.release();
		tasksService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			<a href="/menu" class="profile-image-container" title={$t('index.menu')} aria-label={$t('index.menu')}>
				<img src={$user.imageUri} class="profile-image" width="40" height="40" alt="" />
			</a>

			<div class="page-title" />
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

	<div class="content-wrap lists">
		<div class="to-do-lists-wrap">
			{#if derivedLists && regularLists}
				{#if regularLists.length > 0}
					{#each derivedLists as list}
						<div class="to-do-list derived-list {list.derivedListType}">
							<i class="icon {list.derivedListIconClass}" />
							<a href="/derivedList?type={list.derivedListType}" class="name-container">{list.name}</a>
						</div>
					{/each}

					{#each regularLists as list}
						<div
							class="to-do-list"
							class:empty={list.uncompletedTaskCount === 0}
							class:is-shared={list.sharingState !== 0 && list.sharingState !== 1}
							class:pending-share={list.sharingState === 1}
						>
							<i class="icon {getClassFromIcon(list.icon)}" />
							<a href="/list/{list.id}" class="name-container" class:highlighted={list.id === editedId}>
								<span class="name">{list.name}</span>
								<span class="sharing-icon shared" title={$t('index.shared')} aria-label={$t('index.shared')}
									><i class="fas fa-users" /></span
								>
								<span
									class="sharing-icon pending-accept"
									title={$t('index.pendingAccept')}
									aria-label={$t('index.pendingAccept')}><i class="fas fa-user-clock" /></span
								>
							</a>
						</div>
					{/each}
				{:else}
					<EmptyListMessage messageKey="index.emptyListMessage" />
				{/if}
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
	.content-wrap.lists {
		padding-top: 15px;
	}

	.to-do-lists-wrap {
		margin-bottom: 30px;
	}

	.to-do-list {
		position: relative;
		display: flex;
		justify-content: flex-start;
		margin: 12px 0;
		user-select: none;

		&:first-child {
			margin-top: 0;
		}

		&:last-child {
			margin-bottom: 0;
		}

		.icon {
			min-width: 45px;
			height: 45px;
			border-radius: 6px;
			margin-right: 8px;
			line-height: 45px;
			text-align: center;
			font-size: 22px;
			color: var(--primary-color);
		}

		.sharing-icon {
			display: none;
			background: #fff;
			border-radius: 4px;
			padding: 0 7px;
			font-size: 1rem;
			color: #8ac;
		}

		&.derived-list {
			margin-bottom: 25px;

			&.high-priority {
				.name-container:hover {
					background: #ffe7e7;
				}

				i,
				.name-container {
					background: #fff1f1;
					color: var(--danger-color);
				}
			}

			&.stale-tasks {
				.name-container:hover {
					background: #fff0de;
				}

				i,
				.name-container {
					background: #fff7e4;
					color: #dd7001;
				}
			}
		}

		&.empty {
			.icon {
				color: #aaa;
			}

			.sharing-icon {
				color: #bbb;
			}

			.name-container {
				background: #f4f4f4;

				&:hover {
					background: #eee;
				}
			}
		}

		&.is-shared,
		&.pending-share {
			.name-container {
				padding-right: 10px;

				.name {
					padding-right: 15px;
				}
			}
		}

		&.is-shared .sharing-icon.shared {
			display: inline-block;
		}
		&.pending-share .sharing-icon.pending-accept {
			display: inline-block;
		}

		.name-container {
			display: flex;
			justify-content: space-between;
			width: 100%;
			background: #e9f4ff;
			border-radius: 6px;
			padding: 8px 15px;
			line-height: 29px;
			font-size: 1.1rem;
			text-decoration: none;
			color: var(--regular-color);

			&:hover {
				background: #e1f0ff;
			}
		}
	}
</style>
