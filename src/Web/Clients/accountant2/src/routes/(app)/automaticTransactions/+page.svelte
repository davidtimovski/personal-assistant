<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import EmptyListMessage from '../../../../../shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { Formatter } from '$lib/utils/formatter';
	import { user, syncStatus } from '$lib/stores';
	import { AutomaticTransactionsService } from '$lib/services/automaticTransactionsService';
	import { AutomaticTransactionItem } from '$lib/models/viewmodels/automaticTransactionItem';
	import { SyncEvents } from '$lib/models/syncStatus';

	let automaticTransactions: AutomaticTransactionItem[] | null = null;
	let editedId: number | undefined;

	let automaticTransactionsService: AutomaticTransactionsService;

	onMount(async () => {
		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		automaticTransactionsService = new AutomaticTransactionsService();

		const allAutomaticTransactions = await automaticTransactionsService.getAll();

		const automaticTransactionItems = new Array<AutomaticTransactionItem>();
		for (const automaticTransaction of allAutomaticTransactions) {
			automaticTransactionItems.push(
				new AutomaticTransactionItem(
					automaticTransaction.id,
					automaticTransaction.isDeposit,
					automaticTransaction.amount,
					automaticTransaction.currency,
					automaticTransaction.categoryName || $t('uncategorized'),
					$t(`dayOrdinal${automaticTransaction.dayInMonth}`),
					automaticTransaction.synced
				)
			);
		}

		automaticTransactions = automaticTransactionItems;
	});

	onDestroy(() => {
		automaticTransactionsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fa-solid fa-robot" />
		</div>
		<div class="page-title">{$t('automaticTransactions.automaticTransactions')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !automaticTransactions}
				<div class="double-circle-loading">
					<div class="double-bounce1" />
					<div class="double-bounce2" />
				</div>
			{:else if automaticTransactions.length > 0}
				<table class="editable-table">
					<thead>
						<tr>
							<th class="edit-link-cell" />
							<th>{$t('automaticTransactions.onEvery')}</th>
							<th>{$t('amount')}</th>
							<th>{$t('category')}</th>
							<th class="sync-icon-cell" />
						</tr>
					</thead>
					<tbody>
						{#each automaticTransactions as automaticTransaction}
							<tr class:highlighted-row={automaticTransaction.id === editedId}>
								<td class="edit-link-cell">
									<a
										href="/editAutomaticTransaction/{automaticTransaction.id}"
										class="link"
										title={$t('edit')}
										aria-label={$t('edit')}
									>
										<i class="fas fa-pencil-alt" />
									</a>
								</td>
								<td>{automaticTransaction.dayInMonth}</td>
								<td>
									{Formatter.money(automaticTransaction.amount, automaticTransaction.currency, $user.culture)}
									<i
										class="fas deposit-or-expense-icon {automaticTransaction.isDeposit
											? 'fa-donate deposit-color'
											: 'fa-wallet expense-color'}"
									/>
								</td>
								<td>{automaticTransaction.category}</td>
								<td class="sync-icon-cell">
									{#if !automaticTransaction.synced}
										<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')} />
									{/if}
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			{:else}
				<EmptyListMessage messageKey="automaticTransactions.emptyListMessage" />
			{/if}
		</div>

		<div class="centering-wrap">
			<button
				type="button"
				on:click={() => goto('/editAutomaticTransaction/0')}
				class="new-button"
				disabled={$syncStatus.status === SyncEvents.SyncStarted}
				title={$t('automaticTransactions.newAutomaticTransaction')}
				aria-label={$t('automaticTransactions.newAutomaticTransaction')}
			>
				<i class="fas fa-plus" />
			</button>
		</div>
	</div>
</section>

<style lang="scss">
	.deposit-or-expense-icon {
		margin-left: 5px;
	}
</style>
