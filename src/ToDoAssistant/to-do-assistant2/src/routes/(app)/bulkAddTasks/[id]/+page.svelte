<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../../Core/shared2/models/validationErrors';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { alertState } from '$lib/stores';
	import { TasksService } from '$lib/services/tasksService';
	import { ListsService } from '$lib/services/listsService';
	import { BulkCreate } from '$lib/models/server/requests/bulkCreate';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let tasksText = $state('');
	let tasksAreOneTime = $state(false);
	let tasksArePrivate = $state(false);
	let listIsShared: boolean | null = $state(null);
	let tasksTextIsInvalid: boolean | null = $state(null);
	let tasksTextInput: HTMLTextAreaElement;
	let saveButtonIsLoading = $state(false);

	const alertStateUnsub = alertState.subscribe((value) => {
		if (value.hidden) {
			tasksTextIsInvalid = false;
		}
	});

	let tasksService: TasksService;
	let listsService: ListsService;

	let canSave = $derived(!ValidationUtil.isEmptyOrWhitespace(tasksText));

	async function save(event: Event) {
		event.preventDefault();

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = TasksService.validateBulkCreate(tasksText);

		if (result.valid) {
			tasksTextIsInvalid = false;

			try {
				await tasksService.bulkCreate(new BulkCreate(data.id, tasksText, tasksAreOneTime, tasksArePrivate));
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
			<i class="fas fa-list-ol"></i>
		</div>
		<div class="page-title">{$t('bulkAddTasks.bulkAddTasks')}</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<form onsubmit={save}>
			<div class="form-control">
				<textarea
					bind:value={tasksText}
					bind:this={tasksTextInput}
					class:invalid={tasksTextIsInvalid}
					placeholder={$t('bulkAddTasks.eachRow')}
					aria-label={$t('bulkAddTasks.eachRow')}
				></textarea>
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
			<button type="button" onclick={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
				<span class="button-loader" class:loading={saveButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin"></i>
				</span>
				<span>{$t('bulkAddTasks.add')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>
