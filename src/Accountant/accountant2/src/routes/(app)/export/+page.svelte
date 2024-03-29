<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../../../Core/shared2/utils/dateHelper';
	import AlertBlock from '../../../../../../Core/shared2/components/AlertBlock.svelte';

	import { t } from '$lib/localization/i18n';
	import { isOnline } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';

	let synced: boolean;
	let exportText: string;
	let exportButtonIsLoading = false;

	let transactionsService: TransactionsService;

	$: canExport = isOnline && !exportButtonIsLoading;

	async function exportTransactions() {
		exportButtonIsLoading = true;

		const fileId = generateGuid();
		const fileName = await transactionsService.export(fileId);
		const date = DateHelper.format(new Date());

		const a = document.createElement('a');
		a.style.display = 'none';
		document.body.appendChild(a);
		a.href = window.URL.createObjectURL(fileName);
		a.setAttribute('download', `${$t('export.transactions')}-${date}.csv`);
		a.click();

		window.URL.revokeObjectURL(a.href);
		document.body.removeChild(a);

		transactionsService.deleteExportedFile(fileId);

		await goto('/dashboard');
	}

	function generateGuid(): string {
		return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
			const random = (Math.random() * 16) | 0,
				v = c == 'x' ? random : (random & 0x3) | 0x8;
			return v.toString(16);
		});
	}

	onMount(async () => {
		exportText = $t('export.exportText');

		transactionsService = new TransactionsService();
	});

	onDestroy(() => {
		transactionsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-download" />
		</div>
		<div class="page-title">{$t('export.export')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !$isOnline && synced}
			<AlertBlock type="warning" message={$t('export.offlineText')} />
		{/if}

		<div contenteditable="false" bind:innerHTML={exportText} class="text-wrap" />

		<div class="save-delete-wrap">
			<button type="button" on:click={exportTransactions} class="button primary-button" disabled={!canExport}>
				<span class="button-loader" class:loading={exportButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('export.export')}</span>
			</button>
			<a href="/menu" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>
