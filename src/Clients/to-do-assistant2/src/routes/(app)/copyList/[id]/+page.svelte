<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../shared2/models/validationErrors';

	import { t } from '$lib/localization/i18n';
	import { alertState, lists } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import type { Task } from '$lib/models/entities';

	export let data: PageData;

	let name = '';
	let icon = '';
	let tasks: Task[];
	let nameIsInvalid: boolean;
	const iconOptions = ListsService.getIconOptions();
	let saveButtonIsLoading = false;
	let copyAsTextCompleted = false;
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

	$: canSave = !ValidationUtil.isEmptyOrWhitespace(name);

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

	function validate(): ValidationResult {
		const result = new ValidationResult(true);

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

			try {
				const newId = await listsService.copy(data.id, name, icon);

				alertState.update((x) => {
					x.showSuccess('copyList.copySuccessful');
					return x;
				});
				await goto('/?edited=' + newId);
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
			lists.subscribe((l) => {
				if (l.length === 0) {
					return;
				}

				const list = l.find((x) => x.id === data.id);
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
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-copy" />
		</div>
		<div class="page-title">{name}</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form on:submit|preventDefault={save}>
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
							<div on:click={() => selectIcon(i.icon)} class:selected={icon === i.icon} class="icon-option">
								<i class={i.cssClass} />
							</div>
						{/each}
					</div>
				</div>
			</div>
		</form>

		<div class="save-delete-wrap">
			<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
				<span class="button-loader" class:loading={saveButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('copyList.createCopy')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<button type="button" on:click={copyAsText} class="wide-button with-badge">
				<span>{$t('copyList.copyAsText')}</span>
				<i class="fas fa-check badge toggled" class:visible={copyAsTextCompleted} />
			</button>
		</div>
	</div>
</section>
