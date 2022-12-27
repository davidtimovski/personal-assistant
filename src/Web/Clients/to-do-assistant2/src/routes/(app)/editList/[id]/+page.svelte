<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { slide } from 'svelte/transition';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../shared2/models/validationErrors';
	import AlertBlock from '../../../../../../shared2/components/AlertBlock.svelte';
	import Checkbox from '../../../../../../shared2/components/Checkbox.svelte';
	import Tooltip from '../../../../../../shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { user, alertState } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { UsersService } from '$lib/services/usersService';
	import { SharingState } from '$lib/models/viewmodels/sharingState';

	export let data: PageData;

	const isNew = data.id === 0;

	let name = '';
	let icon = '';
	let tasksText = '';
	let notificationsEnabled: boolean;
	let isOneTimeToggleDefault: boolean;
	let isArchived = false;
	let sharingState: SharingState;
	let nameIsInvalid = false;
	let tasksTextIsInvalid = false;
	let tasksInputIsVisible = false;
	let confirmationInProgress = false;
	let saveButtonText: string;
	let deleteButtonText: string;
	let leaveButtonText: string;
	let iconOptions = ListsService.getIconOptions();
	let nameInput: HTMLInputElement;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;
	let leaveButtonIsLoading = false;
	let loading = !isNew;

	const alertStateUnsub = alertState.subscribe((value) => {
		if (value.hidden) {
			nameIsInvalid = false;
		}
	});

	let listsService: ListsService;
	let usersService: UsersService;

	$: canSave = !ValidationUtil.isEmptyOrWhitespace(name);

	$: notificationsCheckboxEnabled = sharingState !== SharingState.NotShared && $user.toDoNotificationsEnabled;

	function showTasksTextarea() {
		tasksInputIsVisible = true;
	}

	function selectIcon(i: string) {
		icon = i;
	}

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(name)) {
			result.fail('name');
		}

		return result;
	}

	async function save() {
		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();

		if (result.valid) {
			nameIsInvalid = false;

			if (!isNew) {
				try {
					if (sharingState === SharingState.Member) {
						await listsService.updateShared(data.id, notificationsEnabled);
					} else {
						await listsService.update(
							data.id,
							name,
							icon,
							tasksText,
							notificationsEnabled,
							isOneTimeToggleDefault,
							isArchived,
							sharingState
						);
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
					const newId = await listsService.create(name, icon, isOneTimeToggleDefault, tasksText);

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
			if (isNew) {
				await goto('/lists');
			} else {
				await goto(`/list/${data.id}`);
			}
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

			nameInput.focus();
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
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			{#if isNew}
				<span>{$t('editList.newList')}</span>
			{:else}
				<span>{$t('editList.edit')}</span>&nbsp;<span class="colored-text">{name}</span>
			{/if}
		</div>
		<button type="button" on:click={back} class="back-button">
			<i class="fas fa-times" />
		</button>
	</div>

	<div class="content-wrap">
		{#if loading}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else}
			{#if sharingState === 4}
				<AlertBlock type="info" message={$t('editList.forAccessTo')} />
			{/if}

			<form on:submit|preventDefault={save}>
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
								<button type="button" on:click={showTasksTextarea} class="wide-button">
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
							/>
						{/if}
					</div>
				{/if}

				{#if sharingState !== 4}
					<div class="form-control">
						<div class="icon-wrap">
							<span class="placeholder">{$t('editList.icon')}</span>
							<div class="icon-options">
								{#each iconOptions as i}
									<button
										type="button"
										on:click={() => selectIcon(i.icon)}
										class:selected={icon === i.icon}
										class="icon-option"
									>
										<i class={i.cssClass} />
									</button>
								{/each}
							</div>
						</div>
					</div>
				{/if}

				<div class="form-control">
					<Checkbox
						labelKey="editList.notifications"
						bind:value={notificationsEnabled}
						disabled={!notificationsCheckboxEnabled}
					/>
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
					<button
						type="button"
						on:click={save}
						class="button primary-button"
						disabled={!canSave || saveButtonIsLoading}
					>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if (!isNew && sharingState === 0) || sharingState === 1 || sharingState === 2}
					<button
						type="button"
						on:click={deleteList}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={confirmationInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{deleteButtonText}</span>
					</button>
				{/if}

				{#if sharingState === 3 || sharingState === 4}
					<button
						type="button"
						on:click={leaveList}
						class="button danger-button"
						disabled={leaveButtonIsLoading}
						class:confirm={confirmationInProgress}
					>
						<span class="button-loader" class:loading={leaveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{leaveButtonText}</span>
					</button>
				{/if}

				{#if isNew || confirmationInProgress}
					<button type="button" on:click={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		{/if}
	</div>
</section>
