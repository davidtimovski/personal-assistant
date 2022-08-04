<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { t } from '$lib/localization/i18n';
	import { SyncService } from '$lib/services/syncService';
	import { isOnline } from '$lib/stores';

	import AlertBlock from '$lib/components/AlertBlock.svelte';

	let syncButtonIsLoading = false;

	let syncService: SyncService;

	$: canSync = $isOnline && !syncButtonIsLoading;

	async function sync() {
		if (!canSync) {
			return;
		}

		syncButtonIsLoading = true;

		try {
			await syncService.totalSync();
		} catch {
			syncButtonIsLoading = false;
		}
	}

	function back() {
		window.history.back();
	}

	onMount(() => {
		syncService = new SyncService();
	});
</script>

<section>
	<div class="container">
		<div class="au-animate">
			<div class="page-title-wrap">
				<div class="side inactive medium">
					<i class="fas fa-cloud-download-alt" />
				</div>
				<div class="page-title">{$t('totalSync.totalSync')}</div>
				<a on:click={back} class="back-button" role="button">
					<i class="fas fa-times" />
				</a>
			</div>

			<div class="content-wrap">
				{#if !$isOnline}
					<AlertBlock type="warning" message={$t('totalSync.offlineText')} />
				{/if}

				<AlertBlock type="warning" message={$t('totalSync.syncText')} />

				<div class="save-delete-wrap">
					<a on:click={sync} class="button primary-button" class:disabled={!canSync} role="button">
						<span class="button-loader" class:loading={syncButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{$t('totalSync.sync')}</span>
					</a>
					<a href="/menu" class="button secondary-button">{$t('cancel')}</a>
				</div>
			</div>
		</div>
	</div>
</section>
