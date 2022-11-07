<script lang="ts">
	import { onMount, onDestroy, debug } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';

	import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';
	import { DateHelper } from '../../../../shared2/utils/dateHelper';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { t } from '$lib/localization/i18n';
	import { CapitalService } from '$lib/services/capitalService';
	import { Formatter } from '$lib/utils/formatter';
	import { isOnline, user, syncStatus, searchFilters } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';
	import { SearchFilters } from '$lib/models/viewmodels/searchFilters';
	import type { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { HomePageData, HomePageDebt, HomePageUpcomingExpense } from '$lib/models/viewmodels/homePage';
	import { AccountsService } from '$lib/services/accountsService';

	let data = new HomePageData();
	let currency: string;
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

	async function getCapital() {
		const showUpcomingExpenses = localStorage.getBool(LocalStorageKeys.ShowUpcomingExpensesOnHomePage);
		const showDebt = localStorage.getBool(LocalStorageKeys.ShowDebtOnHomePage);

		const mainAccountId = await accountsService.getMainId();

		const balancePromise = accountsService.getBalance(mainAccountId, currency);

		const expendituresPromise = capitalService.getSpent(mainAccountId, $t('uncategorized'), currency);

		const upcomingExpensesPromise = new Promise<{
			upcomingSum: number;
			upcomingExpenses: HomePageUpcomingExpense[];
		} | null>(async (resolve) => {
			if (!showUpcomingExpenses) {
				resolve(null);
				return;
			}

			const result = await capitalService.getUpcomingExpenses($t('uncategorized'), currency);
			resolve(result);
		});

		const debtPromise = new Promise<HomePageDebt[]>(async (resolve) => {
			if (!showDebt) {
				resolve([]);
				return;
			}

			resolve(await capitalService.getDebt(currency, $t('index.combined')));
		});

		const result = await Promise.all([balancePromise, expendituresPromise, upcomingExpensesPromise, debtPromise]);
		data = {
			available: result[0] - (result[2] === null ? 0 : result[2].upcomingSum),
			balance: result[0],
			spent: result[1].spent,
			expenditures: result[1].expenditures,
			upcomingSum: result[2] === null ? 0 : result[2].upcomingSum,
			upcomingExpenses: result[2] === null ? [] : result[2].upcomingExpenses,
			debt: result[3]
		};

		finishProgressBar();
		dataLoaded = true;
		localStorage.set('homePageData', JSON.stringify(data));
	}

	function sync() {
		syncStatus.set(AppEvents.ReSync);
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
		if (!navigator.onLine) {
			dataLoaded = true;
		}

		localStorage = new LocalStorageUtil();
		usersService = new UsersServiceBase('Accountant');
		capitalService = new CapitalService();
		accountsService = new AccountsService();

		const cache = localStorage.getObject<HomePageData>('homePageData');
		if (cache) {
			data = cache;
		}

		currency = localStorage.get(LocalStorageKeys.Currency);

		unsubscriptions.push(
			syncStatus.subscribe((value) => {
				if (value === AppEvents.SyncStarted) {
					startProgressBar();
				} else if (value === AppEvents.SyncFinished) {
					getCapital();
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
				<img src={$user.imageUri} class="profile-image" width="40" height="40" alt="" />
			</a>

			<div class="page-title reduced">
				<span />
			</div>
			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={!$isOnline || progressBarActive}
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
					<div class="summary-value">{Formatter.number(data.available, currency, $user.language)}</div>
				</div>
			</a>
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('index.spent')}</div>
					<div class="summary-value">{Formatter.number(data.spent, currency, $user.language)}</div>
				</div>
			</a>
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('balance')}</div>
					<div class="summary-value">{Formatter.number(data.balance, currency, $user.language)}</div>
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
								<td class="amount-cell">{Formatter.money(expenditure.amount, currency, $user.language)}</td>
							</tr>

							{#each expenditure.subItems as subExpenditure}
								<tr on:click={() => goToTransactions(subExpenditure)} role="button">
									<td class="sub-category-cell">{subExpenditure.categoryName}</td>
									<td class="amount-cell">{Formatter.money(subExpenditure.amount, currency, $user.language)}</td>
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
								<td class="amount-cell">{Formatter.money(upcomingExpense.amount, currency, $user.language)}</td>
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
								<td colspan="3" class="table-sum">{Formatter.money(data.upcomingSum, currency, $user.language)}</td>
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
									{/if}
									<span>{debtItem.person}</span>
								</td>
								<td>{debtItem.description}</td>
								<td class="amount-cell {debtItem.userIsDebtor ? 'expense-color' : 'deposit-color'}">
									{Formatter.money(debtItem.amount, currency, $user.language)}
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
