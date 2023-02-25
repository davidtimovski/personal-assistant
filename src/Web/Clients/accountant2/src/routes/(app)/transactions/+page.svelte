<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import { DateHelper } from '../../../../../shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user, searchFilters } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { AccountsService } from '$lib/services/accountsService';
	import type { TransactionModel } from '$lib/models/entities/transaction';
	import { TransactionItem } from '$lib/models/viewmodels/transactionItem';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { CategoryType } from '$lib/models/entities/category';
	import { DebounceHelper } from '$lib/utils/debounceHelper';

	let transactions: TransactionItem[] | null = null;
	let currency: string;
	let editedId: number | undefined;
	let pageCount: number;
	let categoryOptions: SelectOption[] | null = null;
	let accountOptions: SelectOption[] | null = null;

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let categoriesService: CategoriesService;
	let accountsService: AccountsService;

	function getTransactions(filterChanged: boolean) {
		if (filterChanged) {
			searchFilters.update((x) => {
				x.page = 1;
				return x;
			});
		}

		if ($searchFilters.accountId !== 0 && $searchFilters.type === 3) {
			searchFilters.update((x) => {
				x.type = 0;
				return x;
			});
		}

		transactions = null;

		const transactionsPromise = transactionsService.getAllByPage($searchFilters, currency);
		const countPromise = transactionsService.count($searchFilters);

		Promise.all([transactionsPromise, countPromise]).then((value: [TransactionModel[], number]) => {
			const t = value[0];
			const count = value[1];

			const transactionItems = new Array<TransactionItem>();

			for (const transaction of t) {
				transactionItems.push(
					new TransactionItem(
						transaction.id,
						transaction.amount,
						getType(transaction.fromAccountId, transaction.toAccountId),
						getDetail(
							transaction.description,
							transaction.isEncrypted,
							transaction.categoryId,
							transaction.fromAccountId,
							transaction.toAccountId
						),
						formatDate(transaction.date),
						transaction.synced
					)
				);
			}

			transactions = transactionItems;

			if (filterChanged) {
				// Scroll to top of table
				const transactionsTableWrap = document.getElementById('transactions-table-wrap');
				if (transactionsTableWrap) {
					window.scroll({
						top: transactionsTableWrap.getBoundingClientRect().top + window.scrollY,
						behavior: 'smooth'
					});
				}
			}

			pageCount = Math.ceil(count / $searchFilters.pageSize);
		});
	}

	function getDetail(
		description: string | null,
		isEncrypted: boolean,
		categoryId: number | null,
		fromAccountId: number | null,
		toAccountId: number | null
	): string {
		if (!accountOptions || !categoryOptions) {
			throw new Error('Category and account options not loaded yet');
		}

		if (fromAccountId && toAccountId) {
			const fromAccount = <SelectOption>accountOptions.find((x) => x.id === fromAccountId);
			const toAccount = <SelectOption>accountOptions.find((x) => x.id === toAccountId);
			return fromAccount.name + ' -> ' + toAccount.name;
		}

		if (description) {
			if (isEncrypted) {
				return $t('encryptedPlaceholder');
			}

			const length = 40;
			if (description.length <= length) {
				return description;
			}

			return description.substring(0, length - 2) + '..';
		}

		const category = <SelectOption>categoryOptions.find((x) => x.id === categoryId);
		return category.name;
	}

	function getType(fromAccountId: number | null, toAccountId: number | null): TransactionType {
		if ($searchFilters.accountId) {
			if ($searchFilters.accountId === fromAccountId) {
				return TransactionType.Expense;
			}

			if ($searchFilters.accountId === toAccountId) {
				return TransactionType.Deposit;
			}

			return TransactionType.Deposit;
		}

		return TransactionsService.getType(fromAccountId, toAccountId);
	}

	function formatDate(dateString: string): string {
		const date = new Date(Date.parse(dateString));
		const month = DateHelper.getShortMonth(date, $user.language);

		const now = new Date();
		if (now.getFullYear() === date.getFullYear()) {
			return `${month} ${date.getDate()}`;
		}

		return `${month} ${date.getDate()}, ${date.getFullYear()}`;
	}

	function filterChanged() {
		getTransactions(true);
	}

	function clearDescriptionFilter() {
		searchFilters.update((x) => {
			x.description = '';
			return x;
		});
		getTransactions(true);
	}

	function first() {
		searchFilters.update((x) => {
			x.page = 1;
			return x;
		});
		getTransactions(false);
	}

	function last() {
		searchFilters.update((x) => {
			x.page = pageCount;
			return x;
		});
		getTransactions(false);
	}

	function previous() {
		searchFilters.update((x) => {
			x.page--;
			return x;
		});
		getTransactions(false);
	}

	function next() {
		searchFilters.update((x) => {
			x.page++;
			return x;
		});
		getTransactions(false);
	}

	function viewTransaction(id: number) {
		goto(`/transaction/${id}`);
	}

	onMount(() => {
		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		categoriesService = new CategoriesService();
		accountsService = new AccountsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const categoryOptionsPromise = new Promise<void>(async (resolve) => {
			const options = await categoriesService.getAllAsOptions($t('uncategorized'), CategoryType.AllTransactions);
			categoryOptions = [new SelectOption(0, $t('transactions.all'))].concat(options);
			resolve();
		});

		const accountOptionsPromise = new Promise<void>(async (resolve) => {
			const options = await accountsService.getAllAsOptions();
			accountOptions = [new SelectOption(0, $t('transactions.all'))].concat(options);
			resolve();
		});

		Promise.all([categoryOptionsPromise, accountOptionsPromise]).then(() => {
			getTransactions(false);
		});
	});

	onDestroy(() => {
		transactionsService?.release();
		categoriesService?.release();
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-search-dollar" />
		</div>
		<div class="page-title">{$t('transactions.transactions')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form on:submit|preventDefault={filterChanged}>
			<div class="form-control inline">
				<label for="from-date">{$t('transactions.from')}</label>
				<input type="date" id="from-date" bind:value={$searchFilters.fromDate} on:change={filterChanged} />
			</div>
			<div class="form-control inline">
				<label for="to-date">{$t('transactions.to')}</label>
				<input type="date" id="to-date" bind:value={$searchFilters.toDate} on:change={filterChanged} />
			</div>
			<div class="form-control inline">
				<label for="category">{$t('category')}</label>
				<div class="loadable-select" class:loaded={categoryOptions}>
					<select
						id="category"
						bind:value={$searchFilters.categoryId}
						on:change={filterChanged}
						disabled={!categoryOptions}
						class="category-select"
					>
						{#if categoryOptions}
							{#each categoryOptions as category}
								<option value={category.id}>{category.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin" />
				</div>
			</div>
			<div class="form-control inline">
				<label for="account">{$t('account')}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select
						id="account"
						bind:value={$searchFilters.accountId}
						on:change={filterChanged}
						disabled={!accountOptions}
						class="category-select"
					>
						{#if accountOptions}
							{#each accountOptions as account}
								<option value={account.id}>{account.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin" />
				</div>
			</div>
			<div class="form-control">
				<div class="multi-radio-wrap">
					<div class="multi-radio-part">
						<label class:selected={$searchFilters.type === 0}>
							<span>{$t('transactions.all')}</span>
							<input
								type="radio"
								name="typeToggle"
								value={0}
								bind:group={$searchFilters.type}
								on:change={filterChanged}
							/>
						</label>
					</div>
					<div class="multi-radio-part">
						<label class:selected={$searchFilters.type === 1}>
							<span>{$t('transactions.expenses')}</span>
							<input
								type="radio"
								name="typeToggle"
								value={1}
								bind:group={$searchFilters.type}
								on:change={filterChanged}
							/>
						</label>
					</div>
					<div class="multi-radio-part">
						<label class:selected={$searchFilters.type === 2}>
							<span>{$t('transactions.deposits')}</span>
							<input
								type="radio"
								name="typeToggle"
								value={2}
								bind:group={$searchFilters.type}
								on:change={filterChanged}
							/>
						</label>
					</div>
					{#if $searchFilters.accountId === 0}
						<div class="multi-radio-part">
							<label class:selected={$searchFilters.type === 3}>
								<span>{$t('transactions.transfers')}</span>
								<input
									type="radio"
									name="typeToggle"
									value={3}
									bind:group={$searchFilters.type}
									on:change={filterChanged}
								/>
							</label>
						</div>
					{/if}
				</div>
			</div>
			<div class="form-control">
				<div class="description-filter-wrap" class:searching={$searchFilters.description}>
					<input
						type="text"
						bind:value={$searchFilters.description}
						on:keyup={() => DebounceHelper.debounce(() => getTransactions(true), 1000)}
						maxlength="30"
						placeholder={$t('transactions.searchByDescription')}
						aria-label={$t('transactions.searchByDescription')}
					/>
					<button
						type="button"
						on:click={clearDescriptionFilter}
						title={$t('transactions.clear')}
						aria-label={$t('transactions.clear')}
					>
						<i class="fas fa-times" />
					</button>
				</div>
			</div>
		</form>

		{#if !transactions}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else}
			<div id="transactions-table-wrap">
				<table class="editable-table">
					<thead>
						<tr>
							<th class="type-cell" />
							<th>{$t('amount')}</th>
							<th>{$t('transactions.detail')}</th>
							<th>{$t('date')}</th>
							<th class="sync-icon-cell" />
						</tr>
					</thead>
					{#if transactions.length > 0}
						<tbody>
							{#each transactions as transaction}
								<tr
									on:click={() => viewTransaction(transaction.id)}
									class="clickable"
									class:highlighted-row={transaction.id === editedId}
									role="button"
								>
									<td class="type-cell">
										{#if transaction.type === 1}
											<i class="fas fa-wallet expense-color" />
										{/if}

										{#if transaction.type === 2}
											<i class="fas fa-donate deposit-color" />
										{/if}

										{#if transaction.type === 3}
											<i class="fas fa-exchange-alt transfer-color" />
										{/if}
									</td>
									<td>{Formatter.number(transaction.amount, currency, $user.culture)}</td>
									<td>{transaction.detail}</td>
									<td class="date-cell">{transaction.date}</td>

									<td class="sync-icon-cell">
										{#if !transaction.synced}
											<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')} />
										{/if}
									</td>
								</tr>
							{/each}
						</tbody>
					{:else}
						<tfoot>
							<td class="no-data-column" colspan="5">{$t('transactions.thereIsNothingHere')}</td>
						</tfoot>
					{/if}
				</table>

				{#if transactions.length > 0}
					<div class="transactions-pagination">
						<button
							type="button"
							on:click={first}
							title={$t('transactions.first')}
							aria-label={$t('transactions.first')}
							class="transactions-pagination-arrow-wrap left"
							class:hidden={$searchFilters.page === 1}
						>
							<i class="fas fa-angle-double-left" />
						</button>
						<button
							type="button"
							on:click={previous}
							title={$t('transactions.previous')}
							aria-label={$t('transactions.previous')}
							class="transactions-pagination-arrow-wrap left"
							class:hidden={$searchFilters.page === 1}
						>
							<i class="fas fa-angle-left" />
						</button>
						<div class="transactions-pagination-numbering">
							<span>{$searchFilters.page}</span>/<span>{pageCount}</span>
						</div>
						<button
							type="button"
							on:click={next}
							title={$t('transactions.next')}
							aria-label={$t('transactions.next')}
							class="transactions-pagination-arrow-wrap right"
							class:hidden={$searchFilters.page === pageCount}
						>
							<i class="fas fa-angle-right" />
						</button>
						<button
							type="button"
							on:click={last}
							title={$t('transactions.last')}
							aria-label={$t('transactions.last')}
							class="transactions-pagination-arrow-wrap right"
							class:hidden={$searchFilters.page === pageCount}
						>
							<i class="fas fa-angle-double-right" />
						</button>
					</div>
				{/if}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.description-filter-wrap {
		position: relative;

		input {
			padding-right: 42px;
			width: calc(100% - 56px);
		}

		button {
			display: none;
			position: absolute;
			top: 0;
			right: 0;
			background: transparent;
			border: none;
			outline: none;
			padding: 10px 13px;
			color: var(--primary-color);

			&:hover {
				color: var(--primary-color-dark);
			}
		}

		&.searching {
			input {
				padding-right: 42px;
				width: calc(100% - 56px);
			}

			button {
				display: inline-block;
			}
		}
	}

	.transactions-pagination {
		display: flex;
		justify-content: space-between;
		width: 280px;
		margin: 35px auto 0;

		&-arrow-wrap {
			display: flex;
			align-items: center;
			justify-content: center;
			width: 42px;
			height: 30px;
			background: var(--primary-color);
			border: none;
			outline: none;
			border-radius: var(--border-radius);
			margin: 4px 0;
			font-size: 20px;
			color: #fff;

			&:hover {
				background: var(--primary-color-dark);
			}
		}

		.transactions-pagination-numbering {
			margin: 0px 10px;
			font-size: 20px;
			line-height: 36px;
		}
	}

	@media screen and (min-width: 1200px) {
		.description-filter-wrap {
			input {
				padding-right: 48px;
				width: calc(100% - 62px);
			}

			button {
				padding: 11px 15px;
			}
		}
	}
</style>
