<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { syncStatus } from '$lib/stores';
	import { AccountsService } from '$lib/services/accountsService';
	import { AccountItem } from '$lib/models/viewmodels/accountItem';
	import { AppEvents } from '$lib/models/appEvents';

	let accounts: AccountItem[] | null = null;
	let sum = 0;
	let currency: string;
	let viewStocks = false;
	let someAreInvestmentFunds = false;
	let editedId: number | undefined;
	let editedId2: number | undefined;
	let syncing = false;

	let localStorage: LocalStorageUtil;
	let accountsService: AccountsService;

	function toggleViewStocks() {
		viewStocks = !viewStocks;
	}

	onMount(async () => {
		syncStatus.subscribe((value) => {
			if (value === AppEvents.SyncStarted) {
				syncing = true;
			} else if (value === AppEvents.SyncFinished) {
				syncing = false;
			}
		});

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		const edited2 = $page.url.searchParams.get('edited2');
		if (edited2) {
			editedId2 = parseInt(edited2, 10);
		}

		localStorage = new LocalStorageUtil();
		accountsService = new AccountsService();

		currency = localStorage.get('currency');

		const accountDtos = await accountsService.getAllWithBalance(currency);

		const accountItems = new Array<AccountItem>();
		for (const account of accountDtos) {
			if (!!account.stockPrice) {
				someAreInvestmentFunds = true;
			}

			accountItems.push(
				new AccountItem(
					account.id,
					account.name,
					account.currency,
					account.stockPrice,
					account.stocks,
					<number>account.balance,
					account.synced
				)
			);

			sum += <number>account.balance;
		}

		accounts = accountItems;
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-money-check-alt" />
		</div>
		<div class="page-title">{$t('accounts.accounts')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !accounts}
				<div class="double-circle-loading">
					<div class="double-bounce1" />
					<div class="double-bounce2" />
				</div>
			{:else}
				<div>
					<table class="editable-table accounts-table">
						{#if someAreInvestmentFunds}
							<thead>
								<tr>
									<th class="type-cell" />
									<th class="left-col">{$t('name')}</th>
									<th
										on:click={toggleViewStocks}
										class="right-col clickable-cell"
										role="button"
										title={$t('accounts.toggleStockPriceStocks')}
										aria-label={$t('accounts.toggleStockPriceStocks')}
									>
										<span>{$t(viewStocks ? 'accounts.stocks' : 'accounts.stockPrice')}</span>
									</th>
									<th class="right-col">{$t('balance')}</th>
									<th class="sync-icon-cell" />
								</tr>
							</thead>
						{/if}

						<tbody>
							{#each accounts as account}
								<tr class:highlighted-row={account.id === editedId || account.id === editedId2}>
									<td class="edit-link-cell">
										<a href="/editAccount/{account.id}" class="link" title={$t('edit')} aria-label={$t('edit')}>
											<i class="fas fa-pencil-alt" />
										</a>
									</td>
									<td class="left-col">{account.name}</td>
									{#if someAreInvestmentFunds}
										<td class="right-col">
											{#if viewStocks}
												<span>{account.stocks || ''}</span>
											{:else}
												<span>{account.stockPrice ? Formatter.money(account.stockPrice, currency, 4) : ''}</span>
											{/if}
										</td>
									{/if}
									<td class="right-col">{Formatter.money(account.balance, currency)}</td>
									<td class="sync-icon-cell">
										{#if !account.synced}
											<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')} />
										{/if}
									</td>
								</tr>
							{/each}
						</tbody>
						{#if accounts.length > 1}
							<tfoot>
								<tr>
									<td colspan="4">{Formatter.money(sum, currency)}</td>
								</tr>
							</tfoot>
						{/if}
					</table>
				</div>
			{/if}
		</div>

		<div class="centering-wrap">
			<button
				type="button"
				on:click={() => goto('/editAccount/0')}
				class="new-button"
				disabled={syncing}
				title={$t('accounts.newAccount')}
				aria-label={$t('accounts.newAccount')}
			>
				<i class="fas fa-plus" />
			</button>
		</div>

		{#if accounts && accounts.length > 0}
			<div>
				<hr />

				<a href="/transferFunds" class="wide-button">{$t('accounts.transferFunds')}</a>
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.accounts-table {
		.left-col {
			padding: 5px 15px 5px 0;
			text-align: left;
		}

		.right-col {
			text-align: right;
		}

		td {
			font-size: 1rem;
		}
	}
</style>
