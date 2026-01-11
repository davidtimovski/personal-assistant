<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../../Core/shared2/models/validationErrors';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';
	import Tooltip from '../../../../../../../Core/shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { alertState, localState } from '$lib/stores';
	import { TasksService } from '$lib/services/tasksService';
	import { ListsService } from '$lib/services/listsService';
	import type { List } from '$lib/models/entities';
	import type { Assignee } from '$lib/models/viewmodels/assignee';
	import type { ListOption } from '$lib/models/viewmodels/listOption';
	import type { EditTaskModel } from '$lib/models/viewmodels/editTaskModel';
	import { UpdateTask } from '$lib/models/server/requests/updateTask';
	import Variables from '$lib/variables';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let listId: number | null = $state(null);
	let name = $state('');
	let url = $state('');
	let isOneTime: boolean | null = $state(null);
	let isPrivate: boolean | null = $state(null);
	let isHighPriority: boolean | null = $state(null);
	let assignedToUserId: number | null = $state(null);
	let isInSharedList: boolean | null = $state(null);
	let recipes: Array<string> | null = $state(null);
	let listOptions: ListOption[] | null = $state(null);
	let assigneeOptions: Assignee[] | null = $state(null);
	let nobodyImageUri = Variables.urls.defaultProfileImageUrl;
	let nameIsInvalid = $state(false);
	let urlIsInvalid = $state(false);
	let recipesText = $state('');
	let deleteInProgress = $state(false);
	let deleteButtonText = $state('');
	let saveButtonIsLoading = $state(false);
	let deleteButtonIsLoading = $state(false);
	let taskUsedAsIngredientText: string | null = $state(null);
	let deletingWillUnlinkText: string | null = $state(null);
	let loading = $state(true);

	const alertStateUnsub = alertState.subscribe((value) => {
		if (value.hidden) {
			nameIsInvalid = false;
		}
	});

	let tasksService: TasksService;
	let listsService: ListsService;

	$effect(() => {
		if (isPrivate) {
			assignedToUserId = null;
		}
	});

	let canSave = $derived(!ValidationUtil.isEmptyOrWhitespace(name));

	let assignToUserLabel = $derived($t(assignedToUserId ? 'editTask.assignedTo' : 'editTask.assignToUser'));

	async function loadAssigneeOptions() {
		if (listId === null) {
			throw new Error('ListId not initialized');
		}

		const selectedList = <ListOption>(<ListOption[]>listOptions).find((x) => x.id === listId);
		isInSharedList = selectedList.isShared;

		if (isInSharedList) {
			assigneeOptions = await listsService.getMembersAsAssigneeOptions(listId);
		} else {
			assigneeOptions = [];
		}
	}

	function clearUrl() {
		url = '';
	}

	async function save(event: Event) {
		if (listId === null || isOneTime === null || isHighPriority === null) {
			throw new Error('Data not initialized');
		}

		event.preventDefault();

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = TasksService.validateEdit(name, url);

		if (result.valid) {
			nameIsInvalid = false;

			try {
				await tasksService.update(new UpdateTask(data.id, listId, name, url, isOneTime, isHighPriority, isPrivate, assignedToUserId));
				await listsService.getAll();

				goto(`/list/${listId}?edited=${data.id}`);
			} catch (e) {
				if (e instanceof ValidationErrors) {
					nameIsInvalid = e.fields.includes('Name');
				}

				saveButtonIsLoading = false;
			}
		} else {
			nameIsInvalid = result.erroredFields.includes('name');
			urlIsInvalid = result.erroredFields.includes('url');
			saveButtonIsLoading = false;
		}
	}

	async function deleteTask() {
		if (listId === null || recipes === null) {
			throw new Error('Data not initialized');
		}

		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			await tasksService.delete(data.id);
			tasksService.deleteLocal(data.id, listId, <List[]>$localState.lists);

			alertState.update((x) => {
				x.showSuccess('editTask.deleteSuccessful');
				return x;
			});
			goto(`/list/${listId}`);
		} else {
			if (recipes.length > 0) {
				deleteButtonText = $t('editTask.okayDelete');
			} else {
				deleteButtonText = $t('sure');
			}

			deleteInProgress = true;
		}
	}

	async function cancel() {
		if (!deleteInProgress) {
			await goto(`/list/${listId}`);
		}
		deleteButtonText = $t('delete');
		deleteInProgress = false;
	}

	onMount(async () => {
		deleteButtonText = $t('delete');

		tasksService = new TasksService();
		listsService = new ListsService();

		const tasksPromise = tasksService.getForUpdate(data.id).then((task: EditTaskModel) => {
			if (task === null) {
				throw new Error('Task not found');
			}

			listId = task.listId;
			name = task.name;
			url = task.url;
			isOneTime = task.isOneTime;
			isPrivate = task.isPrivate;
			isHighPriority = task.isHighPriority;
			assignedToUserId = task.assignedToUserId;
			isInSharedList = task.isInSharedList;
			recipes = task.recipes;

			if (isInSharedList) {
				loadAssigneeOptions();
			}

			if (recipes.length > 0) {
				taskUsedAsIngredientText = $t('editTask.taskUsedAsIngredientInRecipes');
				recipesText = recipes.join(', ');
				deletingWillUnlinkText = $t('editTask.deletingItWillUnlinkIt');
			}
		});

		const listOptionsPromise = listsService.getAllAsOptions().then((options) => {
			listOptions = options;
		});

		await Promise.all([tasksPromise, listOptionsPromise]);
		loading = false;
	});

	onDestroy(() => {
		alertStateUnsub();
		tasksService?.release();
		listsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt"></i>
		</div>
		<div class="page-title">
			<span><span>{$t('editTask.edit')}</span>&nbsp;<span class="colored-text">{name}</span></span>
		</div>

		<a href="/list/{listId}" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		{#if loading}
			<div class="double-circle-loading">
				<div class="double-bounce1"></div>
				<div class="double-bounce2"></div>
			</div>
		{:else}
			<form onsubmit={save}>
				<div class="form-control">
					<input
						type="text"
						bind:value={name}
						maxlength="50"
						class:invalid={nameIsInvalid}
						placeholder={$t('name')}
						aria-label={$t('name')}
						required
					/>
				</div>

				<div class="form-control url">
					<input
						type="url"
						bind:value={url}
						maxlength="1000"
						class:invalid={urlIsInvalid}
						placeholder={$t('editTask.url')}
						aria-label={$t('editTask.url')}
					/>
					{#if url}
						<button type="button" onclick={clearUrl} class="clear-url-button" title={$t('clear')} aria-label={$t('clear')}>
							<i class="fas fa-times"></i>
						</button>
					{/if}
				</div>

				<div class="form-control">
					<select id="from-account" bind:value={listId} onchange={loadAssigneeOptions} disabled={!listOptions} aria-label={$t('editTask.list')}>
						{#if listOptions}
							{#each listOptions as list}
								<option value={list.id}>{list.name}</option>
							{/each}
						{/if}
					</select>
				</div>

				<div class="form-control">
					<Checkbox labelKey="editTask.deleteWhenDone" bind:value={isOneTime} />
				</div>

				<div class="form-control">
					<Checkbox labelKey="editTask.highPriority" bind:value={isHighPriority} />
				</div>

				{#if isInSharedList}
					<div class="form-control">
						<Checkbox labelKey="editTask.taskIsPrivate" bind:value={isPrivate} />

						<Tooltip key="privateTasks" application="To Do Assistant" />
					</div>

					{#if !isPrivate && assigneeOptions}
						<div class="assign-to-user">
							<div class="assign-to-user-header">{assignToUserLabel}</div>
							<div class="assign-to-user-content">
								<label class="radio" class:selected={!assignedToUserId}>
									<img src={nobodyImageUri} class="assign-to-user-image" alt="" />
									<div class="assign-to-user-item">
										<span>{$t('editTask.nobody')}</span>
										<input type="radio" name="assign" bind:group={assignedToUserId} value={null} />
									</div>
								</label>

								{#each assigneeOptions as assigneeOption}
									<label class="radio" class:selected={assignedToUserId === assigneeOption.id}>
										<img
											src={assigneeOption.imageUri}
											class="assign-to-user-image"
											title={assigneeOption.name}
											alt={$t('profilePicture', { name: assigneeOption.name })}
										/>
										<div class="assign-to-user-item">
											<span>{assigneeOption.name} <i class="fas fa-check"></i></span>
											<input type="radio" name="assign" bind:group={assignedToUserId} value={assigneeOption.id} />
										</div>
									</label>
								{/each}
							</div>
						</div>
					{/if}
				{/if}
			</form>

			<hr />

			{#if deleteInProgress && recipes !== null && recipes.length > 0}
				<div class="delete-confirmation-alert">
					<span contenteditable="false" bind:innerHTML={taskUsedAsIngredientText}></span>
					<br />
					<span contenteditable="false" bind:innerHTML={recipesText}></span>.
					<br />
					<span contenteditable="false" bind:innerHTML={deletingWillUnlinkText}></span>
				</div>
			{/if}

			<div class="save-delete-wrap">
				{#if !deleteInProgress}
					<button type="button" onclick={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{$t('save')}</span>
					</button>
				{/if}

				<button type="button" onclick={deleteTask} class="button danger-button" disabled={deleteButtonIsLoading} class:confirm={deleteInProgress}>
					<span class="button-loader" class:loading={deleteButtonIsLoading}>
						<i class="fas fa-circle-notch fa-spin"></i>
					</span>
					<span>{deleteButtonText}</span>
				</button>

				{#if deleteInProgress}
					<button type="button" onclick={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.form-control.url {
		position: relative;

		input {
			width: calc(100% - 74px);
			padding-right: 60px;
		}

		.clear-url-button {
			position: absolute;
			top: 1px;
			right: 0;
			background: none;
			border: none;
			outline: none;
			padding: 0 15px;
			line-height: 38px;
			text-decoration: none;
			color: var(--primary-color);
		}
	}

	.assign-to-user {
		.assign-to-user-header {
			background: var(--primary-color);
			border-top-left-radius: 8px;
			border-top-right-radius: 8px;
			padding: 8px 10px;
			text-align: center;
			color: #fff;
		}

		.assign-to-user-content {
			border: 1px solid #ddd;
			border-top: none;
			border-bottom-left-radius: 8px;
			border-bottom-right-radius: 8px;

			.assign-to-user-image {
				width: 34px;
				height: 34px;
				border-radius: 50%;
				margin: 6px 12px 0 0;
			}

			.assign-to-user-item {
				padding: 9px 0;
				line-height: 27px;
			}
		}

		input[type='radio'] {
			display: none;
		}

		.radio {
			display: flex;
			justify-content: flex-start;
			padding: 0 10px;
			margin-bottom: 8px;

			&:last-child {
				margin-bottom: 0;
			}

			&:hover {
				color: var(--primary-color);
			}

			&.selected {
				background: #f0f6ff;
				color: var(--primary-color-dark);

				i {
					display: inline;
				}
			}

			i {
				display: none;
				margin-left: 8px;
			}
		}
	}

	.delete-confirmation-alert {
		background: #fff;
		border: 1px solid #ddd;
		border-radius: 8px;
		margin-bottom: 25px;
		padding: 10px 15px;
		line-height: 28px;
		text-align: center;
		user-select: none;
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .assign-to-user .radio:hover {
		color: var(--regular-color);
	}

	@media screen and (min-width: 1200px) {
		.form-control.url .clear-url-button {
			line-height: 46px;
		}
	}
</style>
