<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user, syncStatus } from '$lib/stores';
	import { AccountsService } from '$lib/services/accountsService';
	import { AccountItem } from '$lib/models/viewmodels/accountItem';
	import { SyncEvents } from '$lib/models/syncStatus';

	let accounts: AccountItem[] | null = $state(null);
	let sum = $state(0);
	let currency: string | null = $state(null);
	let viewStocks = $state(false);
	let someAreInvestmentFunds = $state(false);
	let editedId: number | undefined = $state(undefined);
	let editedId2: number | undefined = $state(undefined);

	const localStorage = new LocalStorageUtil();
	let accountsService: AccountsService;

	function toggleViewStocks() {
		viewStocks = !viewStocks;
	}

	onMount(async () => {
		const url = new URL(window.location.href);
		const queryParams = new URLSearchParams(url.search);
		const edited = queryParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		const edited2 = queryParams.get('edited2');
		if (edited2) {
			editedId2 = parseInt(edited2, 10);
		}

		accountsService = new AccountsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const accountDtos = await accountsService.getAllWithBalance(currency);

		const accountItems = new Array<AccountItem>();
		for (const account of accountDtos) {
			if (!!account.stockPrice) {
				someAreInvestmentFunds = true;
			}

			accountItems.push(
				new AccountItem(account.id, account.name, account.currency, account.stockPrice, account.stocks, <number>account.balance, account.synced)
			);

			sum += <number>account.balance;
		}

		accounts = accountItems;
	});

	onDestroy(() => {
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-money-check-alt"></i>
		</div>
		<div class="page-title">{$t('accounts.accounts')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !accounts}
				<div class="double-circle-loading">
					<div class="double-bounce1"></div>
					<div class="double-bounce2"></div>
				</div>
			{:else}
				<div>
					<table class="editable-table accounts-table">
						<thead>
							<tr>
								<th class="type-cell"></th>
								<th class="left-col">{$t('name')}</th>
								{#if someAreInvestmentFunds}
									<th
										onclick={toggleViewStocks}
										class="right-col clickable-cell"
										role="button"
										title={$t('accounts.toggleStockPriceStocks')}
										aria-label={$t('accounts.toggleStockPriceStocks')}
									>
										<span>{$t(viewStocks ? 'accounts.stocks' : 'accounts.stockPrice')}</span>
									</th>
								{/if}

								<th class="right-col">{$t('balance')}</th>
								<th class="sync-icon-cell"></th>
							</tr>
						</thead>

						<tbody>
							{#each accounts as account}
								<tr class:highlighted-row={account.id === editedId || account.id === editedId2}>
									<td class="edit-link-cell">
										<a href="/editAccount/{account.id}" class="link" title={$t('edit')} aria-label={$t('edit')}>
											<i class="fas fa-pencil-alt"></i>
										</a>
									</td>
									<td class="left-col">{account.name}</td>
									{#if someAreInvestmentFunds}
										<td class="right-col">
											{#if viewStocks}
												<span>{Formatter.number(account.stocks, $user.culture)}</span>
											{:else}
												<span>{account.stockPrice ? Formatter.moneyPrecise(account.stockPrice, account.currency, $user.culture, 4) : ''}</span>
											{/if}
										</td>
									{/if}
									<td class="right-col">{Formatter.money(account.balance, currency, $user.culture)}</td>
									<td class="sync-icon-cell">
										{#if !account.synced}
											<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')}></i>
										{/if}
									</td>
								</tr>
							{/each}
						</tbody>
						{#if accounts.length > 1}
							<tfoot>
								<tr>
									<td colspan="4">{Formatter.money(sum, currency, $user.culture)}</td>
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
				onclick={() => goto('/editAccount/0')}
				class="new-button"
				disabled={$syncStatus.status === SyncEvents.SyncStarted}
				title={$t('accounts.newAccount')}
				aria-label={$t('accounts.newAccount')}
			>
				<i class="fas fa-plus"></i>
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
