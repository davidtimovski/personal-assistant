<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { ThrottleHelper } from '$lib/utils/throttleHelper';
	import { state, remoteEvents } from '$lib/stores';
	import { TasksService } from '$lib/services/tasksService';
	import { DerivedLists, ListsService } from '$lib/services/listsService';
	import type { ListTask } from '$lib/models/viewmodels/listTask';
	import { SoundPlayer } from '$lib/utils/soundPlayer';
	import { RemoteEventType } from '$lib/models/remoteEvents';

	import DerivedTask from './components/DerivedTask.svelte';

	let type: string;
	let name = '';
	let tasks = new Array<ListTask>();
	let privateTasks = new Array<ListTask>();
	let iconClass: string | null = null;
	let shadowTasks: ListTask[];
	let shadowPrivateTasks: ListTask[];
	let searchTasksText = '';
	let soundsEnabled = false;
	const derivedListNameLookup = new Map<string, string>();
	const unsubscriptions: Unsubscriber[] = [];

	const localStorage = new LocalStorageUtil();
	let tasksService: TasksService;
	let soundPlayer: SoundPlayer;

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

		if (searchTasksText.length > 0) {
			resetSearchFilter();
		}

		// Animate action
		task.active = true;
		disableTasks();

		ThrottleHelper.executeAfterDelay(async () => {
			if ($state.lists === null) {
				throw new Error('Lists not loaded yet');
			}

			if (task.isOneTime) {
				if (!remote) {
					await tasksService.delete(task.id);
				}
				tasksService.deleteLocal(task.id, task.listId, $state.lists);
			} else {
				if (!remote) {
					await tasksService.complete(task.id);
				}
				tasksService.completeLocal(task.id, task.listId, $state.lists);
			}
		}, Date.now());
	}

	function disableTasks() {
		privateTasks = [
			...privateTasks.map((t) => {
				t.disabled = true;
				return t;
			})
		];
		tasks = [
			...tasks.map((t) => {
				t.disabled = true;
				return t;
			})
		];
	}

	onMount(async () => {
		derivedListNameLookup.set(DerivedLists.HighPriority, $t('highPriority'));
		derivedListNameLookup.set(DerivedLists.StaleTasks, $t('staleTasks'));

		type = <string>$page.url.searchParams.get('type');

		tasksService = new TasksService();
		soundPlayer = new SoundPlayer();

		soundsEnabled = localStorage.getBool(LocalStorageKeys.SoundsEnabled);

		unsubscriptions.push(
			state.subscribe((s) => {
				if (s.lists === null) {
					return;
				}

				const list = s.lists.find((x) => x.derivedListType === type);
				if (!list) {
					goto('/lists');
				} else {
					name = <string>derivedListNameLookup.get(type);
					iconClass = ListsService.getDerivedListIconClass(type);

					tasks = TasksService.getTasks(list.tasks, s.fromCache);
					privateTasks = TasksService.getPrivateTasks(list.tasks, s.fromCache);

					shadowTasks = tasks.slice();
					shadowPrivateTasks = privateTasks.slice();
				}
			})
		);

		unsubscriptions.push(
			remoteEvents.subscribe((e) => {
				if ($state.lists === null) {
					return;
				}

				if (e.type === RemoteEventType.TaskCompletedRemotely || e.type === RemoteEventType.TaskDeletedRemotely) {
					const allTasks = tasks.concat(privateTasks);

					const task = allTasks.find((x) => x.id === e.data.id);
					if (task) {
						complete(task, true);
					}
				} else if (e.type === RemoteEventType.TaskUncompletedRemotely) {
					const list = $state.lists.find((x) => x.derivedListType === type);
					if (!list) {
						throw new Error('List not found');
					}

					const task = list.tasks.find((x) => x.id === e.data.id);
					if (!task) {
						throw new Error('Task not found');
					}

					if (task.isPrivate) {
						privateTasks = TasksService.getPrivateTasks(list.tasks, false);
					} else {
						tasks = TasksService.getTasks(list.tasks, false);
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
		<a href="/lists" class="back-button">
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
				<div class="to-do-tasks-wrap private">
					<div class="private-tasks-label">
						<i class="fas fa-key" />
						<span>{$t('derivedList.privateTasks')}</span>
					</div>

					{#each privateTasks as task}
						<DerivedTask
							active={task.active}
							disabled={task.disabled}
							highPriority={task.isHighPriority}
							stale={type === DerivedLists.StaleTasks}
							name={task.name}
							url={task.url}
							isOneTime={task.isOneTime}
							modifiedDate={task.modifiedDate}
							on:click={() => complete(task)}
						/>
					{/each}
				</div>
			{/if}

			<div class="to-do-tasks-wrap">
				{#each tasks as task}
					<DerivedTask
						active={task.active}
						disabled={task.disabled}
						highPriority={task.isHighPriority}
						stale={type === DerivedLists.StaleTasks}
						assignedUser={task.assignedUser}
						name={task.name}
						url={task.url}
						isOneTime={task.isOneTime}
						modifiedDate={task.modifiedDate}
						on:click={() => complete(task)}
					/>
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

		&.private {
			background: #f4faff;
			border-radius: var(--border-radius);
			box-shadow: inset 0 1px 4px 0 rgba(0, 0, 0, 0.14);
			padding: 15px;
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
