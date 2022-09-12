<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { UsersService } from '../../../../shared2/services/usersService';
	import { DateHelper } from '../../../../shared2/utils/dateHelper';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { t } from '$lib/localization/i18n';
	import { CapitalService } from '$lib/services/capitalService';
	import { Formatter } from '$lib/utils/formatter';
	import { loggedInUser, syncStatus, searchFilters } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';
	import { SearchFilters } from '$lib/models/viewmodels/searchFilters';
	import type { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { HomePageData } from '$lib/models/viewmodels/homePageData';
	import { AccountsService } from '$lib/services/accountsService';

	let imageUri: any;
	let data = new HomePageData();
	let currency: string;
	let menuButtonIsLoading = false;
	let connTracker = {
		isOnline: true
	};
	let dataLoaded = false;

	// Progress bar
	let progressBarActive = false;
	let progress = 0;
	let progressIntervalId: number | null = null;
	let progressBarVisible = false;

	let localStorage: LocalStorageUtil;
	let usersService: UsersService;
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

			data.debt = await capitalService.getDebt(currency);
			resolve();
		});

		await Promise.all([numbersPromise, expendituresPromise, debtPromise]);

		finishProgressBar();
		dataLoaded = true;
		localStorage.set('homePageData', JSON.stringify(data));
	}

	function goToMenu() {
		menuButtonIsLoading = true;
		goto('/menu');
	}

	function sync() {
		syncStatus.set(AppEvents.ReSync);

		usersService.getProfileImageUri().then((uri) => {
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
		usersService = new UsersService('Accountant');
		capitalService = new CapitalService();
		accountsService = new AccountsService();

		let cache = localStorage.getObject<HomePageData>('homePageData');
		if (cache) {
			data = cache;
		}

		const showUpcomingExpenses = localStorage.getBool(LocalStorageKeys.ShowUpcomingExpensesOnDashboard);
		const showDebt = localStorage.getBool(LocalStorageKeys.ShowDebtOnDashboard);
		currency = localStorage.get('currency');

		loggedInUser.subscribe((value) => {
			if (!value) {
				return;
			}

			if (usersService.profileImageUriIsStale()) {
				usersService.getProfileImageUri().then((uri) => {
					imageUri = uri;
				});
			} else {
				imageUri = localStorage.get('profileImageUri');
			}
		});

		syncStatus.subscribe((value) => {
			if (value === AppEvents.SyncStarted) {
				startProgressBar();
			} else if (value === AppEvents.SyncFinished) {
				getCapital(showUpcomingExpenses, showDebt);
			}
		});
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			{#if menuButtonIsLoading}
				<span class="menu-loader">
					<i class="fas fa-circle-notch fa-spin" />
				</span>
			{:else}
				<div
					on:click={goToMenu}
					class="profile-image-container"
					role="button"
					title={$t('dashboard.menu')}
					aria-label={$t('dashboard.menu')}
				>
					<img src={imageUri} class="profile-image" width="40" height="40" alt={$t('profilePicture')} />
				</div>
			{/if}

			<div class="page-title reduced">
				<span />
			</div>
			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={!connTracker.isOnline || progressBarActive}
				title={$t('dashboard.refresh')}
				aria-label={$t('dashboard.refresh')}
			>
				<i class="fas fa-sync-alt" />
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {progress}%;" />
		</div>
	</div>

	<div class="content-wrap dashboard">
		<div class="capital-summary">
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('dashboard.available')}</div>
					<div class="summary-value">{Formatter.number(data.available, currency)}</div>
				</div>
			</a>
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('dashboard.spent')}</div>
					<div class="summary-value">{Formatter.number(data.spent, currency)}</div>
				</div>
			</a>
			<a href="/transactions" class="summary-item-wrap" class:loaded={dataLoaded}>
				<div class="summary-item">
					<div class="summary-title">{$t('balance')}</div>
					<div class="summary-value">{Formatter.number(data.balance, currency)}</div>
				</div>
			</a>
		</div>

		<div class="dashboard-buttons">
			<a href="newTransaction/1" class="dashboard-button">
				{$t('dashboard.newDeposit')}
			</a>
			<a href="newTransaction/0" class="dashboard-button">
				{$t('dashboard.newExpense')}
			</a>
		</div>

		{#if data.expenditures && data.expenditures.length > 0}
			<div>
				<a href="/transactions" class="dashboard-table-title">{$t('dashboard.expenditures')}</a>
				<table class="amount-by-category-table">
					<tbody>
						{#each data.expenditures as expenditure}
							<tr on:click={() => goToTransactions(expenditure)}>
								<td>{expenditure.categoryName}</td>
								<td class="amount-cell">{Formatter.money(expenditure.amount, currency)}</td>
							</tr>

							{#each expenditure.subItems as subExpenditure}
								<tr on:click={() => goToTransactions(subExpenditure)}>
									<td class="sub-category-cell">{subExpenditure.categoryName}</td>
									<td class="amount-cell">{Formatter.money(subExpenditure.amount, currency)}</td>
								</tr>
							{/each}
						{/each}
					</tbody>
				</table>
			</div>
		{/if}

		{#if data.upcomingExpenses && data.upcomingExpenses.length > 0}
			<div>
				<a href="/upcomingExpenses" class="dashboard-table-title">{$t('dashboard.upcomingExpenses')}</a>
				<table class="dashboard-table">
					<tbody>
						{#each data.upcomingExpenses as upcomingExpense}
							<tr>
								<td>{upcomingExpense.category}</td>
								<td>{upcomingExpense.description}</td>
								<td class="amount-cell">{Formatter.money(upcomingExpense.amount, currency)}</td>
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
								<td colspan="3">{Formatter.money(data.upcomingSum, currency)}</td>
							</tr>
						</tfoot>
					{/if}
				</table>
			</div>
		{/if}

		{#if data.debt && data.debt.length > 0}
			<div>
				<a href="/debt" class="dashboard-table-title">{$t('dashboard.debt')}</a>
				<table class="dashboard-table">
					<tbody>
						{#each data.debt as debtItem}
							<tr>
								<td>
									{#if debtItem.userIsDebtor}
										<span>{$t('dashboard.to')}</span>
									{:else}
										<span>{$t('dashboard.from')}</span>
										{debtItem.person}
									{/if}
								</td>
								<td>{debtItem.description}</td>
								<td class="amount-cell {debtItem.userIsDebtor ? 'expense-color' : 'deposit-color'}">
									{Formatter.money(debtItem.amount, currency)}
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
	.content-wrap.dashboard {
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

	.dashboard-buttons {
		display: flex;
		justify-content: space-between;
		border-top: 1px solid #ddd;
		padding-top: 15px;
		margin-top: 20px;

		.dashboard-button {
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

	.dashboard-table-title {
		display: block;
		border-bottom: 1px solid #ddd;
		padding-bottom: 5px;
		margin: 35px 0 10px;
		text-decoration: none;
		color: var(--primary-color);
	}
	.dashboard-table {
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

		tfoot tr:last-child td {
			border-top: 1px solid #ddd;
			padding: 5px 0;
			font-size: 1.1rem;
		}
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .dashboard-buttons .dashboard-button:hover {
		color: var(--primary-color);
	}

	@media screen and (min-width: 1200px) {
		.dashboard-table td {
			font-size: 1.1rem;
			line-height: 1.3rem;
		}
	}
</style>
