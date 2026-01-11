<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../../Core/shared2/utils/dateHelper';
	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { AccountsService } from '$lib/services/accountsService';
	import { EncryptionService } from '$lib/services/encryptionService';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let fromExpenditureHeatmap = false;
	let type: TransactionType | null = null;
	let typeLabel = $state('---');
	let accountLabel = $state('---');
	let accountValue = $state('---');
	let amount: number | undefined = $state(undefined);
	let originalAmount: number | null = $state(null);
	let currency: string | null = $state(null);
	let fromStocks: number | null = $state(null);
	let toStocks: number | null = $state(null);
	let categoryName = $state('---');
	let description: string | null = $state(null);
	let date = $state('---');
	let isEncrypted = $state(false);
	let encryptedDescription: string | null = $state(null);
	let salt: string | null;
	let nonce: string | null;
	let generated = $state(false);
	let decryptionPassword: string | null = $state(null);
	let preferredCurrency: string | null = $state(null);
	let passwordShown = $state(false);
	let decryptButtonIsLoading = $state(false);
	let decryptionPasswordIsInvalid = $state(false);
	const typeStringMap = new Map<TransactionType, string>();
	let passwordInput: HTMLInputElement | null = $state(null);
	let passwordShowIconLabel = $state('');

	const localStorage = new LocalStorageUtil();
	let transactionsService: TransactionsService;
	let categoriesService: CategoriesService;
	let accountsService: AccountsService;
	let encryptionService: EncryptionService;

	function formatOccurrenceDate(occurrenceDateString: string): string {
		const date = new Date(Date.parse(occurrenceDateString));
		const month = DateHelper.getLongMonth(date, $user.language);

		const now = new Date();
		if (now.getFullYear() === date.getFullYear()) {
			return `${month} ${date.getDate()}`;
		}

		return `${month} ${date.getDate()}, ${date.getFullYear()}`;
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

	async function decrypt(event: Event) {
		event.preventDefault();

		if (ValidationUtil.isEmptyOrWhitespace(decryptionPassword)) {
			decryptionPasswordIsInvalid = true;
			return;
		}

		decryptButtonIsLoading = true;

		try {
			if (!encryptedDescription || !salt || !nonce || !decryptionPassword) {
				throw new Error('Cannot decrypt. Description, salt, nonce, or password missing.');
			}

			const decryptedDescription = await encryptionService.decrypt(encryptedDescription, salt, nonce, decryptionPassword);

			isEncrypted = false;
			description = decryptedDescription;
			decryptButtonIsLoading = false;
		} catch {
			decryptionPasswordIsInvalid = true;
			decryptButtonIsLoading = false;
		}
	}

	function back() {
		if (fromExpenditureHeatmap) {
			goto('/expenditureHeatmap');
		} else {
			goto('/transactions');
		}
	}

	onMount(async () => {
		typeStringMap.set(TransactionType.Expense, $t('transaction.expense'));
		typeStringMap.set(TransactionType.Deposit, $t('transaction.deposit'));
		typeStringMap.set(TransactionType.Transfer, $t('transaction.transfer'));

		const fromExpenditureHeatmapParam = $page.url.searchParams.get('fromExpenditureHeatmap');
		if (fromExpenditureHeatmapParam) {
			fromExpenditureHeatmap = fromExpenditureHeatmapParam === 'true';
		}

		transactionsService = new TransactionsService();
		categoriesService = new CategoriesService();
		accountsService = new AccountsService();
		encryptionService = new EncryptionService();

		preferredCurrency = localStorage.get(LocalStorageKeys.Currency);

		const transaction = await transactionsService.getForViewing(data.id, preferredCurrency);
		if (transaction === null) {
			throw new Error('Transaction not found');
		}

		if (transaction.isEncrypted) {
			passwordShowIconLabel = $t('showPassword');
		}

		type = TransactionsService.getType(transaction.fromAccountId, transaction.toAccountId);

		typeLabel = <string>typeStringMap.get(type);
		amount = transaction.convertedAmount;
		originalAmount = transaction.amount;
		currency = transaction.currency;
		fromStocks = transaction.fromStocks;
		toStocks = transaction.toStocks;

		if (transaction.categoryId) {
			const category = await categoriesService.get(transaction.categoryId);
			categoryName = category.fullName;
		} else {
			categoryName = $t('uncategorized');
		}

		description = transaction.description;
		date = formatOccurrenceDate(transaction.date);
		isEncrypted = transaction.isEncrypted;
		encryptedDescription = transaction.encryptedDescription;
		salt = transaction.salt;
		nonce = transaction.nonce;
		generated = transaction.generated;

		if (type === TransactionType.Transfer) {
			const fromAccount = await accountsService.get(<number>transaction.fromAccountId);
			const toAccount = await accountsService.get(<number>transaction.toAccountId);
			accountLabel = $t('transaction.accounts');
			accountValue = $t('transaction.to', {
				from: fromAccount.name,
				to: toAccount.name
			});
		} else if (type === TransactionType.Deposit) {
			const toAccount = await accountsService.get(<number>transaction.toAccountId);
			accountLabel = $t('toAccount');
			accountValue = toAccount.name;
		} else {
			const fromAccount = await accountsService.get(<number>transaction.fromAccountId);
			accountLabel = $t('fromAccount');
			accountValue = fromAccount.name;
		}
	});

	onDestroy(() => {
		transactionsService?.release();
		categoriesService?.release();
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<a href="/editTransaction/{data.id}" class="side small" title={$t('edit')} aria-label={$t('edit')}>
			<i class="fas fa-pencil-alt"></i>
		</a>
		<div class="page-title">{typeLabel}</div>
		<button type="button" onclick={back} class="back-button">
			<i class="fas fa-times"></i>
		</button>
	</div>

	<div class="content-wrap">
		<div class="viewing">
			{#if generated}
				<AlertBlock type="info" message={$t('transaction.generatedAlert')} />
			{/if}

			<div class="form-control inline">
				<span>{$t('amount')}</span>
				<span>{Formatter.money(amount, preferredCurrency, $user.culture)}</span>
			</div>

			{#if currency && currency !== preferredCurrency}
				<div class="form-control inline">
					<span>{$t('transaction.originalAmount')}</span>
					<span>{Formatter.money(originalAmount, currency, $user.culture)}</span>
				</div>
			{/if}

			<div class="form-control inline">
				<span>{accountLabel}</span>
				<span>{accountValue}</span>
			</div>

			{#if fromStocks}
				<div class="form-control inline">
					<span>{$t('soldStocks')}</span>
					<span class="expense-color">{Formatter.numberPrecise(fromStocks, preferredCurrency, $user.culture, 4)}</span>
				</div>
			{/if}

			{#if toStocks}
				<div class="form-control inline">
					<span>{$t('purchasedStocks')}</span>
					<span class="deposit-color">{Formatter.numberPrecise(toStocks, preferredCurrency, $user.culture, 4)}</span>
				</div>
			{/if}

			<div class="form-control inline">
				<span>{$t('category')}</span>
				<span>{categoryName}</span>
			</div>

			<div class="form-control inline">
				<span>{$t('date')}</span>
				<span>{date}</span>
			</div>

			{#if description}
				<div class="form-control">
					<div class="description-view">
						<span>{$t('description')}</span>
						<textarea bind:value={description} readonly></textarea>
					</div>
				</div>
			{/if}

			{#if isEncrypted}
				<div class="form-control">
					<div class="description-view encrypted">
						<span>{$t('description')}</span>

						<form onsubmit={decrypt} class="decrypt-form">
							<div class="viewable-password">
								<input
									type="password"
									bind:this={passwordInput}
									bind:value={decryptionPassword}
									class="password-input"
									maxlength="100"
									class:invalid={decryptionPasswordIsInvalid}
									placeholder={$t('password')}
									aria-label={$t('password')}
									required
								/>
								<button
									type="button"
									onclick={togglePasswordShow}
									class="password-show-button"
									class:shown={passwordShown}
									title={passwordShowIconLabel}
									aria-label={passwordShowIconLabel}
								>
									<i class="fas fa-eye"></i>
									<i class="fas fa-eye-slash"></i>
								</button>
							</div>
							<button
								type="button"
								onclick={decrypt}
								class="decrypt-button"
								class:loading={decryptButtonIsLoading}
								title={$t('decryptDescription')}
								aria-label={$t('decryptDescription')}
							>
								<i class="fas fa-unlock"></i>
								<i class="fas fa-circle-notch fa-spin"></i>
							</button>
						</form>
					</div>
				</div>
			{/if}
		</div>
	</div>
</section>

<style lang="scss">
	.description-view {
		background: #f9f8fe;
		box-shadow: inset var(--box-shadow);
		border-radius: var(--border-radius);
		padding: 6px 12px 12px 12px;
		line-height: 30px;

		span {
			display: block;
			margin-bottom: 8px;
			color: var(--faded-color);
		}

		.password-input {
			background-color: #fff;
		}

		textarea {
			width: 100%;
			height: 100px;
			border: none;
			padding: 0;
			background: transparent;
			border-radius: 0;
			box-shadow: none;
		}
	}
</style>
