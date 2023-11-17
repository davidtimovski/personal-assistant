<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../../Core/shared2/utils/dateHelper';
	import { ValidationResult, ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';
	import Tooltip from '../../../../../../../Core/shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { alertState, syncStatus, user } from '$lib/stores';
	import type { SelectOption, SelectOptionExtended } from '$lib/models/viewmodels/selectOption';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { AccountsService } from '$lib/services/accountsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { DebtsService } from '$lib/services/debtsService';
	import { CategoryType } from '$lib/models/entities/category';
	import { SyncEvents } from '$lib/models/syncStatus';

	import AmountInput from '$lib/components/AmountInput.svelte';
	import type { Account } from '$lib/models/entities/account';

	export let data: PageData;

	let accountId: number | null = null;
	let categoryId: number | null = null;
	let amount: number | null = null;
	let toStocks: number | null = null;
	let currency: string | null = null;
	let description: string | null = null;
	let date = DateHelper.format(new Date());
	let encrypt = false;
	let encryptionPassword: string | null = null;
	let debtId: number | null = null;
	let userIsDebtor: boolean;
	let debtPerson: string;
	let accountOptions: SelectOptionExtended<Account>[] | null = null;
	let categoryOptions: SelectOption[] | null = null;
	const maxDate = date;
	let passwordShown = false;
	let amountIsInvalid = false;
	let toStocksIsInvalid = false;
	let dateIsInvalid = false;
	let encryptionPasswordIsInvalid = false;
	let submitButtonIsLoading = false;
	let stocksInput: HTMLInputElement | null = null;
	let passwordInput: HTMLInputElement | null = null;
	let passwordShowIconLabel: string;
	let accountIsFund = false;
	let selectedAccountOption: SelectOptionExtended<Account> | null = null;

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let accountsService: AccountsService;
	let categoriesService: CategoriesService;
	let debtsService: DebtsService;

	let amountFrom = 0.01;
	let amountTo = 8000000;
	const toStocksFrom = 0.0001;
	const toStocksTo = 100000;

	const alertUnsubscriber = alertState.subscribe((value) => {
		if (value.hidden) {
			amountIsInvalid = false;
			dateIsInvalid = false;
			encryptionPasswordIsInvalid = false;
		}
	});

	$: pastMidnight = (): string | null => {
		if (!date) {
			return null;
		}

		const now = new Date();
		const selectedDate = new Date(date);

		const isToday =
			selectedDate.getDate() == now.getDate() && selectedDate.getMonth() == now.getMonth() && selectedDate.getFullYear() == now.getFullYear();

		if (isToday) {
			const hour = now.getHours();
			if (hour < 4) {
				return DateHelper.formatHoursMinutes(now, $user.culture);
			}
		}

		return null;
	};

	$: canEncrypt = () => {
		if (!description) {
			encrypt = false;
			encryptionPassword = null;
			return false;
		}

		const canEncrypt = description.trim().length > 0;
		if (!canEncrypt) {
			encrypt = false;
			encryptionPassword = null;
		}
		return canEncrypt;
	};

	function deriveAmountBasedOnStocks() {
		let decimals = 2;
		if (selectedAccountOption!.data.currency === 'MKD') {
			decimals = 0;
		}

		amount = parseFloat((toStocks! * selectedAccountOption!.data.stockPrice!).toFixed(decimals));
	}

	function accountChanged() {
		if (data.isExpense) {
			return;
		}

		selectedAccountOption = accountOptions!.find((x) => x.id === accountId)!;
		accountIsFund = selectedAccountOption.data.stockPrice !== null;

		if (accountIsFund) {
			deriveAmountBasedOnStocks();
			currency = selectedAccountOption.data.currency;

			stocksInput?.focus();
		} else {
			amount = null;
			toStocks = null;
		}
	}

	function togglePasswordShow() {
		if (!passwordInput) {
			return;
		}

		if (passwordShown) {
			passwordInput.type = 'password';
			passwordShowIconLabel = $t('showPassword');
		} else {
			passwordInput.type = 'text';
			passwordShowIconLabel = $t('hidePassword');
		}

		passwordShown = !passwordShown;
	}

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (accountIsFund && !ValidationUtil.between(toStocks, toStocksFrom, toStocksTo)) {
			result.fail('toStocks');
		} else if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
			result.fail('amount');
		}

		if (!date) {
			result.fail('date');
		}

		if (encrypt && ValidationUtil.isEmptyOrWhitespace(encryptionPassword)) {
			result.fail('encryptionPassword');
		}

		return result;
	}

	async function submit() {
		if (!amount || !currency) {
			throw new Error('Unexpected error: required fields missing');
		}

		submitButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();
		if (result.valid) {
			amountIsInvalid = false;
			dateIsInvalid = false;
			encryptionPasswordIsInvalid = false;

			try {
				let fromAccountId: number | null = null;
				let toAccountId: number | null = null;
				if (data.isExpense) {
					fromAccountId = accountId;
				} else {
					toAccountId = accountId;
				}

				await transactionsService.create(
					fromAccountId,
					toAccountId,
					categoryId,
					amount,
					toStocks,
					currency,
					description,
					<string>date,
					encrypt,
					encryptionPassword
				);

				if (debtId) {
					await debtsService.delete(debtId);

					alertState.update((x) => {
						x.showSuccess('newTransaction.debtSettlingSuccessful');
						return x;
					});
				} else {
					const messageKey = data.isExpense ? 'newTransaction.expenseSubmitted' : 'newTransaction.depositSubmitted';

					alertState.update((x) => {
						x.showSuccess(messageKey);
						return x;
					});
				}

				goto('/dashboard');
			} catch {
				submitButtonIsLoading = false;
			}
		} else {
			const messages = new Array<string>();

			const invalidAmount = result.erroredFields.includes('amount');
			if (invalidAmount) {
				messages.push($t('amountBetween', { from: amountFrom, to: amountTo }));
				amountIsInvalid = true;
			}

			const invalidToStocks = result.erroredFields.includes('toStocks');
			if (invalidToStocks) {
				messages.push($t('stocksBetween', { from: toStocksFrom, to: toStocksTo }));
				toStocksIsInvalid = true;
			}

			const invalidDate = result.erroredFields.includes('date');
			if (invalidDate) {
				messages.push($t('dateIsRequired'));
				dateIsInvalid = true;
			}

			const invalidEncryptionPassword = result.erroredFields.includes('encryptionPassword');
			if (invalidEncryptionPassword) {
				messages.push($t('passwordIsRequired'));
				encryptionPasswordIsInvalid = true;
			}

			if (messages.length > 0) {
				alertState.update((x) => {
					x.showErrors(messages);
					return x;
				});
			}

			submitButtonIsLoading = false;
		}
	}

	onMount(async () => {
		passwordShowIconLabel = $t('showPassword');

		const debtIdParam = $page.url.searchParams.get('debtId');
		if (debtIdParam) {
			debtId = parseInt(debtIdParam, 10);
		}

		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		accountsService = new AccountsService();
		categoriesService = new CategoriesService();
		debtsService = new DebtsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		if (currency === 'MKD') {
			amountFrom = 1;
			amountTo = 450000000;
		}

		const categoryType = data.isExpense ? CategoryType.ExpenseOnly : CategoryType.DepositOnly;

		const exlcudeFunds = data.isExpense;
		accountsService.getAllAsOptionsExtended(exlcudeFunds).then((options) => {
			accountOptions = options;
			accountId = <number>options[0].id;
		});

		categoriesService.getAllAsOptions($t('uncategorized'), categoryType).then((options) => {
			categoryOptions = options;
		});

		if (debtId) {
			const debt = await debtsService.get(debtId);
			userIsDebtor = debt.userIsDebtor;
			debtPerson = debt.person;
			amount = debt.amount;
		}
	});

	onDestroy(() => {
		alertUnsubscriber();
		transactionsService?.release();
		accountsService?.release();
		categoriesService?.release();
		debtsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		{#if data.isExpense}
			<div class="side inactive small">
				<i class="fas fa-wallet" />
			</div>
		{:else}
			<div class="side inactive medium">
				<i class="fas fa-donate" />
			</div>
		{/if}

		<div class="page-title">
			<span>{$t(data.isExpense ? 'newTransaction.newExpense' : 'newTransaction.newDeposit')}</span>
		</div>

		<a href={debtId ? '/debt' : '/dashboard'} class="back-button" role="button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if debtId}
			{#if userIsDebtor}
				<AlertBlock type="info" message={$t('newTransaction.settleDebtTo', { person: debtPerson })} />
			{:else}
				<AlertBlock type="info" message={$t('newTransaction.settleDebtFrom', { person: debtPerson })} />
			{/if}
		{/if}

		{#if pastMidnight()}
			<AlertBlock type="info" message={$t('newTransaction.considerUsingYesterday', { time: pastMidnight() })} />
		{/if}

		<form on:submit|preventDefault={submit} autocomplete="off">
			{#if accountIsFund}
				<div class="form-control inline">
					<label for="stocks">{$t('stocks')}</label>
					<input
						type="number"
						id="stocks"
						bind:this={stocksInput}
						bind:value={toStocks}
						on:keyup={deriveAmountBasedOnStocks}
						min={toStocksFrom}
						max={toStocksTo}
						step="0.0001"
						class="stocks-input"
						class:invalid={toStocksIsInvalid}
						required
					/>
				</div>
			{/if}

			<div class="form-control inline">
				<label for="amount">{$t('amount')}</label>
				<AmountInput bind:amount bind:currency disabled={accountIsFund} invalid={amountIsInvalid} focusOnInit={true} />
			</div>

			<div class="form-control inline">
				<label for="account">{$t(data.isExpense ? 'fromAccount' : 'toAccount')}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select id="account" bind:value={accountId} on:change={accountChanged} disabled={!accountOptions} class="category-select">
						{#if accountOptions}
							{#each accountOptions as account}
								<option value={account.id}>{account.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin" />
				</div>
			</div>

			<div class="form-control inline">
				<label for="category">{$t('category')}</label>
				<div class="loadable-select" class:loaded={categoryOptions}>
					<select id="category" bind:value={categoryId} disabled={!categoryOptions} class="category-select">
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
				<label for="date">{$t('date')}</label>
				<input type="date" id="date" bind:value={date} max={maxDate} class:invalid={dateIsInvalid} required />
			</div>

			<div class="form-control">
				<div class="encryption-box" class:active={encrypt}>
					<textarea
						bind:value={description}
						maxlength="500"
						class="description-textarea"
						placeholder={$t('description')}
						aria-label={$t('description')}
					/>

					<Checkbox labelKey="newTransaction.encryptDescription" bind:value={encrypt} disabled={!canEncrypt()} />

					{#if encrypt}
						<Tooltip key="encryptedDescription" application="Accountant" />

						<div class="viewable-password">
							<input
								type="password"
								bind:this={passwordInput}
								bind:value={encryptionPassword}
								disabled={!encrypt}
								maxlength="100"
								class:invalid={encryptionPasswordIsInvalid}
								placeholder={$t('password')}
								aria-label={$t('password')}
							/>
							<button
								type="button"
								on:click={togglePasswordShow}
								class="password-show-button"
								class:shown={passwordShown}
								title={passwordShowIconLabel}
								aria-label={passwordShowIconLabel}
							>
								<i class="fas fa-eye" />
								<i class="fas fa-eye-slash" />
							</button>
						</div>
					{/if}
				</div>
			</div>

			<hr />

			<div class="save-delete-wrap">
				<button
					class="button primary-button"
					disabled={$syncStatus.status === SyncEvents.SyncStarted || !amount || !accountId || submitButtonIsLoading}
				>
					<span class="button-loader" class:loading={submitButtonIsLoading}>
						<i class="fas fa-circle-notch fa-spin" />
					</span>
					<span>{$t('newTransaction.submit')}</span>
				</button>
				<a href={debtId ? '/debt' : '/dashboard'} class="button secondary-button">{$t('cancel')}</a>
			</div>
		</form>
	</div>
</section>
