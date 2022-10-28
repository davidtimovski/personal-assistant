<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { t } from '$lib/localization/i18n';
	import { alertState, lists } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { SharingState } from '$lib/models/viewmodels/sharingState';

	export let data: PageData;

	let name = '';
	let uncompleteText = '';
	let uncompleteButtonIsLoading = false;
	const unsubscriptions: Unsubscriber[] = [];

	let listsService: ListsService;

	async function uncomplete() {
		uncompleteButtonIsLoading = true;

		try {
			await listsService.uncompleteAllTasks(data.id);

			alertState.update((x) => {
				x.showSuccess('uncompleteTasks.uncompleteTasksSuccessful');
				return x;
			});
			goto(`/list/${data.id}`);
		} catch {
			uncompleteButtonIsLoading = false;
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

				name = <string>list.name;
				uncompleteText = $t(
					list.sharingState !== SharingState.NotShared
						? 'uncompleteTasks.thisWillUncompleteShared'
						: 'uncompleteTasks.thisWillUncomplete'
				);
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
		<div class="side inactive">
			<i class="fas fa-check-circle" />
		</div>
		<div class="page-title">
			<span>{$t('uncompleteTasks.uncompleteTasksIn')}</span>&nbsp;<span class="colored-text">{name}</span>?
		</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="text-wrap">{uncompleteText}</div>

		<div class="save-delete-wrap">
			<button type="button" on:click={uncomplete} class="button primary-button" disabled={uncompleteButtonIsLoading}>
				<span class="button-loader" class:loading={uncompleteButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('uncompleteTasks.uncomplete')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>
