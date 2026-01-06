<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { t } from '$lib/localization/i18n';
	import { alertState, localState } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { SharingState } from '$lib/models/viewmodels/sharingState';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let name = $state('');
	let uncompleteText = $state('');
	let uncompleteButtonIsLoading = $state(false);

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
			localState.subscribe((s) => {
				if (s.lists === null) {
					return;
				}

				const list = s.lists.find((x) => x.id === data.id);
				if (!list) {
					throw new Error('List not found');
				}

				name = <string>list.name;
				uncompleteText = $t(
					list.sharingState !== SharingState.NotShared ? 'uncompleteTasks.thisWillUncompleteShared' : 'uncompleteTasks.thisWillUncomplete'
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
			<i class="fas fa-check-circle"></i>
		</div>
		<div class="page-title">
			<span><span>{$t('uncompleteTasks.uncompleteTasksIn')}</span>&nbsp;<span class="colored-text">{name}</span>?</span>
		</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div class="text-wrap">{uncompleteText}</div>

		<div class="save-delete-wrap">
			<button type="button" onclick={uncomplete} class="button primary-button" disabled={uncompleteButtonIsLoading}>
				<span class="button-loader" class:loading={uncompleteButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin"></i>
				</span>
				<span>{$t('uncompleteTasks.uncomplete')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>
