<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../shared2/utils/dateHelper';
	import { ValidationUtil } from '../../../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { AccountsService } from '$lib/services/accountsService';
	import { EncryptionService } from '$lib/services/encryptionService';

	import AlertBlock from '$lib/components/AlertBlock.svelte';

	export let data: PageData;

	let fromExpenditureHeatmap = false;
	let type: TransactionType;
	let typeLabel = '---';
	let accountLabel = '---';
	let accountValue = '---';
	let amount: number | undefined;
	let originalAmount: number;
	let currency = '';
	let fromStocks: number | null;
	let toStocks: number | null;
	let categoryName = '---';
	let description: string | null;
	let date = '---';
	let isEncrypted: boolean;
	let encryptedDescription: string | null;
	let salt: string | null;
	let nonce: string | null;
	let generated: boolean;
	let decryptionPassword: string | null = null;
	let preferredCurrency: string;
	let language: string;
	let passwordShown = false;
	let decryptButtonIsLoading = false;
	let decryptionPasswordIsInvalid = false;
	let typeStringMap = new Map<TransactionType, string>();
	let passwordInput: HTMLInputElement | null = null;
	let passwordShowIconLabel: string;

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let categoriesService: CategoriesService;
	let accountsService: AccountsService;
	let encryptionService: EncryptionService;

	function formatOccurrenceDate(occurrenceDateString: string): string {
		const date = new Date(Date.parse(occurrenceDateString));
		const month = DateHelper.getLongMonth(date, language);

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

	async function decrypt() {
		if (ValidationUtil.isEmptyOrWhitespace(decryptionPassword)) {
			decryptionPasswordIsInvalid = true;
			return;
		}

		decryptButtonIsLoading = true;

		try {
			if (!encryptedDescription || !salt || !nonce || !decryptionPassword) {
				throw new Error('Cannot decrypt. Description, salt, nonce, or password missing.');
			}

			const decryptedDescription = await encryptionService.decrypt(
				encryptedDescription,
				salt,
				nonce,
				decryptionPassword
			);

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

		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		categoriesService = new CategoriesService();
		accountsService = new AccountsService();
		encryptionService = new EncryptionService();

		preferredCurrency = localStorage.get('currency');
		language = localStorage.get('language');

		const transaction = await transactionsService.getForViewing(data.id, preferredCurrency);
		if (transaction === null) {
			// TODO
			await goto('notFound');
		}

		if (transaction.isEncrypted) {
			passwordShowIconLabel = $t('showPassword');
		}

		const type = TransactionsService.getType(transaction.fromAccountId, transaction.toAccountId);

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
			accountLabel = $t('transaction.toAccount');
			accountValue = toAccount.name;
		} else {
			const fromAccount = await accountsService.get(<number>transaction.fromAccountId);
			accountLabel = $t('transaction.fromAccount');
			accountValue = fromAccount.name;
		}
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<a href="/editTransaction/{data.id}" class="side small" title={$t('edit')} aria-label={$t('edit')}>
			<i class="fas fa-pencil-alt" />
		</a>
		<div class="page-title">{typeLabel}</div>
		<button type="button" on:click={back} class="back-button">
			<i class="fas fa-times" />
		</button>
	</div>

	<div class="content-wrap">
		<div class="viewing">
			{#if generated}
				<AlertBlock type="info" message={$t('transaction.generatedAlert')} />
			{/if}

			<div class="form-control inline">
				<span>{$t('amount')}</span>
				<span class:expense-color={type === 1} class:deposit-color={type === 2} class:transfer-color={type === 3}
					>{Formatter.money(amount, preferredCurrency)}</span
				>
			</div>

			{#if currency && currency !== preferredCurrency}
				<div class="form-control inline">
					<span><span>{$t('transaction.originalAmountIn')}</span>{currency}</span>
					<span>{Formatter.money(originalAmount, preferredCurrency)}</span>
				</div>
			{/if}

			<div class="form-control inline">
				<span>{accountLabel}</span>
				<span>{accountValue}</span>
			</div>

			{#if fromStocks}
				<div class="form-control inline">
					<span>{$t('soldStocks')}</span>
					<span class="expense-color">{Formatter.moneyPrecise(fromStocks, preferredCurrency)}</span>
				</div>
			{/if}

			{#if toStocks}
				<div class="form-control inline">
					<span>{$t('purchasedStocks')}</span>
					<span class="deposit-color">{Formatter.moneyPrecise(toStocks, preferredCurrency)}</span>
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
						<textarea bind:value={description} readonly />
					</div>
				</div>
			{/if}

			{#if isEncrypted}
				<div class="form-control">
					<div class="description-view encrypted">
						<span>{$t('description')}</span>

						<form on:submit={decrypt} class="decrypt-form">
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
							<button
								type="button"
								on:click={decrypt}
								class="decrypt-button"
								class:loading={decryptButtonIsLoading}
								title={$t('decryptDescription')}
								aria-label={$t('decryptDescription')}
							>
								<i class="fas fa-unlock" />
								<i class="fas fa-circle-notch fa-spin" />
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
