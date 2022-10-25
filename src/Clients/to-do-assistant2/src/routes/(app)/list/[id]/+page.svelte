<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { slide } from 'svelte/transition';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';
	import EmptyListMessage from '../../../../../../shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { alertState, lists, remoteEvents } from '$lib/stores';
	import { SharingState } from '$lib/models/viewmodels/sharingState';
	import { ListsService } from '$lib/services/listsService';
	import { TasksService } from '$lib/services/tasksService';
	import type { ListTask } from '$lib/models/viewmodels/listTask';
	import { SoundPlayer } from '$lib/utils/soundPlayer';
	import { RemoteEventType } from '$lib/models/remoteEvents';

	export let data: PageData;

	let name = '';
	let isOneTimeToggleDefault: boolean;
	let sharingState = SharingState.NotShared;
	let isArchived = false;
	let derivedListType: string;
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
	let isPrivate = false;
	let isOneTime = false;
	let newTaskIsLoading = false;
	let newTaskIsInvalid = false;
	let newTaskNameInput: HTMLInputElement;
	let editedId: number | undefined;
	//let isReordering = false;
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

	// async function reorder(changedArray: ListTask[], actionData: any) {
	// 	const id: number = changedArray[actionData.toIndex].id;
	// 	const oldOrder = ++actionData.fromIndex;
	// 	const newOrder = ++actionData.toIndex;

	// 	await tasksService.reorder(id, data.id, oldOrder, newOrder);

	// 	const list = $lists.find((x) => x.id === data.id);
	// 	if (!list) {
	// 		throw new Error('List not found');
	// 	}

	// 	if (actionData.item.isPrivate) {
	// 		privateTasks = TasksService.getPrivateTasks(list.tasks);
	// 	} else {
	// 		tasks = TasksService.getTasks(list.tasks);
	// 	}
	// 	isReordering = false;
	// }

	function isSearchingToggleChanged() {
		if (isSearching) {
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

	function newTaskNameInputChanged(event: KeyboardEvent) {
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
				similarTaskNames = findSimilarTasks(newTaskName);

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
						await tasksService.create(data.id, newTaskName, isOneTime, isPrivate);
						await listsService.getAll();

						newTaskIsLoading = false;
						newTaskName = '';

						const list = $lists.find((x) => x.id === data.id);
						if (!list) {
							throw new Error('List not found');
						}

						if (isPrivate) {
							privateTasks = TasksService.getPrivateTasks(list.tasks);
						} else {
							tasks = TasksService.getTasks(list.tasks);
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

		if (task.isOneTime) {
			if (!remote) {
				await tasksService.delete(task.id);
			}
			tasksService.deleteLocal(task.id, data.id, $lists, localStorage);
		} else {
			completedTasksAreVisible = true;

			if (!remote) {
				await tasksService.complete(task.id);
			}
			tasksService.completeLocal(task.id, data.id, $lists, localStorage);
		}
	}

	async function uncomplete(task: ListTask, remote = false) {
		if (!remote && soundsEnabled) {
			soundPlayer.playBlop();
		}

		editedId = 0;

		if (isSearching) {
			resetSearchFilter();
		}

		if (!remote) {
			await tasksService.uncomplete(task.id);
		}
		tasksService.uncompleteLocal(task.id, data.id, $lists, localStorage);
	}

	async function editList() {
		editListButtonIsLoading = true;
		await goto(`/editList/${data.id}`);
	}

	async function back() {
		if (isArchived) {
			await goto('/archivedLists');
		} else {
			await goto('/');
		}
	}

	async function restore() {
		await listsService.setIsArchived(data.id, false);

		goto('/?edited=' + data.id);
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
		if (soundsEnabled) {
			soundPlayer.initialize();
		}

		unsubscriptions.push(
			lists.subscribe((l) => {
				if (l.length === 0) {
					return;
				}

				const list = l.find((x) => x.id === data.id);
				if (!list) {
					throw new Error('List not found');
				}

				name = <string>list.name;
				isOneTimeToggleDefault = list.isOneTimeToggleDefault;
				sharingState = list.sharingState;
				isArchived = list.isArchived;
				derivedListType = list.derivedListType;
				tasks = TasksService.getTasks(list.tasks);
				privateTasks = TasksService.getPrivateTasks(list.tasks);
				completedTasks = TasksService.getCompletedTasks(list.tasks);
				completedPrivateTasks = TasksService.getCompletedPrivateTasks(list.tasks);
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

		<!-- <label
			class="tasks-reorder-toggle tasks"
			class:checked={isReordering}
			title={$t('list.reorderTasks')}
			aria-label={$t('list.reorderTasks')}
		>
			<input type="checkbox" bind:checked={isReordering} />
			<i class="fas fa-random" />
		</label> -->

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
					<div class="alert-message" contenteditable="true" bind:innerHTML={listIsArchivedText} />
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
				<div class="duplicate-task-alert" in:slide>
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
				<div class="duplicate-task-alert" in:slide>
					<span class="side inactive">
						<i class="fas fa-info-circle" />
					</span>
					<div class="alert-message danger" contenteditable="true" bind:innerHTML={similarTasksMessageText} />
					<button type="button" on:click={create} class="side" title={$t('list.add')} aria-label={$t('list.add')}>
						<i class="fas fa-check-circle" />
					</button>
				</div>
			{/if}

			<form on:submit|preventDefault={create}>
				<div class="add-input-wrap" class:with-private-toggle={sharingState !== 0} class:searching={isSearching}>
					<input
						type="text"
						bind:value={newTaskName}
						bind:this={newTaskNameInput}
						on:keyup={newTaskNameInputChanged}
						class="new-task-input"
						class:invalid={newTaskIsInvalid}
						placeholder={addNewPlaceholderText}
						aria-label={addNewPlaceholderText}
						readonly={newTaskIsLoading}
						maxlength="50"
						required
					/>

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
						<button on:click={create} class="add-task-button" title={$t('list.add')} aria-label={$t('list.add')}>
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
						<div class="to-do-task" class:high-priority={derivedListType !== 'high-priority' && task.isHighPriority}>
							<div class="to-do-task-content" class:highlighted={task.id === editedId}>
								<a href="/editTask/{task.id}" class="edit-button" title={$t('list.edit')} aria-label={$t('list.edit')}>
									<i class="fas fa-pencil-alt" />
								</a>
								<!-- <span class="sort-handle" title={$t('dragToReorder')} aria-label={$t('dragToReorder')}>
									<i class="reorder-icon fas fa-hand-paper" />
								</span> -->

								<span class="name">{task.name}</span>

								<button
									type="button"
									on:click={() => complete(task)}
									class="check-button"
									class:one-time={task.isOneTime}
									title={$t('list.complete')}
									aria-label={$t('list.complete')}
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

			<div class="to-do-tasks-wrap">
				{#each tasks as task}
					<div class="to-do-task" class:high-priority={derivedListType !== 'high-priority' && task.isHighPriority}>
						<div class="to-do-task-content" class:assigned={task.assignedUser} class:highlighted={task.id === editedId}>
							<a href="/editTask/{task.id}" class="edit-button" title={$t('list.edit')} aria-label={$t('list.edit')}>
								<i class="fas fa-pencil-alt" />
							</a>
							<!-- <span class="sort-handle" title={$t('dragToReorder')} aria-label={$t('dragToReorder')}>
								<i class="reorder-icon fas fa-hand-paper" />
							</span> -->

							{#if task.assignedUser}
								<img src={task.assignedUser.imageUri} class="to-do-task-assignee-image" alt={$t('profilePicture')} />
							{/if}

							<span class="name">{task.name}</span>

							<button
								type="button"
								on:click={() => complete(task)}
								class="check-button"
								class:one-time={task.isOneTime}
								title={$t('list.complete')}
								aria-label={$t('list.complete')}
							>
								<i class="far fa-square" />
								<i class="fas fa-check-square" />
								<i class="fas fa-trash-alt" />
							</button>
						</div>
					</div>
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
									<div class="to-do-task completed">
										<div class="to-do-task-content" class:highlighted={task.id === editedId}>
											<a
												href="/editTask/{task.id}"
												class="edit-button"
												title={$t('list.edit')}
												aria-label={$t('list.edit')}
											>
												<i class="fas fa-pencil-alt" />
											</a>
											<!-- <span class="sort-handle" title={$t('dragToReorder')} aria-label={$t('dragToReorder')}>
												<i class="reorder-icon fas fa-hand-paper" />
											</span> -->

											<span class="name">{task.name}</span>

											<button
												type="button"
												on:click={() => uncomplete(task)}
												class="uncheck-button"
												title={$t('list.uncomplete')}
												aria-label={$t('list.uncomplete')}
											>
												<i class="fas fa-check-square" />
												<i class="far fa-square" />
											</button>
										</div>
									</div>
								{/each}
							</div>
						{/if}

						<div class="to-do-tasks-wrap">
							{#each completedTasks as task}
								<div class="to-do-task completed">
									<div
										class="to-do-task-content"
										class:assigned={task.assignedUser}
										class:highlighted={task.id === editedId}
									>
										<a
											href="/editTask/{task.id}"
											class="edit-button"
											title={$t('list.edit')}
											aria-label={$t('list.edit')}
										>
											<i class="fas fa-pencil-alt" />
										</a>

										<!-- <span class="sort-handle" title={$t('dragToReorder')} aria-label={$t('dragToReorder')}>
											<i class="reorder-icon fas fa-hand-paper" />
										</span> -->

										{#if task.assignedUser}
											<img
												src={task.assignedUser.imageUri}
												class="to-do-task-assignee-image"
												alt={$t('profilePicture')}
											/>
										{/if}

										<span class="name">{task.name}</span>

										<button
											type="button"
											on:click={() => uncomplete(task)}
											class="uncheck-button"
											title={$t('list.uncomplete')}
											aria-label={$t('list.uncomplete')}
										>
											<i class="fas fa-check-square" />
											<i class="far fa-square" />
										</button>
									</div>
								</div>
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

		.new-task-input {
			width: calc(100% - 60px);
			padding-right: 46px;
			line-height: 45px;

			// &.new-task {
			// 	width: calc(100% - 112px);
			// 	padding-right: 98px;
			// }
		}

		&.with-private-toggle .new-task-input {
			width: calc(100% - 154px);
			padding-right: 140px;
		}

		&.searching .new-task-input {
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

			&:hover {
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

		// &.reordering .edit-button {
		// 	display: none;
		// }

		.to-do-task {
			display: flex;
			justify-content: flex-start;
			margin-bottom: 7px;

			&:last-child {
				margin-bottom: 0;
			}

			&.completed {
				opacity: 0.6;

				&:hover {
					opacity: 1;
				}
			}

			&.high-priority {
				font-weight: bold;
				color: var(--danger-color-dark);
			}

			&-assignee-image {
				width: 34px;
				height: 34px;
				border-radius: 50%;
				margin: 6px 9px 0 4px;
			}

			.fa-square,
			.fa-check-square,
			.one-time .fa-trash-alt {
				display: inline;
			}

			.fa-trash-alt,
			.one-time .fa-square,
			.one-time .fa-check-square {
				display: none;
			}

			.one-time {
				font-size: 23px;
			}

			.check-button,
			.uncheck-button {
				min-width: 45px;
				background: transparent;
				border: none;
				outline: none;
				font-size: 27px;
				line-height: 45px;
				text-align: center;
				color: var(--primary-color);

				&:hover {
					color: var(--primary-color-dark);
				}
			}

			.check-button {
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

			.uncheck-button {
				.fa-square {
					display: none;
				}

				&:active {
					.fa-square {
						display: inline;
					}
					.fa-check-square {
						display: none;
					}
				}
			}

			// .reorder-icon {
			// 	display: none;
			// 	width: 45px;
			// 	line-height: 45px;
			// 	text-align: center;
			// 	color: var(--primary-color);
			// }

			&-content {
				display: flex;
				justify-content: space-between;
				width: 100%;
				border-radius: 6px;

				&.private .to-do-task:last-child {
					margin-bottom: 0;
				}

				.name {
					width: 100%;
					border-bottom: 1px solid #ddd;
					padding: 9px 5px;
					line-height: 27px;
					text-align: center;
					cursor: default;
				}
				&.assigned .name {
					padding: 9px 52px 9px 5px;
				}
			}
		}
	}

	// .sort-handle {
	// 	cursor: grab;
	// 	cursor: -webkit-grab;
	// }
	// .reordering .reorder-icon {
	// 	display: inline-block !important;
	// }

	.edit-button {
		min-width: 45px;
		font-size: 23px;
		line-height: 45px;
		text-decoration: none;
		text-align: center;
		color: var(--primary-color);

		&:hover {
			color: var(--primary-color-dark);
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

	// .tasks-reorder-toggle {
	// 	position: absolute;
	// 	top: 0;
	// 	left: 55px;
	// 	z-index: 1;
	// 	display: flex;
	// 	justify-content: center;
	// 	align-items: center;
	// 	width: 45px;
	// 	height: 60px;
	// 	font-size: 25px;
	// 	text-decoration: none;
	// 	color: var(--faded-color);

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
