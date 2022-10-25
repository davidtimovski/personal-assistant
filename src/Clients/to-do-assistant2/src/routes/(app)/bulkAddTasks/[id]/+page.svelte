<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../shared2/models/validationErrors';
	import Checkbox from '../../../../../../shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { alertState } from '$lib/stores';
	import { TasksService } from '$lib/services/tasksService';
	import { ListsService } from '$lib/services/listsService';

	export let data: PageData;

	let tasksText = '';
	let tasksAreOneTime = false;
	let tasksArePrivate = false;
	let listIsShared: boolean;
	let tasksTextIsInvalid: boolean;
	let tasksTextInput: HTMLTextAreaElement;
	let saveButtonIsLoading = false;

	const alertStateUnsub = alertState.subscribe((value) => {
		if (value.hidden) {
			tasksTextIsInvalid = false;
		}
	});

	let tasksService: TasksService;
	let listsService: ListsService;

	$: canSave = !ValidationUtil.isEmptyOrWhitespace(tasksText);

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(tasksText)) {
			result.fail('tasksText');
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
			tasksTextIsInvalid = false;

			try {
				await tasksService.bulkCreate(data.id, tasksText, tasksAreOneTime, tasksArePrivate);
				await listsService.getAll();

				alertState.update((x) => {
					x.showSuccess('bulkAddTasks.addSuccessful');
					return x;
				});
				goto(`/list/${data.id}`);
			} catch (e) {
				if (e instanceof ValidationErrors) {
					tasksTextIsInvalid = e.fields.includes('TasksText');
				}

				saveButtonIsLoading = false;
			}
		} else {
			tasksTextIsInvalid = true;
			saveButtonIsLoading = false;
		}
	}

	onMount(async () => {
		tasksService = new TasksService();
		listsService = new ListsService();

		listIsShared = await listsService.getIsShared(data.id);

		tasksTextInput.focus();
	});

	onDestroy(() => {
		alertStateUnsub();
		listsService?.release();
		tasksService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-list-ol" />
		</div>
		<div class="page-title">{$t('bulkAddTasks.bulkAddTasks')}</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form on:submit|preventDefault={save}>
			<div class="form-control">
				<textarea
					bind:value={tasksText}
					bind:this={tasksTextInput}
					class:invalid={tasksTextIsInvalid}
					placeholder={$t('bulkAddTasks.eachRow')}
					aria-label={$t('bulkAddTasks.eachRow')}
				/>
			</div>

			<div class="form-control">
				<Checkbox labelKey="deleteOnCompletion" bind:value={tasksAreOneTime} />
			</div>

			{#if listIsShared}
				<div class="form-control">
					<Checkbox labelKey="bulkAddTasks.tasksWillBePrivate" bind:value={tasksArePrivate} />
				</div>
			{/if}
		</form>

		<div class="save-delete-wrap">
			<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
				<span class="button-loader" class:loading={saveButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('bulkAddTasks.add')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>
