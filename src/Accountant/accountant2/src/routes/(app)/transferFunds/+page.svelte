<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../../../Core/shared2/utils/dateHelper';
	import { ValidationResult, ValidationUtil } from '../../../../../../Core/shared2/utils/validationUtils';
	import { CurrenciesService } from '../../../../../../Core/shared2/services/currenciesService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user, alertState } from '$lib/stores';
	import type { Account } from '$lib/models/entities/account';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { AccountsService } from '$lib/services/accountsService';
	import { TransactionsService } from '$lib/services/transactionsService';

	import AmountInput from '$lib/components/AmountInput.svelte';

	let fromAccountId: number;
	let fromAccount: Account | null = null;
	let toAccountId: number;
	let toAccount: Account | null = null;
	let amount: number | null = null;
	let currency: string | null = null;
	let accountOptions: SelectOption[] | null = null;
	let accounts: Account[];
	let mainAccountId: number;
	let fromAccountLabel = '---';
	let toAccountLabel = '---';
	let amountIsInvalid = false;
	let transferButtonLabel = '';
	let transferButtonIsLoading = false;

	const localStorage = new LocalStorageUtil();
	let accountsService: AccountsService;
	let transactionsService: TransactionsService;
	let currenciesService: CurrenciesService;

	let amountFrom = 0.01;
	let amountTo = 8000000;

	function fromAccountChanged() {
		if (!accountOptions) {
			return;
		}

		setFromAccount();

		if (fromAccountId === mainAccountId) {
			if (fromAccountId === toAccountId) {
				toAccountId = <number>accountOptions[1].id;
				setToAccount();
			}
		} else {
			if (fromAccountId === toAccountId) {
				toAccountId = mainAccountId;
				setToAccount();
			}
		}

		setTransferButtonLabel();
	}

	function toAccountChanged() {
		if (!accountOptions) {
			return;
		}

		setToAccount();

		if (toAccountId === mainAccountId) {
			if (toAccountId === fromAccountId) {
				fromAccountId = <number>accountOptions[1].id;
				setFromAccount();
			}
		} else {
			if (toAccountId === fromAccountId) {
				fromAccountId = mainAccountId;
				setFromAccount();
			}
		}

		setTransferButtonLabel();
	}

	function setFromAccount() {
		fromAccount = <Account>accounts.find((x) => x.id === fromAccountId);
		fromAccountLabel = $t(fromAccount.stockPrice === null ? 'transferFunds.fromAccount' : 'transferFunds.fromInvestmentFund');
	}

	function setToAccount() {
		toAccount = <Account>accounts.find((x) => x.id === toAccountId);
		toAccountLabel = $t(toAccount.stockPrice === null ? 'transferFunds.toAccount' : 'transferFunds.toInvestmentFund');
	}

	function setTransferButtonLabel() {
		if (fromAccount?.stockPrice === null && toAccount?.stockPrice === null) {
			transferButtonLabel = $t('transferFunds.transfer');
		} else if (fromAccount?.stockPrice !== null && toAccount?.stockPrice !== null) {
			transferButtonLabel = $t('transferFunds.transferStock');
		} else if (fromAccount?.stockPrice === null && toAccount?.stockPrice !== null) {
			transferButtonLabel = $t('transferFunds.buyStock');
		} else if (fromAccount?.stockPrice !== null && toAccount?.stockPrice === null) {
			transferButtonLabel = $t('transferFunds.sellStock');
		}
	}

	function validate(): ValidationResult {
		if (!fromAccount || !amount) {
			throw new Error('Unexpected error: required fields missing');
		}

		const result = new ValidationResult();

		if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
			result.fail('amount');
		}

		const amountFloat = parseFloat(amount.toString());
		const fromBalance = currenciesService.convert(<number>fromAccount.balance, fromAccount.currency, <string>currency);

		if (fromBalance < amountFloat) {
			result.fail('insufficientFunds');
		}

		return result;
	}

	async function transfer() {
		if (!fromAccount || !toAccount || !amount || !currency || !accountOptions) {
			throw new Error('Unexpected error: required fields missing');
		}

		transferButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();

		amountIsInvalid = !result.valid;

		if (result.valid) {
			amountIsInvalid = false;

			try {
				let fromStocks: number | null = null;
				if (fromAccount.stockPrice !== null) {
					fromStocks = parseFloat((amount / fromAccount.stockPrice).toFixed(4));
				}

				let toStocks: number | null = null;
				if (toAccount.stockPrice !== null) {
					toStocks = parseFloat((amount / toAccount.stockPrice).toFixed(4));
				}

				if (fromStocks || toStocks) {
					await transactionsService.buySellStocks(fromAccountId, toAccountId, amount, fromStocks, toStocks, currency);
				} else {
					await transactionsService.create(
						fromAccountId,
						toAccountId,
						null,
						amount,
						null,
						currency,
						null,
						DateHelper.format(new Date()),
						false,
						null
					);
				}

				alertState.update((x) => {
					x.showSuccess('transferFunds.transferSuccessful');
					return x;
				});

				goto(`/accounts?edited=${fromAccountId}&edited2=${toAccountId}`);
			} catch {
				transferButtonIsLoading = false;
			}
		} else {
			if (result.erroredFields.includes('insufficientFunds')) {
				const fromAccountName = accountOptions.find((x) => x.id === fromAccountId)?.name;

				alertState.update((x) => {
					x.showErrors([
						$t('transferFunds.accountOnlyHas', {
							account: fromAccountName,
							balance: Formatter.money((<Account>fromAccount).balance, (<Account>fromAccount).currency, $user.culture)
						})
					]);
					return x;
				});
			}

			amountIsInvalid = true;
			transferButtonIsLoading = false;
		}
	}

	onMount(async () => {
		accountsService = new AccountsService();
		transactionsService = new TransactionsService();
		currenciesService = new CurrenciesService('Accountant');

		currency = localStorage.get(LocalStorageKeys.Currency);

		if (currency === 'MKD') {
			amountFrom = 1;
			amountTo = 450000000;
		}

		accountsService.getMainId().then(async (id: number) => {
			mainAccountId = id;

			const options = await accountsService.getAllAsOptions();

			fromAccountId = id;
			setFromAccount();

			toAccountId = <number>options[1].id;
			setToAccount();

			setTransferButtonLabel();

			accountOptions = options;
		});

		accounts = await accountsService.getAllWithBalance(currency);
	});

	onDestroy(() => {
		accountsService?.release();
		transactionsService?.release();
		currenciesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fa-solid fa-money-bill-transfer" />
		</div>
		<div class="page-title">{$t('transferFunds.transferFunds')}</div>
		<a href="/accounts" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form on:submit|preventDefault={transfer}>
			<div class="form-control">
				<label for="from-account" class="transfer-funds-label">{fromAccountLabel}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select id="from-account" bind:value={fromAccountId} on:change={fromAccountChanged} disabled={!accountOptions}>
						{#if accountOptions}
							{#each accountOptions as account}
								<option value={account.id}>{account.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin" />
				</div>

				{#if fromAccount?.stockPrice}
					<div class="account-stock-price-balance-label">
						<span>{$t('transferFunds.stockPrice')}</span>
						<span>{Formatter.moneyPrecise(fromAccount.stockPrice, currency, $user.culture, 4)}</span>
					</div>
				{:else}
					<div class="account-stock-price-balance-label">
						<span>{$t('balance')}</span>
						<span>{Formatter.moneyPrecise(fromAccount?.balance, currency, $user.culture)}</span>
					</div>
				{/if}
			</div>

			<div class="form-control">
				<label for="to-account" class="transfer-funds-label">{toAccountLabel}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select id="to-account" bind:value={toAccountId} on:change={toAccountChanged} disabled={!accountOptions}>
						{#if accountOptions}
							{#each accountOptions as account}
								<option value={account.id}>{account.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin" />
				</div>

				{#if toAccount?.stockPrice}
					<div class="account-stock-price-balance-label">
						<span>{$t('transferFunds.stockPrice')}</span>
						<span>{Formatter.moneyPrecise(toAccount?.stockPrice, currency, $user.culture, 4)}</span>
					</div>
				{:else}
					<div class="account-stock-price-balance-label">
						<span>{$t('balance')}</span>
						<span>{Formatter.moneyPrecise(toAccount?.balance, currency, $user.culture)}</span>
					</div>
				{/if}
			</div>

			<div class="form-control inline transfer-funds-amount-control">
				<label for="amount" class="transfer-funds-label">{$t('amount')}</label>

				<AmountInput bind:amount bind:currency invalid={amountIsInvalid} />
			</div>

			<hr />

			<div class="save-delete-wrap">
				<button class="button primary-button" class:disabled={!amount || transferButtonIsLoading}>
					<span class="button-loader" class:loading={transferButtonIsLoading}>
						<i class="fas fa-circle-notch fa-spin" />
					</span>
					<span>{transferButtonLabel}</span>
				</button>
				<a href="/accounts" class="button secondary-button">{$t('cancel')}</a>
			</div>
		</form>
	</div>
</section>

<style lang="scss">
	.transfer-funds-label {
		display: block;
		margin-left: 10px;
		color: var(--primary-color);
	}
	.transfer-funds-amount-control {
		margin-top: 10px;
	}

	.account-stock-price-balance-label {
		display: flex;
		justify-content: space-between;
		padding: 0 10px;
		margin-top: 5px;
	}
</style>
