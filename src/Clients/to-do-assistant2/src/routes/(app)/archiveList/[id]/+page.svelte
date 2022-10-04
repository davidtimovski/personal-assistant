<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { t } from '$lib/localization/i18n';
	import { alertState, lists } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { SharingState } from '$lib/models/viewmodels/sharingState';

	export let data: PageData;

	let name = '';
	let archiveButtonIsLoading = false;
	let archivingAListText = '';

	let listsService: ListsService;

	async function archive() {
		archiveButtonIsLoading = true;

		try {
			await listsService.setIsArchived(data.id, true);

			alertState.update((x) => {
				x.showSuccess('archiveList.archiveSuccessful');
				return x;
			});
			goto('/');
		} catch {
			archiveButtonIsLoading = false;
		}
	}

	onMount(async () => {
		listsService = new ListsService();

		return lists.subscribe((l) => {
			if (l.length === 0) {
				return;
			}

			const list = l.find((x) => x.id === data.id);
			if (!list) {
				throw new Error('List not found');
			}

			name = <string>list.name;
			archivingAListText = $t(
				list.sharingState === SharingState.NotShared ? 'archiveList.archivingAList' : 'archiveList.archivingAListShared'
			);
		});
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-archive" />
		</div>
		<div class="page-title">
			<span><span>{$t('archiveList.archive')}</span>&nbsp;<span class="colored-text">{name}</span>?</span>
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
