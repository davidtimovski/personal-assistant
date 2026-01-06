<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { slide } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../../Core/shared2/models/validationErrors';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';
	import Tooltip from '../../../../../../../Core/shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { user, alertState } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { UsersService } from '$lib/services/usersService';
	import { SharingState } from '$lib/models/viewmodels/sharingState';
	import { UpdateList } from '$lib/models/server/requests/updateList';
	import { UpdateSharedList } from '$lib/models/server/requests/updateSharedList';
	import { CreateList } from '$lib/models/server/requests/createList';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	const isNew = data.id === 0;

	let name = $state('');
	let icon = $state('');
	let tasksText = $state('');
	let notificationsEnabled: boolean | null = $state(null);
	let isOneTimeToggleDefault: boolean | null = $state(null);
	let isArchived = false;
	let sharingState: SharingState | null = $state(null);
	let nameIsInvalid = $state(false);
	let tasksTextIsInvalid = $state(false);
	let tasksInputIsVisible = $state(false);
	let confirmationInProgress = $state(false);
	let saveButtonText: string = $state('');
	let deleteButtonText: string = $state('');
	let leaveButtonText: string = $state('');
	let iconOptions = ListsService.getIconOptions();
	let nameInput: HTMLInputElement | null = $state(null);
	let saveButtonIsLoading = $state(false);
	let deleteButtonIsLoading = $state(false);
	let leaveButtonIsLoading = $state(false);
	let loading = $state(!isNew);

	const alertStateUnsub = alertState.subscribe((value) => {
		if (value.hidden) {
			nameIsInvalid = false;
		}
	});

	let listsService: ListsService;
	let usersService: UsersService;

	let canSave = $derived(!ValidationUtil.isEmptyOrWhitespace(name));

	let notificationsCheckboxEnabled = $derived(sharingState !== SharingState.NotShared && $user.toDoNotificationsEnabled);

	function showTasksTextarea() {
		tasksInputIsVisible = true;
	}

	function selectIcon(i: string) {
		icon = i;
	}

	async function save(event: Event) {
		if (notificationsEnabled === null || isOneTimeToggleDefault === null) {
			throw new Error('Data not initialized');
		}

		event.preventDefault();

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = ListsService.validateEdit(name);

		if (result.valid) {
			nameIsInvalid = false;

			if (!isNew) {
				try {
					if (sharingState === SharingState.Member) {
						await listsService.updateShared(new UpdateSharedList(data.id, notificationsEnabled));
					} else {
						await listsService.update(new UpdateList(data.id, name, icon, isOneTimeToggleDefault, notificationsEnabled));
					}

					const redirectRoute = isArchived ? '/archivedLists' : '/lists';
					await goto(redirectRoute + '?edited=' + data.id);
				} catch (e) {
					if (e instanceof ValidationErrors) {
						nameIsInvalid = e.fields.includes('Name');
					}

					saveButtonIsLoading = false;
				}
			} else {
				try {
					const newId = await listsService.create(new CreateList(name, icon, isOneTimeToggleDefault, tasksText));

					await goto('/lists?edited=' + newId);
				} catch (e) {
					if (e instanceof ValidationErrors) {
						nameIsInvalid = e.fields.includes('Name');
						tasksTextIsInvalid = e.fields.includes('TasksText');
					}

					saveButtonIsLoading = false;
				}
			}
		} else {
			nameIsInvalid = true;
			saveButtonIsLoading = false;
		}
	}

	async function deleteList() {
		if (confirmationInProgress) {
			deleteButtonIsLoading = true;

			await listsService.delete(data.id);

			alertState.update((x) => {
				x.showSuccess('editList.deleteSuccessful');
				return x;
			});
			goto('/lists');
		} else {
			deleteButtonText = $t('sure');
			confirmationInProgress = true;
		}
	}

	async function leaveList() {
		if (confirmationInProgress) {
			leaveButtonIsLoading = true;

			await listsService.leave(data.id);

			alertState.update((x) => {
				x.showSuccess('editList.youHaveLeftTheList');
				return x;
			});
			goto('/lists');
		} else {
			leaveButtonText = $t('sure');
			confirmationInProgress = true;
		}
	}

	async function cancel() {
		if (!confirmationInProgress) {
			back();
		}
		deleteButtonText = $t('delete');
		leaveButtonText = $t('editList.leave');
		confirmationInProgress = false;
	}

	function back() {
		if (isNew) {
			goto('/lists');
		} else {
			goto(`/list/${data.id}`);
		}
	}

	onMount(async () => {
		deleteButtonText = $t('delete');
		leaveButtonText = $t('editList.leave');

		listsService = new ListsService();
		usersService = new UsersService();

		if (isNew) {
			icon = iconOptions[0].icon;
			sharingState = SharingState.NotShared;
			saveButtonText = $t('editList.create');

			nameInput?.focus();
		} else {
			saveButtonText = $t('save');

			const model = await listsService.get(data.id);
			if (model === null) {
				throw new Error('List not found');
			}

			name = model.name;
			icon = model.icon;
			notificationsEnabled = model.notificationsEnabled;
			isOneTimeToggleDefault = model.isOneTimeToggleDefault;
			isArchived = model.isArchived;
			sharingState = model.sharingState;
			loading = false;
		}
	});

	onDestroy(() => {
		alertStateUnsub();
		listsService?.release();
		usersService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt"></i>
		</div>
		<div class="page-title">
			<span>
				{#if isNew}
					<span>{$t('editList.newList')}</span>
				{:else}
					<span>{$t('editList.edit')}</span>&nbsp;<span class="colored-text">{name}</span>
				{/if}
			</span>
		</div>
		<button type="button" onclick={back} class="back-button">
			<i class="fas fa-times"></i>
		</button>
	</div>

	<div class="content-wrap">
		{#if loading}
			<div class="double-circle-loading">
				<div class="double-bounce1"></div>
				<div class="double-bounce2"></div>
			</div>
		{:else}
			{#if sharingState === 4}
				<AlertBlock type="info" message={$t('editList.forAccessTo')} />
			{/if}

			<form onsubmit={save}>
				{#if sharingState !== 4}
					<div class="form-control">
						<input
							type="text"
							bind:value={name}
							bind:this={nameInput}
							maxlength="50"
							class:invalid={nameIsInvalid}
							placeholder={$t('listName')}
							aria-label={$t('listName')}
							required
						/>
					</div>
				{/if}

				{#if isNew}
					<div class="form-control">
						{#if !tasksInputIsVisible}
							<div class="horizontal-buttons-wrap">
								<button type="button" onclick={showTasksTextarea} class="wide-button">
									{$t('editList.addTasks')}
								</button>
							</div>
						{:else}
							<textarea
								bind:value={tasksText}
								class:invalid={tasksTextIsInvalid}
								in:slide
								placeholder={$t('editList.eachRow')}
								aria-label={$t('editList.eachRow')}
							></textarea>
						{/if}
					</div>
				{/if}

				{#if sharingState !== 4}
					<div class="form-control">
						<div class="icon-wrap">
							<span class="placeholder">{$t('editList.icon')}</span>
							<div class="icon-options">
								{#each iconOptions as i}
									<button type="button" onclick={() => selectIcon(i.icon)} class:selected={icon === i.icon} class="icon-option">
										<i class={i.cssClass}></i>
									</button>
								{/each}
							</div>
						</div>
					</div>
				{/if}

				<div class="form-control">
					<Checkbox labelKey="editList.notifications" bind:value={notificationsEnabled} disabled={!notificationsCheckboxEnabled} />
				</div>

				{#if sharingState !== 4}
					<div class="form-control">
						<Checkbox labelKey="deleteOnCompletion" bind:value={isOneTimeToggleDefault} />

						<Tooltip key="oneTimeTasks" application="To Do Assistant" />
					</div>
				{/if}
			</form>

			<hr />

			<div class="save-delete-wrap">
				{#if !confirmationInProgress}
					<button type="button" onclick={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if (!isNew && sharingState === 0) || sharingState === 1 || sharingState === 2}
					<button
						type="button"
						onclick={deleteList}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={confirmationInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{deleteButtonText}</span>
					</button>
				{/if}

				{#if sharingState === 3 || sharingState === 4}
					<button
						type="button"
						onclick={leaveList}
						class="button danger-button"
						disabled={leaveButtonIsLoading}
						class:confirm={confirmationInProgress}
					>
						<span class="button-loader" class:loading={leaveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{leaveButtonText}</span>
					</button>
				{/if}

				{#if isNew || confirmationInProgress}
					<button type="button" onclick={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		{/if}
	</div>
</section>
