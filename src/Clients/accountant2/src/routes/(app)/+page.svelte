<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';

	import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';
	import { DateHelper } from '../../../../shared2/utils/dateHelper';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { t } from '$lib/localization/i18n';
	import { CapitalService } from '$lib/services/capitalService';
	import { Formatter } from '$lib/utils/formatter';
	import { locale, authInfo, syncStatus, searchFilters } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';
	import { SearchFilters } from '$lib/models/viewmodels/searchFilters';
	import type { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { HomePageData } from '$lib/models/viewmodels/homePage';
	import { AccountsService } from '$lib/services/accountsService';

	let imageUri: any;
	let data = new HomePageData();
	let currency: string;
	let connTracker = {
		isOnline: true
	};
	let dataLoaded = false;
	const unsubscriptions: Unsubscriber[] = [];

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersServiceBase;
	let capitalService: CapitalService;
	let accountsService: AccountsService;

	async function getCapital(showUpcomingExpenses: boolean, showDebt: boolean) {
		const mainAccountId = await accountsService.getMainId();

		const balancePromise = accountsService.getBalance(mainAccountId, currency).then((result) => {
			data.balance = result;
			return data.balance;
		});

		const expendituresPromise = capitalService.getSpent(mainAccountId, $t('uncategorized'), currency).then((result) => {
			data.spent = result[1];
			data.expenditures = result[0];
		});

		const upcomingExpensesPromise = new Promise<number>(async (resolve) => {
			if (!showUpcomingExpenses) {
				resolve(0);
				return;
			}

			const result = await capitalService.getUpcomingExpenses($t('uncategorized'), currency);
			data.upcomingSum = result[1];
			data.upcomingExpenses = result[0];
			resolve(data.upcomingSum);
		});

		const numbersPromise = Promise.all([balancePromise, upcomingExpensesPromise]).then((result) => {
			data.available = result[0] - result[1];
		});

		const debtPromise = new Promise<void>(async (resolve) => {
			if (!showDebt) {
				resolve();
				return;
			}

			data.debt = await capitalService.getDebt(currency, $t('index.combined'));
			resolve();
		});

		await Promise.all([numbersPromise, expendituresPromise, debtPromise]);

		finishProgressBar();
		dataLoaded = true;
		localStorage.set('homePageData', JSON.stringify(data));
	}

	function sync() {
		syncStatus.set(AppEvents.ReSync);

		usersService.getProfileImageUri().then((uri: string) => {
			if (imageUri !== uri) {
				imageUri = uri;
			}
		});
	}

	function goToTransactions(expenditure: AmountByCategory) {
		const from = new Date();
		from.setDate(1);
		const fromDate = DateHelper.format(from);

		const toDate = DateHelper.format(new Date());

		searchFilters.set(new SearchFilters(1, 15, fromDate, toDate, 0, expenditure.categoryId, TransactionType.Any, null));

		goto('/transactions');
	}

	function startProgressBar() {
		dataLoaded = false;
		progressBarActive = true;
		progress = 10;

		progressIntervalId = window.setInterval(() => {
			if (progress < 85) {
				progress += 15;
			} else if (progressIntervalId) {
				window.clearInterval(progressIntervalId);
			}
		}, 500);

		progressBarVisible = true;
	}

	function finishProgressBar() {
		window.setTimeout(() => {
			progress = 100;
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	onMount(() => {
		localStorage = new LocalStorageUtil();
		usersService = new UsersServiceBase('Accountant');
		capitalService = new CapitalService();
		accountsService = new AccountsService();

		let cache = localStorage.getObject<HomePageData>('homePageData');
		if (cache) {
			data = cache;
		}

		const showUpcomingExpenses = localStorage.getBool(LocalStorageKeys.ShowUpcomingExpensesOnHomePage);
		const showDebt = localStorage.getBool(LocalStorageKeys.ShowDebtOnHomePage);
		currency = localStorage.get(LocalStorageKeys.Currency);

		unsubscriptions.push(
			authInfo.subscribe((value) => {
				if (!value) {
					return;
				}

				if (usersService.profileImageUriIsStale()) {
					usersService.getProfileImageUri().then((uri: string) => {
						imageUri = uri;
					});
				} else {
					imageUri = localStorage.get('profileImageUri');
				}
			})
		);

		unsubscriptions.push(
			syncStatus.subscribe((value) => {
				if (value === AppEvents.SyncStarted) {
					startProgressBar();
				} else if (value === AppEvents.SyncFinished) {
					getCapital(showUpcomingExpenses, showDebt);
				}
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		usersService?.release();
		capitalService?.release();
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			<a href="/menu" class="profile-image-container" title={$t('index.menu')} aria-label={$t('index.menu')}>
				<img src={imageUri} class="profile-image" width="40" height="40" alt={$t('profilePicture')} />
			</a>

			<div class="page-title reduced">
				<span />
			</div>
			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={!connTracker.isOnline || progressBarActive}
				title={$t('index.refresh')}
				aria-label={$t('index.refresh')}
			>
				<i class="fas fa-sync-alt" />
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {progress}%;" />
		</div>
	</div>

	<div class="content-wrap">
		<div class="capital-summary">
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('index.available')}</div>
					<div class="summary-value">{Formatter.number(data.available, currency, $locale)}</div>
				</div>
			</a>
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('index.spent')}</div>
					<div class="summary-value">{Formatter.number(data.spent, currency, $locale)}</div>
				</div>
			</a>
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('balance')}</div>
					<div class="summary-value">{Formatter.number(data.balance, currency, $locale)}</div>
				</div>
			</a>
		</div>

		<div class="home-buttons">
			<a href="newTransaction/1" class="home-button">
				{$t('index.newDeposit')}
			</a>
			<a href="newTransaction/0" class="home-button">
				{$t('index.newExpense')}
			</a>
		</div>

		{#if data.expenditures && data.expenditures.length > 0}
			<div>
				<a href="/transactions" class="home-table-title">{$t('index.expenditures')}</a>
				<table class="amount-by-category-table">
					<tbody>
						{#each data.expenditures as expenditure}
							<tr on:click={() => goToTransactions(expenditure)} role="button">
								<td>{expenditure.categoryName}</td>
								<td class="amount-cell">{Formatter.money(expenditure.amount, currency, $locale)}</td>
							</tr>

							{#each expenditure.subItems as subExpenditure}
								<tr on:click={() => goToTransactions(subExpenditure)} role="button">
									<td class="sub-category-cell">{subExpenditure.categoryName}</td>
									<td class="amount-cell">{Formatter.money(subExpenditure.amount, currency, $locale)}</td>
								</tr>
							{/each}
						{/each}
					</tbody>
				</table>
			</div>
		{/if}

		{#if data.upcomingExpenses && data.upcomingExpenses.length > 0}
			<div>
				<a href="/upcomingExpenses" class="home-table-title">{$t('index.upcomingExpenses')}</a>
				<table class="home-table">
					<tbody>
						{#each data.upcomingExpenses as upcomingExpense}
							<tr>
								<td>{upcomingExpense.category}</td>
								<td>{upcomingExpense.description}</td>
								<td class="amount-cell">{Formatter.money(upcomingExpense.amount, currency, $locale)}</td>
							</tr>
						{/each}
					</tbody>
					{#if data.upcomingExpenses.length > 1}
						<tfoot>
							<tr>
								<td colspan="3">
									<!-- Space taker -->
								</td>
							</tr>
							<tr>
								<td colspan="3" class="table-sum">{Formatter.money(data.upcomingSum, currency, $locale)}</td>
							</tr>
						</tfoot>
					{/if}
				</table>
			</div>
		{/if}

		{#if data.debt && data.debt.length > 0}
			<div>
				<a href="/debt" class="home-table-title">{$t('index.debt')}</a>
				<table class="home-table">
					<tbody>
						{#each data.debt as debtItem}
							<tr>
								<td>
									{#if debtItem.userIsDebtor}
										<span>{$t('index.to')}</span>
									{:else}
										<span>{$t('index.from')}</span>
										{debtItem.person}
									{/if}
								</td>
								<td>{debtItem.description}</td>
								<td class="amount-cell {debtItem.userIsDebtor ? 'expense-color' : 'deposit-color'}">
									{Formatter.money(debtItem.amount, currency, $locale)}
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.content-wrap {
		padding-top: 15px;
	}

	.capital-summary {
		display: flex;
	}
	.summary-item-wrap {
		width: 32%;
		background: linear-gradient(225deg, #7a46f3, #00a6ed);
		border-radius: var(--border-radius);
		margin-right: 2%;
		text-decoration: none;
		color: #fff;
		opacity: 0.6;
		transition: opacity 350ms ease-out;

		&.loaded {
			opacity: 1;
		}

		&:last-child {
			margin-right: 0;
		}

		.summary-item {
			padding: 8px 10px 10px;

			.summary-title {
				font-size: 0.9rem;
			}
			.summary-value {
				margin-top: 10px;
				font-size: 1.4rem;
				line-height: 1.2rem;
			}
		}
	}

	.home-buttons {
		display: flex;
		justify-content: space-between;
		border-top: 1px solid #ddd;
		padding-top: 15px;
		margin-top: 20px;

		.home-button {
			position: relative;
			display: inline-block;
			width: calc(50% - 15px);
			background: #fff;
			border: 1px solid #ddd;
			border-left: 3px solid var(--primary-color);
			border-right: 3px solid var(--primary-color);
			border-radius: var(--border-radius);
			text-align: center;
			text-decoration: none;
			font-size: inherit;
			line-height: 45px;
			color: var(--primary-color);
			cursor: pointer;

			&:hover {
				color: var(--primary-color-dark);
			}
		}
	}

	.home-table-title {
		display: block;
		border-bottom: 1px solid #ddd;
		padding-bottom: 5px;
		margin: 35px 0 10px;
		text-decoration: none;
		color: var(--primary-color);
	}
	.home-table {
		width: 100%;
		font-size: 1rem;

		td {
			padding: 5px 15px 5px 0;
			line-height: 1.2rem;
		}

		td:last-child {
			padding-right: 0;
			text-align: right;
		}

		.amount-cell {
			white-space: nowrap;
		}

		.table-sum {
			border-top: 1px solid #ddd;
			padding: 5px 0;
			font-size: 1.1rem;
		}
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .home-buttons .home-button:hover {
		color: var(--primary-color);
	}

	@media screen and (min-width: 1200px) {
		.home-table td {
			font-size: 1.1rem;
			line-height: 1.3rem;
		}

		.home-table .table-sum {
			font-size: 1.3rem;
		}
	}
</style>
