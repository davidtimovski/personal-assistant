<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { page } from '$app/stores';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { isOffline, user, lists, remoteEvents } from '$lib/stores';
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
	//let isReordering = false;
	const derivedListNameLookup = new Map<string, string>();
	const unsubscriptions: Unsubscriber[] = [];

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
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
		derivedLists = ListsService.getDerivedForHomeScreen($lists, derivedListNameLookup);
		regularLists = ListsService.getForHomeScreen($lists);
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

		if ($lists.length === 0) {
			startProgressBar();
		}

		unsubscriptions.push(
			lists.subscribe((l) => {
				if (l.length === 0) {
					return;
				}

				setListsFromState();
				finishProgressBar();
			})
		);

		unsubscriptions.push(
			remoteEvents.subscribe((e) => {
				if (e.type === RemoteEventType.TaskCompletedRemotely) {
					tasksService.completeLocal(e.data.id, e.data.listId, $lists, localStorage);
				} else if (e.type === RemoteEventType.TaskUncompletedRemotely) {
					tasksService.uncompleteLocal(e.data.id, e.data.listId, $lists, localStorage);
				} else if (e.type === RemoteEventType.TaskDeletedRemotely) {
					tasksService.deleteLocal(e.data.id, e.data.listId, $lists, localStorage);
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
			<div class="progress" class:visible={progressBarVisible} style="width: {progress}%;" />
		</div>
	</div>

	<div class="content-wrap lists">
		<div class="to-do-lists-wrap">
			{#if derivedLists && regularLists}
				<div>
					{#each derivedLists as list}
						<div class="to-do-list derived-list {list.derivedListType}">
							<i class="icon {list.derivedListIconClass}" />
							<a href="/derivedList?type={list.derivedListType}" class="name">{list.name}</a>
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
							<!-- <span class="sort-handle" title={$t('dragToReorder')} aria-label={$t('dragToReorder')}>
							<i class="reorder-icon fas fa-hand-paper" />
						</span> -->
							<a href="/list/{list.id}" class="name" class:highlighted={list.id === editedId}>{list.name}</a>
							<i class="fas fa-users shared-icon" title={$t('index.shared')} aria-label={$t('index.shared')} />
							<i
								class="fas fa-user-clock shared-icon"
								title={$t('index.pendingAccept')}
								aria-label={$t('index.pendingAccept')}
							/>
						</div>
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

		// &.reordering .to-do-list:not(.derived-list) .icon {
		// 	display: none;
		// }
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
			min-width: 43px;
			height: 41px;
			border: 2px solid #9df;
			border-radius: 6px;
			margin-right: 8px;
			line-height: 41px;
			text-align: center;
			font-size: 22px;
			color: var(--primary-color);
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

		&.derived-list {
			margin-bottom: 25px;

			.name {
				padding: 4px 15px;
				line-height: 33px;
			}

			&.high-priority {
				.name {
					border: 2px solid;

					&:hover {
						background: #ffe7e7;
					}
				}

				i,
				.name {
					background: #fff1f1;
					border-color: #f9a;
					color: var(--danger-color);
				}
			}

			&.stale-tasks {
				.name {
					border: 2px solid;

					&:hover {
						background: #fff0de;
					}
				}

				i,
				.name {
					background: #fff7e4;
					border-color: #f9ba76;
					color: #dd7001;
				}
			}
		}

		&.empty {
			.icon {
				border-color: #ddd;
				color: #aaa;
			}

			//.reorder-icon,
			.shared-icon {
				color: #aaa;
			}

			.name {
				background: #f4f4f4;
				color: #4a4a4a;

				&:hover {
					background: #eee;
				}
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

		&.is-shared .name {
			margin-right: 8px;
		}
	}

	// .sort-handle {
	// 	cursor: grab;
	// 	cursor: -webkit-grab;
	// }
	// .reordering .to-do-list:not(.derived-list) .reorder-icon {
	// 	display: inline-block !important;
	// }
</style>
