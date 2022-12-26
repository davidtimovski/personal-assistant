<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { t } from '$lib/localization/i18n';
	import { alertState, state } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { SharingState } from '$lib/models/viewmodels/sharingState';

	export let data: PageData;

	let name = '';
	let archivingAListText = '';
	let archiveButtonIsLoading = false;
	const unsubscriptions: Unsubscriber[] = [];

	let listsService: ListsService;

	async function archive() {
		archiveButtonIsLoading = true;

		try {
			await listsService.setIsArchived(data.id, true);

			alertState.update((x) => {
				x.showSuccess('archiveList.archiveSuccessful');
				return x;
			});
			goto('/lists');
		} catch {
			archiveButtonIsLoading = false;
		}
	}

	onMount(async () => {
		listsService = new ListsService();

		unsubscriptions.push(
			state.subscribe((s) => {
				if (s.lists === null) {
					return;
				}

				const list = s.lists.find((x) => x.id === data.id);
				if (!list) {
					throw new Error('List not found');
				}

				name = <string>list.name;
				archivingAListText = $t(
					list.sharingState === SharingState.NotShared
						? 'archiveList.archivingAList'
						: 'archiveList.archivingAListShared'
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
		<div class="side inactive small">
			<i class="fas fa-archive" />
		</div>
		<div class="page-title">
			<span>{$t('archiveList.archive')}</span>&nbsp;<span class="colored-text">{name}</span>?
		</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="text-wrap">{archivingAListText}</div>

		<div class="save-delete-wrap">
			<button type="button" on:click={archive} class="button primary-button" disabled={archiveButtonIsLoading}>
				<span class="button-loader" class:loading={archiveButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('archiveList.archive')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>
