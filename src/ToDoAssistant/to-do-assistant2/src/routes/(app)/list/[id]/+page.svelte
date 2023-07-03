<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { slide } from 'svelte/transition';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import EmptyListMessage from '../../../../../../../Core/shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { ThrottleHelper } from '$lib/utils/throttleHelper';
	import { alertState, state, remoteEvents } from '$lib/stores';
	import { SharingState } from '$lib/models/viewmodels/sharingState';
	import { ListsService } from '$lib/services/listsService';
	import { TasksService } from '$lib/services/tasksService';
	import type { ListTask } from '$lib/models/viewmodels/listTask';
	import { SoundPlayer } from '$lib/utils/soundPlayer';
	import { RemoteEventType } from '$lib/models/remoteEvents';

	import Task from './components/Task.svelte';

	export let data: PageData;

	let name = '';
	let isOneTimeToggleDefault: boolean;
	let sharingState = SharingState.NotShared;
	let isArchived = false;
	let tasks = new Array<ListTask>();
	let privateTasks = new Array<ListTask>();
	let completedTasks = new Array<ListTask>();
	let completedPrivateTasks = new Array<ListTask>();
	let topDrawerIsOpen = false;
	let shareButtonText: string;
	let completedTasksAreVisible = false;
	let uncompleteDuplicateButtonVisible = false;
	let duplicateTaskMessageText: string;
	let duplicateTask: ListTask | null = null;
	let similarTasksMessageText: string;
	let similarTaskNames: string[] = [];
	let shadowTasks: ListTask[];
	let shadowPrivateTasks: ListTask[];
	let shadowCompletedTasks: ListTask[];
	let shadowCompletedPrivateTasks: ListTask[];
	let newTaskName = '';
	let newTaskUrl = '';
	let isPrivate = false;
	let isOneTime = false;
	let newTaskIsLoading = false;
	let newTaskIsInvalid = false;
	let newTaskNameInput: HTMLInputElement;
	let editedId: number | undefined;
	let isSearching = false;
	let soundsEnabled = false;
	let editListButtonIsLoading = false;
	let listIsArchivedText: string;
	let addNewPlaceholderText: string;
	const unsubscriptions: Unsubscriber[] = [];

	let localStorage: LocalStorageUtil;
	let listsService: ListsService;
	let tasksService: TasksService;
	let soundPlayer: SoundPlayer;

	function toggleTopDrawer() {
		topDrawerIsOpen = !topDrawerIsOpen;
	}

	function closeDrawer() {
		topDrawerIsOpen = false;
	}

	function isSearchingToggleChanged() {
		if (isSearching) {
			newTaskUrl = '';
			newTaskIsInvalid = false;
			completedTasksAreVisible = true;
			shadowTasks = tasks.slice();
			shadowPrivateTasks = privateTasks.slice();
			shadowCompletedTasks = completedTasks.slice();
			shadowCompletedPrivateTasks = completedPrivateTasks.slice();
			addNewPlaceholderText = $t('list.searchTasks');
			filterTasks();
			newTaskNameInput.focus();
		} else {
			resetSearchFilter(false);
		}
	}

	function isOneTimeToggleChanged() {
		newTaskNameInput.focus();
	}

	function isPrivateToggleChanged() {
		addNewPlaceholderText = isPrivate ? $t('list.addNewPrivate') : $t('list.addNew');
		newTaskNameInput.focus();
	}

	function newTaskNameInputChange(event: KeyboardEvent) {
		if (isSearching) {
			if (newTaskName.trim().length > 0) {
				filterTasks();
			} else {
				tasks = shadowTasks.slice();
				privateTasks = shadowPrivateTasks.slice();
				completedTasks = shadowCompletedTasks.slice();
				completedPrivateTasks = shadowCompletedPrivateTasks.slice();
			}
		} else if (event.key !== 'Enter') {
			newTaskIsInvalid = false;
			duplicateTask = null;
			similarTaskNames = [];
		}
	}

	function newTaskNameInputPaste(e: any) {
		newTaskIsInvalid = false;
		const pastedText = e.clipboardData.getData('Text');

		if (TasksService.isUrl(pastedText)) {
			newTaskUrl = pastedText.trim();
			window.setTimeout(() => {
				newTaskName = '';
				newTaskNameInput.focus();
			}, 0);
		}
	}

	function clearUrl() {
		newTaskUrl = '';
	}

	function filterTasks() {
		const searchText = newTaskName.trim().toLowerCase();

		const mapFunction = (task: ListTask) => {
			const index = task.name.toLowerCase().indexOf(searchText);
			return { task: task, index: index, matches: index >= 0 };
		};

		const substringPositionThenOrder = (a: { index: number; task: ListTask }, b: { index: number; task: ListTask }) => {
			if (a.index < b.index) return -1;
			if (a.index > b.index) return 1;
			return a.task.order - b.task.order;
		};

		tasks = shadowTasks
			.map(mapFunction)
			.filter((x) => x.matches)
			.sort(substringPositionThenOrder)
			.map((x) => x.task);
		privateTasks = shadowPrivateTasks
			.map(mapFunction)
			.filter((x) => x.matches)
			.sort(substringPositionThenOrder)
			.map((x) => x.task);
		completedTasks = shadowCompletedTasks
			.map(mapFunction)
			.filter((x) => x.matches)
			.sort(substringPositionThenOrder)
			.map((x) => x.task);
		completedPrivateTasks = shadowCompletedPrivateTasks
			.map(mapFunction)
			.filter((x) => x.matches)
			.sort(substringPositionThenOrder)
			.map((x) => x.task);
	}

	function resetSearchFilter(resetTaskName: boolean = true) {
		if (resetTaskName) {
			newTaskName = '';
		}
		isSearching = false;
		tasks = shadowTasks.slice();
		privateTasks = shadowPrivateTasks.slice();
		completedTasks = shadowCompletedTasks.slice();
		completedPrivateTasks = shadowCompletedPrivateTasks.slice();
		addNewPlaceholderText = $t('list.addNew');
	}

	function toggleCompletedTasksAreVisible() {
		completedTasksAreVisible = !completedTasksAreVisible;
	}

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(newTaskName)) {
			result.fail('name');
		}

		return result;
	}

	async function create() {
		newTaskIsLoading = true;
		editedId = 0;

		const result = validate();

		if (result.valid) {
			newTaskIsInvalid = false;

			duplicateTask = findDuplicateTask();

			if (duplicateTask) {
				newTaskIsLoading = false;
				newTaskIsInvalid = true;

				if (duplicateTask.isCompleted) {
					duplicateTaskMessageText = $t('list.alreadyExistsUncomplete');
					uncompleteDuplicateButtonVisible = true;
				} else {
					duplicateTaskMessageText = $t('list.alreadyExists');
					uncompleteDuplicateButtonVisible = false;
				}
			} else {
				if (similarTaskNames.length) {
					similarTaskNames = [];
				} else {
					similarTaskNames = findSimilarTasks(newTaskName);
				}

				if (similarTaskNames.length) {
					newTaskIsLoading = false;
					similarTasksMessageText = $t('list.similarTasksExist', {
						taskNames: similarTaskNames.map((x) => `<span class="colored-text">${x}</span>`).join(', ')
					});
				} else {
					newTaskIsInvalid = false;
					duplicateTask = null;
					similarTaskNames = [];
					try {
						await tasksService.create(data.id, newTaskName, newTaskUrl, isOneTime, isPrivate);
						await listsService.getAll();

						newTaskIsLoading = false;
						newTaskName = '';
						newTaskUrl = '';

						if ($state.lists === null) {
							throw new Error('Lists not loaded yet');
						}

						const list = $state.lists.find((x) => x.id === data.id);
						if (!list) {
							throw new Error('List not found');
						}

						if (isPrivate) {
							privateTasks = TasksService.getPrivateTasks(list.tasks, false);
						} else {
							tasks = TasksService.getTasks(list.tasks, false);
						}
						if (soundsEnabled) {
							soundPlayer.playBlop();
						}
					} catch {
						newTaskIsLoading = false;
						newTaskIsInvalid = true;
					}
				}
			}
		} else {
			newTaskIsInvalid = true;
			newTaskIsLoading = false;
		}
	}

	async function complete(task: ListTask, remote = false) {
		if (!remote && soundsEnabled) {
			soundPlayer.playBleep();
		}

		editedId = 0;

		if (isSearching) {
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
				tasksService.deleteLocal(task.id, data.id, $state.lists);
			} else {
				if (!remote) {
					await tasksService.complete(task.id);
				}
				tasksService.completeLocal(task.id, data.id, $state.lists);
			}
		}, Date.now());
	}

	async function uncomplete(task: ListTask, remote = false) {
		if (!remote && soundsEnabled) {
			soundPlayer.playBlop();
		}

		editedId = 0;

		if (isSearching) {
			resetSearchFilter();
		}

		// Animate action
		task.active = true;
		disableTasks();

		ThrottleHelper.executeAfterDelay(async () => {
			if ($state.lists === null) {
				throw new Error('Lists not loaded yet');
			}

			if (!remote) {
				await tasksService.uncomplete(task.id);
			}
			tasksService.uncompleteLocal(task.id, data.id, $state.lists);
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
		completedPrivateTasks = [
			...completedPrivateTasks.map((t) => {
				t.disabled = true;
				return t;
			})
		];
		completedTasks = [
			...completedTasks.map((t) => {
				t.disabled = true;
				return t;
			})
		];
	}

	async function editList() {
		editListButtonIsLoading = true;
		await goto(`/editList/${data.id}`);
	}

	async function back() {
		if (isArchived) {
			await goto('/archivedLists');
		} else {
			await goto('/lists');
		}
	}

	async function restore() {
		await listsService.setIsArchived(data.id, false);

		goto('/lists?edited=' + data.id);
	}

	async function uncompleteDuplicate() {
		if (!duplicateTask) {
			throw new Error('There is no duplicate');
		}

		newTaskName = '';
		newTaskIsInvalid = false;

		await uncomplete(duplicateTask);
		editedId = duplicateTask.id;
		duplicateTask = null;
	}

	function findDuplicateTask() {
		const allTasks = tasks.concat(completedTasks);

		for (let task of allTasks) {
			if (task.name.toLowerCase() === newTaskName.trim().toLowerCase()) {
				duplicateTask = task;
				return task;
			}
		}

		return null;
	}

	function findSimilarTasks(newTaskName: string): string[] {
		const newTaskNameWords = newTaskName.split(' ').filter((word) => word.length > 3);
		const allTaskNames = tasks
			.concat(privateTasks)
			.concat(completedTasks)
			.concat(completedPrivateTasks)
			.map((task) => {
				return task.name;
			});

		const similarTasks = [];
		for (const name of allTaskNames) {
			for (const newTaskWord of newTaskNameWords) {
				if (name.toLowerCase().includes(newTaskWord.toLowerCase())) {
					similarTasks.push(name);
				}
			}
		}
		return similarTasks;
	}

	function hideDuplicateTaskAlert() {
		newTaskIsInvalid = false;
		duplicateTask = null;
	}

	$: duplicateAlertIsVisible = duplicateTask !== null;

	$: similarTasksAlertIsVisible = similarTaskNames.length > 0;

	onMount(async () => {
		addNewPlaceholderText = $t('list.addNew');

		unsubscriptions.push(
			alertState.subscribe((value) => {
				if (value.hidden) {
					newTaskIsInvalid = false;
				}
			})
		);

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		listsService = new ListsService();
		tasksService = new TasksService();
		soundPlayer = new SoundPlayer();

		soundsEnabled = localStorage.getBool(LocalStorageKeys.SoundsEnabled);

		unsubscriptions.push(
			state.subscribe((s) => {
				if (s.lists === null) {
					return;
				}

				const list = s.lists.find((x) => x.id === data.id);
				if (!list) {
					throw new Error('List not found');
				}

				name = <string>list.name;
				isOneTimeToggleDefault = list.isOneTimeToggleDefault;
				sharingState = list.sharingState;
				isArchived = list.isArchived;
				tasks = TasksService.getTasks(list.tasks, s.fromCache);
				privateTasks = TasksService.getPrivateTasks(list.tasks, s.fromCache);
				completedTasks = TasksService.getCompletedTasks(list.tasks, s.fromCache);
				completedPrivateTasks = TasksService.getCompletedPrivateTasks(list.tasks, s.fromCache);
				isOneTime = isOneTimeToggleDefault;

				listIsArchivedText = $t('list.listIsArchived');
				shareButtonText = sharingState === SharingState.NotShared ? $t('list.shareList') : $t('list.members');
			})
		);

		unsubscriptions.push(
			remoteEvents.subscribe((e) => {
				if (e.type === RemoteEventType.TaskCompletedRemotely || e.type === RemoteEventType.TaskDeletedRemotely) {
					const allTasks = tasks.concat(privateTasks).concat(completedTasks).concat(completedPrivateTasks);

					const task = allTasks.find((x) => x.id === e.data.id);
					if (task) {
						complete(task, true);
					}
				} else if (e.type === RemoteEventType.TaskUncompletedRemotely) {
					const allTasks = tasks.concat(privateTasks).concat(completedTasks).concat(completedPrivateTasks);

					const task = allTasks.find((x) => x.id === e.data.id);
					if (task) {
						uncomplete(task, true);
					}
				}
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		listsService?.release();
		tasksService?.release();
	});
</script>

<section class="container" on:click={closeDrawer}>
	<div class="page-title-wrap">
		{#if editListButtonIsLoading}
			<span class="page-loading">
				<i class="fas fa-circle-notch fa-spin" />
			</span>
		{:else}
			<button
				type="button"
				on:click={editList}
				class="edit-button"
				title={$t('list.edit')}
				aria-label={$t('list.edit')}
			>
				<i class="fas fa-pencil-alt" />
			</button>
		{/if}

		<div class="page-title">{name}</div>

		<label
			class="search-toggle"
			class:checked={isSearching}
			title={$t('list.searchTasks')}
			aria-label={$t('list.searchTasks')}
		>
			<input type="checkbox" bind:checked={isSearching} on:change={isSearchingToggleChanged} />
			<i class="fas fa-search" />
		</label>

		<button type="button" on:click={back} class="back-button">
			<i class="fas fa-times" />
		</button>
	</div>

	<div class="body">
		<div class="top-buttons-drawer-container">
			<div class="top-buttons-drawer" class:open={topDrawerIsOpen}>
				<div class="top-buttons-drawer-wrap">
					<div class="top-buttons-drawer-content horizontal-buttons-wrap">
						<a href="/bulkAddTasks/{data.id}" class="wide-button">{$t('list.bulkAddTasks')}</a>
						<a href="/shareList/{data.id}" class="wide-button">{shareButtonText}</a>
						<a href="/copyList/{data.id}" class="wide-button">{$t('list.copy')}</a>
						<a href="/uncompleteTasks/{data.id}" class="wide-button">{$t('list.uncompleteAllTasks')}</a>

						{#if !isArchived}
							<a href="/archiveList/{data.id}" class="wide-button">{$t('list.archive')}</a>
						{/if}
					</div>
				</div>

				<button type="button" on:click|stopPropagation={toggleTopDrawer} class="top-drawer-handle">
					<i class="fas fa-angle-down" />
					<i class="fas fa-angle-up" />
				</button>
			</div>
		</div>

		<div class="content-wrap">
			{#if isArchived}
				<div class="archived-list-alert">
					<span class="side inactive small">
						<i class="fas fa-archive" />
					</span>
					<div class="alert-message" contenteditable="false" bind:innerHTML={listIsArchivedText} />
					<button
						type="button"
						on:click={restore}
						class="side"
						title={$t('list.restore')}
						aria-label={$t('list.restore')}
					>
						<i class="fas fa-check-circle" />
					</button>
				</div>
			{/if}

			{#if duplicateAlertIsVisible}
				<div class="duplicate-task-alert" transition:slide>
					<button type="button" on:click={hideDuplicateTaskAlert} class="side">
						<i class="fas fa-times-circle" />
					</button>
					<div class="alert-message danger">{duplicateTaskMessageText}</div>
					<button
						type="button"
						on:click={uncompleteDuplicate}
						class="side"
						class:hidden={!uncompleteDuplicateButtonVisible}
						title={$t('list.uncomplete')}
						aria-label={$t('list.uncomplete')}
					>
						<i class="fas fa-check-circle" />
					</button>
				</div>
			{/if}

			{#if similarTasksAlertIsVisible}
				<div class="duplicate-task-alert" transition:slide>
					<span class="side inactive">
						<i class="fas fa-info-circle" />
					</span>
					<div class="alert-message danger" contenteditable="false" bind:innerHTML={similarTasksMessageText} />
					<button type="button" on:click={create} class="side" title={$t('list.add')} aria-label={$t('list.add')}>
						<i class="fas fa-check-circle" />
					</button>
				</div>
			{/if}

			<form on:submit|preventDefault={create}>
				<div class="add-input-wrap" class:with-private-toggle={sharingState !== 0} class:searching={isSearching}>
					<div class="new-task-input-wrap" class:invalid={newTaskIsInvalid}>
						<input
							type="text"
							bind:value={newTaskName}
							bind:this={newTaskNameInput}
							on:keyup={newTaskNameInputChange}
							on:paste={newTaskNameInputPaste}
							class="new-task-input"
							placeholder={addNewPlaceholderText}
							aria-label={addNewPlaceholderText}
							readonly={newTaskIsLoading}
							maxlength="50"
							required
						/>
						{#if newTaskUrl}
							<div transition:slide class="new-task-url-wrap">
								<hr />
								<input type="url" bind:value={newTaskUrl} maxlength="1000" readonly />
								<button
									type="button"
									on:click={clearUrl}
									class="clear-url-button"
									title={$t('clear')}
									aria-label={$t('clear')}
								>
									<i class="fas fa-times" />
								</button>
							</div>
						{/if}
					</div>

					{#if sharingState !== 0 && !isSearching}
						<label
							class="is-private-toggle"
							class:checked={isPrivate}
							title={$t('list.togglePrivateTasks')}
							aria-label={$t('list.togglePrivateTasks')}
						>
							<input type="checkbox" bind:checked={isPrivate} on:change={isPrivateToggleChanged} />
							<i class="fas fa-lock" />
							<i class="fas fa-unlock" />
						</label>
					{/if}

					{#if !isSearching}
						<label
							class="is-one-time-toggle"
							class:checked={isOneTime}
							title={$t('list.toggleTaskDeletionOnCompletion')}
							aria-label={$t('list.toggleTaskDeletionOnCompletion')}
						>
							<input type="checkbox" bind:checked={isOneTime} on:change={isOneTimeToggleChanged} />
							<i class="fas fa-trash-alt" />
							<i class="far fa-trash-alt" />
						</label>
					{/if}

					{#if !newTaskIsLoading && !isSearching}
						<button
							type="button"
							on:click={create}
							disabled={$state.fromCache}
							class="add-task-button"
							title={$t('list.add')}
							aria-label={$t('list.add')}
						>
							<i class="fas fa-plus-circle" />
						</button>
					{/if}

					{#if newTaskIsLoading && !isSearching}
						<div class="loader">
							<i class="fas fa-circle-notch fa-spin" />
						</div>
					{/if}
				</div>
			</form>

			{#if privateTasks.length > 0}
				<div class="to-do-tasks-wrap private">
					<div class="private-tasks-label">
						<i class="fas fa-key" />
						<span>{$t('list.privateTasks')}</span>
					</div>

					{#each privateTasks as task}
						<Task
							active={task.active}
							disabled={task.disabled}
							highPriority={task.isHighPriority}
							highlighted={task.id === editedId}
							id={task.id}
							name={task.name}
							url={task.url}
							completed={task.isCompleted}
							isOneTime={task.isOneTime}
							on:click={() => complete(task)}
						/>
					{/each}
				</div>
			{/if}

			<div class="to-do-tasks-wrap">
				{#each tasks as task}
					<Task
						active={task.active}
						disabled={task.disabled}
						highPriority={task.isHighPriority}
						highlighted={task.id === editedId}
						assignedUser={task.assignedUser}
						id={task.id}
						name={task.name}
						url={task.url}
						completed={task.isCompleted}
						isOneTime={task.isOneTime}
						on:click={() => complete(task)}
					/>
				{/each}
			</div>

			{#if completedTasks.length > 0 || completedPrivateTasks.length > 0}
				<div>
					<button type="button" on:click={toggleCompletedTasksAreVisible} class="toggle-completed-visible">
						<div class="labeled-separator-text">
							<i class="fas fa-check" />

							{#if !completedTasksAreVisible}
								<span><span>{$t('list.showDone')}</span> ({completedTasks.length + completedPrivateTasks.length})</span>
							{:else}
								<span>{$t('list.hideDone')}</span>
							{/if}
						</div>
						<hr />
					</button>

					<div class="completed-tasks" class:visible={completedTasksAreVisible}>
						{#if completedPrivateTasks.length > 0}
							<div class="to-do-tasks-wrap private">
								<div class="private-tasks-label">
									<i class="fas fa-key" />
									<span>{$t('list.donePrivateTasks')}</span>
								</div>

								{#each completedPrivateTasks as task}
									<Task
										active={task.active}
										disabled={task.disabled}
										highPriority={task.isHighPriority}
										highlighted={task.id === editedId}
										id={task.id}
										name={task.name}
										url={task.url}
										completed={task.isCompleted}
										isOneTime={task.isOneTime}
										on:click={() => uncomplete(task)}
									/>
								{/each}
							</div>
						{/if}

						<div class="to-do-tasks-wrap">
							{#each completedTasks as task}
								<Task
									active={task.active}
									disabled={task.disabled}
									highPriority={task.isHighPriority}
									highlighted={task.id === editedId}
									assignedUser={task.assignedUser}
									id={task.id}
									name={task.name}
									url={task.url}
									completed={task.isCompleted}
									isOneTime={task.isOneTime}
									on:click={() => uncomplete(task)}
								/>
							{/each}
						</div>
					</div>
				</div>
			{/if}

			{#if tasks.length === 0 && completedTasks.length === 0}
				<EmptyListMessage messageKey="list.emptyListMessage" />
			{/if}
		</div>

		<button type="button" on:click={closeDrawer} class="body-overlay" class:visible={topDrawerIsOpen} />
	</div>
</section>

<style lang="scss">
	.page-title {
		padding: 10px 48px 10px 38px;
	}

	.alert-message {
		padding: 12px 10px;
		line-height: 27px;
		text-align: center;
	}

	.add-input-wrap {
		position: relative;

		.new-task-input-wrap {
			background-color: var(--input-background);
			border: 1px solid #ddd;
			border-radius: 6px;
			padding: 5px 12px;

			&.invalid {
				background: #fff0f6;
			}

			input {
				width: 100%;
				border: none;
				outline: none;
				background: transparent;
				padding: 0;
				line-height: 35px;
			}

			.new-task-input {
				width: calc(100% - 87px);
				padding-right: 100px;
			}

			.new-task-url-wrap {
				position: relative;

				input {
					width: calc(100% - 45px);
					color: #678;
				}

				.clear-url-button {
					position: absolute;
					top: 2px;
					right: -12px;
					background: none;
					border: none;
					outline: none;
					padding: 0 15px;
					font-size: 23px;
					line-height: 47px;
					text-decoration: none;
					color: var(--primary-color);
				}
			}

			hr {
				margin: 7px 0;
			}
		}

		&.with-private-toggle .new-task-input-wrap .new-task-input {
			width: calc(100% - 132px);
			padding-right: 145px;
		}

		&.searching .new-task-input-wrap .new-task-input {
			width: calc(100% - 26px);
			padding-right: 12px;
		}

		.add-task-button,
		.is-one-time-toggle,
		.is-private-toggle,
		.loader {
			position: absolute;
			top: 1px;
			right: 1px;
			background: none;
			border: none;
			outline: none;
			padding: 0 10px;
			font-size: 23px;
			line-height: 37px;
			text-decoration: none;
			color: var(--primary-color);
		}

		.add-task-button {
			line-height: 45px;

			&:disabled {
				color: var(--faded-color);
			}

			&:not(:disabled):hover {
				color: var(--primary-color-dark);
			}
		}

		.is-one-time-toggle {
			right: 45px;
			line-height: 45px;

			&:hover {
				color: var(--primary-color-dark);
			}
		}

		.is-private-toggle {
			right: 90px;
			line-height: 45px;

			&:hover {
				color: var(--primary-color-dark);
			}
		}

		.is-one-time-toggle input,
		.is-private-toggle input {
			display: none;
		}

		.is-one-time-toggle i:nth-child(2),
		.is-private-toggle i:nth-child(2) {
			display: none;
		}

		.is-one-time-toggle.checked i:nth-child(3),
		.is-private-toggle.checked i:nth-child(3) {
			display: none;
		}

		.is-one-time-toggle i:nth-child(3),
		.is-private-toggle i:nth-child(3) {
			color: var(--faded-color);
		}

		.is-one-time-toggle.checked i:nth-child(2),
		.is-private-toggle.checked i:nth-child(2) {
			display: inline;
		}

		.loader {
			top: 5px;
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

	.completed-tasks {
		display: none;

		&.visible {
			display: block;
		}

		.to-do-tasks-wrap {
			margin-top: 15px;
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

	.toggle-completed-visible {
		display: block;
		width: 100%;
		background: transparent;
		border: none;
		outline: none;
		padding: 3% 10% 3%;
		margin-top: 10%;
		color: var(--faded-color);
		text-align: center;

		&:hover {
			color: var(--primary-color);
		}

		i {
			margin-right: 5px;
		}

		hr {
			margin: 8px 0 0 0;
			color: inherit;
		}
	}

	.archived-list-alert {
		display: flex;
		justify-content: space-between;
		background: #f4faff;
		border-radius: 8px;
		margin-bottom: 25px;
		color: var(--primary-color);
	}

	.duplicate-task-alert {
		display: flex;
		justify-content: space-between;
		background: #f4faff;
		border-radius: 8px;
		margin-bottom: 25px;
	}

	.search-toggle {
		position: absolute;
		top: 0;
		right: 50px;
		height: 60px;
		width: 45px;
		display: flex;
		justify-content: center;
		align-items: center;
		font-size: 25px;
		text-decoration: none;
		color: var(--faded-color);

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

	@media screen and (min-width: 1200px) {
		.add-input-wrap {
			button,
			.is-one-time-toggle,
			.is-private-toggle,
			.loader {
				font-size: 25px;
				line-height: 45px;
			}

			.loader {
				top: 1px;
			}
		}
	}
</style>
