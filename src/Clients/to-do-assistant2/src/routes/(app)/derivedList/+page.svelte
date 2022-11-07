<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../../shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { user, lists, remoteEvents } from '$lib/stores';
	import { TasksService } from '$lib/services/tasksService';
	import { DerivedLists, ListsService } from '$lib/services/listsService';
	import type { ListTask } from '$lib/models/viewmodels/listTask';
	import { SoundPlayer } from '$lib/utils/soundPlayer';
	import { RemoteEventType } from '$lib/models/remoteEvents';

	let type: string;
	let name = '';
	let tasks = new Array<ListTask>();
	let privateTasks = new Array<ListTask>();
	let iconClass: string | null = null;
	let shadowTasks: ListTask[];
	let shadowPrivateTasks: ListTask[];
	let searchTasksText = '';
	let editedId: number | undefined;
	let soundsEnabled = false;
	const derivedListNameLookup = new Map<string, string>();
	const unsubscriptions: Unsubscriber[] = [];

	let localStorage: LocalStorageUtil;
	let tasksService: TasksService;
	let soundPlayer: SoundPlayer;

	function formatStaleTaskDate(modifiedDate: string): string {
		return DateHelper.formatDayMonth(new Date(modifiedDate), $user.language);
	}

	function searchTasksInputChanged() {
		if (searchTasksText.trim().length > 0) {
			filterTasks();
		} else {
			tasks = shadowTasks.slice();
			privateTasks = shadowPrivateTasks.slice();
		}
	}

	function clearFilter() {
		searchTasksText = '';
		filterTasks();
	}

	function filterTasks() {
		tasks = shadowTasks.filter((task: ListTask) => {
			return task.name.toLowerCase().includes(searchTasksText.trim().toLowerCase());
		});
		privateTasks = shadowPrivateTasks.filter((task: ListTask) => {
			return task.name.toLowerCase().includes(searchTasksText.trim().toLowerCase());
		});
	}

	function resetSearchFilter() {
		searchTasksText = '';
		tasks = shadowTasks.slice();
		privateTasks = shadowPrivateTasks.slice();
	}

	async function complete(task: ListTask, remote = false) {
		if (!remote && soundsEnabled) {
			soundPlayer.playBleep();
		}

		editedId = 0;

		if (searchTasksText.length > 0) {
			resetSearchFilter();
		}

		if (task.isOneTime) {
			if (!remote) {
				await tasksService.delete(task.id);
			}
			tasksService.deleteLocal(task.id, task.listId, $lists, localStorage);
		} else {
			if (!remote) {
				await tasksService.complete(task.id);
			}
			tasksService.completeLocal(task.id, task.listId, $lists, localStorage);
		}
	}

	onMount(async () => {
		derivedListNameLookup.set(DerivedLists.HighPriority, $t('highPriority'));
		derivedListNameLookup.set(DerivedLists.StaleTasks, $t('staleTasks'));

		type = <string>$page.url.searchParams.get('type');

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		tasksService = new TasksService();
		soundPlayer = new SoundPlayer();

		soundsEnabled = localStorage.getBool(LocalStorageKeys.SoundsEnabled);
		if (soundsEnabled) {
			soundPlayer.initialize();
		}

		unsubscriptions.push(
			lists.subscribe((l) => {
				if (l.length === 0) {
					return;
				}

				const list = l.find((x) => x.derivedListType === type);
				if (!list) {
					goto('/');
				} else {
					name = <string>derivedListNameLookup.get(type);
					iconClass = ListsService.getDerivedListIconClass(type);

					tasks = TasksService.getTasks(list.tasks);
					privateTasks = TasksService.getPrivateTasks(list.tasks);

					shadowTasks = tasks.slice();
					shadowPrivateTasks = privateTasks.slice();
				}
			})
		);

		unsubscriptions.push(
			remoteEvents.subscribe((e) => {
				if (e.type === RemoteEventType.TaskCompletedRemotely || e.type === RemoteEventType.TaskDeletedRemotely) {
					const allTasks = tasks.concat(privateTasks);

					const task = allTasks.find((x) => x.id === e.data.id);
					if (task) {
						complete(task, true);
					}
				} else if (e.type === RemoteEventType.TaskUncompletedRemotely) {
					const list = $lists.find((x) => x.derivedListType === type);
					if (!list) {
						throw new Error('List not found');
					}

					const task = list.tasks.find((x) => x.id === e.data.id);
					if (!task) {
						throw new Error('Task not found');
					}

					if (task.isPrivate) {
						privateTasks = TasksService.getPrivateTasks(list.tasks);
					} else {
						tasks = TasksService.getTasks(list.tasks);
					}
				}
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		tasksService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class={iconClass} />
		</div>
		<div class="page-title">{name}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="body">
		<div class="content-wrap">
			<form>
				<div class="search-tasks-wrap" class:searching={searchTasksText.length > 0}>
					<input
						type="text"
						bind:value={searchTasksText}
						on:keyup={searchTasksInputChanged}
						placeholder={$t('derivedList.searchTasks')}
						aria-label={$t('derivedList.searchTasks')}
						maxlength="50"
					/>
					<button type="button" on:click={clearFilter} title={$t('clear')} aria-label={$t('clear')}>
						<i class="fas fa-times" />
					</button>
				</div>
			</form>

			{#if privateTasks.length > 0}
				<div class="to-do-tasks-wrap private" class:high-priority={type === DerivedLists.HighPriority}>
					<div class="private-tasks-label">
						<i class="fas fa-key" />
						<span>{$t('derivedList.privateTasks')}</span>
					</div>

					{#each privateTasks as task}
						<div class="to-do-task">
							<div class="to-do-task-content" class:highlighted={task.id === editedId}>
								{#if type === DerivedLists.StaleTasks}
									<span class="unchanged-since">{formatStaleTaskDate(task.modifiedDate)}</span>
								{/if}

								{#if task.url}
									<a href={task.url} class="name" target="_blank" rel="noreferrer">{task.name}</a>
								{:else}
									<span class="name">{task.name}</span>
								{/if}

								<button
									type="button"
									on:click={() => complete(task)}
									class="check-button"
									class:one-time={task.isOneTime}
									title={$t('derivedList.complete')}
									aria-label={$t('derivedList.complete')}
								>
									<i class="far fa-square" />
									<i class="fas fa-check-square" />
									<i class="fas fa-trash-alt" />
								</button>
							</div>
						</div>
					{/each}
				</div>
			{/if}

			<div class="to-do-tasks-wrap" class:high-priority={type === DerivedLists.HighPriority}>
				{#each tasks as task}
					<div class="to-do-task">
						{#if task.assignedUser}
							<img
								src={task.assignedUser.imageUri}
								class="to-do-task-assignee-image"
								title={task.assignedUser.name}
								alt={$t('profilePicture', { name: task.assignedUser.name })}
							/>
						{/if}

						<div class="to-do-task-content" class:highlighted={task.id === editedId} class:assigned={task.assignedUser}>
							{#if type === DerivedLists.StaleTasks}
								<span class="unchanged-since">{formatStaleTaskDate(task.modifiedDate)}</span>
							{/if}

							{#if task.url}
								<a href={task.url} class="name" target="_blank" rel="noreferrer">{task.name}</a>
							{:else}
								<span class="name">{task.name}</span>
							{/if}

							<button
								type="button"
								on:click={() => complete(task)}
								class="check-button"
								class:one-time={task.isOneTime}
								title={$t('derivedList.complete')}
								aria-label={$t('derivedList.complete')}
							>
								<i class="far fa-square" />
								<i class="fas fa-check-square" />
								<i class="fas fa-trash-alt" />
							</button>
						</div>
					</div>
				{/each}
			</div>
		</div>
	</div>
</section>

<style lang="scss">
	.search-tasks-wrap {
		position: relative;

		input {
			line-height: 45px;

			&:disabled {
				text-align: center;
			}
		}

		button {
			display: none;
			position: absolute;
			top: 0;
			right: 0;
			background: transparent;
			border: none;
			outline: none;
			padding: 0 15px;
			font-size: 23px;
			line-height: 47px;
			color: var(--primary-color);

			&:hover {
				color: var(--primary-color-dark);
			}
		}

		&.searching {
			input {
				padding-right: 56px;
				width: calc(100% - 70px);
			}

			button {
				display: inline-block;
			}
		}
	}

	.to-do-tasks-wrap {
		margin-top: 35px;

		&.high-priority .to-do-task .to-do-task-content .name {
			padding-left: 50px;
		}

		&.private {
			background: #f4faff;
			border-radius: var(--border-radius);
			box-shadow: inset 0 1px 4px 0 rgba(0, 0, 0, 0.14);
			padding: 15px;
		}

		.to-do-task {
			display: flex;
			justify-content: flex-start;
			margin-bottom: 7px;

			&:last-child {
				margin-bottom: 0;
			}

			&-assignee-image {
				width: 34px;
				height: 34px;
				border-radius: 50%;
				margin: 6px 9px 0 0;
			}

			.check-button {
				min-width: 45px;
				background: transparent;
				border: none;
				outline: none;
				font-size: 27px;
				line-height: 45px;
				text-decoration: none;
				text-align: center;
				color: var(--primary-color);

				&:hover {
					color: var(--primary-color-dark);
				}

				.fa-check-square {
					display: none;
				}

				&:not(.one-time):active {
					.fa-check-square {
						display: inline;
					}
					.fa-square {
						display: none;
					}
				}

				&.one-time:active .fa-trash-alt {
					color: var(--danger-color);
				}
			}

			&-content {
				display: flex;
				justify-content: space-between;
				width: 100%;
				border-radius: 6px;

				&.private .to-do-task:last-child {
					margin-bottom: 0;
				}

				.unchanged-since {
					padding: 9px 5px;
					line-height: 27px;
					white-space: nowrap;
					color: #dd7001;
				}

				.name {
					width: 100%;
					border-bottom: 1px solid #ddd;
					padding: 9px 5px;
					line-height: 27px;
					text-align: center;
				}
				a.name {
					color: var(--primary-color-dark);
				}
				&.assigned .name {
					padding: 9px 48px 9px 50px;
				}

				.fa-square,
				.one-time .fa-trash-alt {
					display: inline;
				}

				.fa-trash-alt,
				.one-time .fa-square {
					display: none;
				}

				.one-time {
					font-size: 23px;
				}
			}
		}
	}

	.private-tasks-label {
		padding-bottom: 20px;
		text-align: center;
		user-select: none;

		i {
			margin-right: 5px;
			font-size: 16px;
		}
	}
</style>
