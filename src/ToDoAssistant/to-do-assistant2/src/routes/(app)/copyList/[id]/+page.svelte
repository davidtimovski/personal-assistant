<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../../Core/shared2/models/validationErrors';

	import { t } from '$lib/localization/i18n';
	import { alertState, localState } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import type { Task } from '$lib/models/entities';
	import { CopyList } from '$lib/models/server/requests/copyList';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let name = $state('');
	let icon = $state('');
	let tasks: Task[];
	let nameIsInvalid = $state(false);
	const iconOptions = ListsService.getIconOptions();
	let saveButtonIsLoading = $state(false);
	let copyAsTextCompleted = $state(false);
	let originalListName: string;
	const unsubscriptions: Unsubscriber[] = [];

	unsubscriptions.push(
		alertState.subscribe((value) => {
			if (value.hidden) {
				nameIsInvalid = false;
			}
		})
	);

	let listsService: ListsService;

	let canSave = $derived(!ValidationUtil.isEmptyOrWhitespace(name));

	function selectIcon(i: string) {
		icon = i;
	}

	function copyAsText() {
		listsService.copyAsText(originalListName, tasks);

		copyAsTextCompleted = true;
		window.setTimeout(() => {
			copyAsTextCompleted = false;
		}, 1500);
	}

	async function save(event: Event) {
		event.preventDefault();

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = ListsService.validateCopy(name);

		if (result.valid) {
			nameIsInvalid = false;

			try {
				const newId = await listsService.copy(new CopyList(data.id, name, icon));

				alertState.update((x) => {
					x.showSuccess('copyList.copySuccessful');
					return x;
				});
				await goto('/lists?edited=' + newId);
			} catch (e) {
				if (e instanceof ValidationErrors) {
					nameIsInvalid = e.fields.includes('Name');
				}

				saveButtonIsLoading = false;
			}
		} else {
			nameIsInvalid = true;
			saveButtonIsLoading = false;
		}
	}

	onMount(async () => {
		listsService = new ListsService();

		unsubscriptions.push(
			localState.subscribe((s) => {
				if (s.lists === null) {
					return;
				}

				const list = s.lists.find((x) => x.id === data.id);
				if (!list) {
					throw new Error('List not found');
				}

				name = ($t('copyList.copyOf') + ' ' + list.name).substring(0, 50);
				icon = <string>list.icon;
				tasks = list.tasks;
				originalListName = <string>list.name;
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		listsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-copy"></i>
		</div>
		<div class="page-title">{name}</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<form onsubmit={save}>
			<div class="form-control">
				<input
					type="text"
					bind:value={name}
					maxlength="50"
					class:invalid={nameIsInvalid}
					placeholder={$t('listName')}
					aria-label={$t('listName')}
					required
				/>
			</div>

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
		</form>

		<div class="save-delete-wrap">
			<button type="button" onclick={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
				<span class="button-loader" class:loading={saveButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin"></i>
				</span>
				<span>{$t('copyList.createCopy')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<button type="button" onclick={copyAsText} class="wide-button with-badge">
				<span>{$t('copyList.copyAsText')}</span>
				<i class="fas fa-check badge toggled" class:visible={copyAsTextCompleted}></i>
			</button>
		</div>
	</div>
</section>
