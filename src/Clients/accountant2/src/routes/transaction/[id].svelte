<script context="module">
	// @ts-ignore
	export async function load({ params }) {
		return {
			props: {
				id: parseInt(params.id, 10)
			}
		};
	}
</script>

<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import { DateHelper } from '../../../../shared2/utils/dateHelper';
	import { ValidationUtil } from '../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { ViewTransaction } from '$lib/models/viewmodels/viewTransaction';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { AccountsService } from '$lib/services/accountsService';
	import { EncryptionService } from '$lib/services/encryptionService';

	import AlertBlock from '$lib/components/AlertBlock.svelte';

	export let id: number;

	let fromExpenditureHeatmap = false;
	let model: ViewTransaction | null = null;
	let currency: string;
	let language: string;
	let passwordShown = false;
	let decryptButtonIsLoading = false;
	let decryptionPasswordIsInvalid: boolean;
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
		if (!model) {
			return;
		}

		if (ValidationUtil.isEmptyOrWhitespace(model.decryptionPassword)) {
			decryptionPasswordIsInvalid = true;
			return;
		}

		decryptButtonIsLoading = true;

		try {
			if (!model.encryptedDescription || !model.salt || !model.nonce || !model.decryptionPassword) {
				throw new Error('Cannot decrypt. Description, salt, nonce, or password missing.');
			}

			const decryptedDescription = await encryptionService.decrypt(
				model.encryptedDescription,
				model.salt,
				model.nonce,
				model.decryptionPassword
			);

			model.isEncrypted = false;
			model.description = decryptedDescription;
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

		currency = localStorage.get('currency');
		language = localStorage.get('language');

		const transaction = await transactionsService.getForViewing(id, currency);
		if (transaction === null) {
			// TODO
			await goto('notFound');
		}

		if (transaction.isEncrypted) {
			passwordShowIconLabel = $t('showPassword');
		}

		const type = TransactionsService.getType(transaction.fromAccountId, transaction.toAccountId);
		let categoryName: string;
		if (transaction.categoryId) {
			const category = await categoriesService.get(transaction.categoryId);
			categoryName = category.fullName;
		} else {
			categoryName = $t('uncategorized');
		}

		const viewTransaction = new ViewTransaction(
			type,
			<string>typeStringMap.get(type),
			null,
			null,
			<number>transaction.convertedAmount,
			transaction.currency,
			transaction.amount,
			transaction.fromStocks,
			transaction.toStocks,
			categoryName,
			transaction.description,
			formatOccurrenceDate(transaction.date),
			transaction.isEncrypted,
			transaction.encryptedDescription,
			transaction.salt,
			transaction.nonce,
			transaction.generated,
			null
		);

		if (viewTransaction.type === TransactionType.Transfer) {
			const fromAccount = await accountsService.get(<number>transaction.fromAccountId);
			const toAccount = await accountsService.get(<number>transaction.toAccountId);
			viewTransaction.accountLabel = $t('transaction.accounts');
			viewTransaction.accountValue = $t('transaction.to', {
				from: fromAccount.name,
				to: toAccount.name
			});
		} else if (viewTransaction.type === TransactionType.Deposit) {
			const toAccount = await accountsService.get(<number>transaction.toAccountId);
			viewTransaction.accountLabel = $t('transaction.toAccount');
			viewTransaction.accountValue = toAccount.name;
		} else {
			const fromAccount = await accountsService.get(<number>transaction.fromAccountId);
			viewTransaction.accountLabel = $t('transaction.fromAccount');
			viewTransaction.accountValue = fromAccount.name;
		}

		model = viewTransaction;
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<a href="/editTransaction/{id}" class="side small" title={$t('edit')} aria-label={$t('edit')}>
			<i class="fas fa-pencil-alt" />
		</a>
		<div class="page-title">{model?.typeLabel}</div>
		<button type="button" on:click={back} class="back-button">
			<i class="fas fa-times" />
		</button>
	</div>

	<div class="content-wrap">
		{#if !model}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else}
			<div class="viewing">
				{#if model.generated}
					<AlertBlock type="info" message={$t('transaction.generatedAlert')} />
				{/if}

				<div class="form-control inline">
					<span>{$t('amount')}</span>
					<span
						class:expense-color={model.type === 1}
						class:deposit-color={model.type === 2}
						class:transfer-color={model.type === 3}>{Formatter.money(model.amount, currency)}</span
					>
				</div>

				{#if model.currency !== currency}
					<div class="form-control inline">
						<span><span>{$t('transaction.originalAmountIn')}</span>{model.currency}</span>
						<span>{Formatter.money(model.originalAmount, currency)}</span>
					</div>
				{/if}

				<div class="form-control inline">
					<span>{model.accountLabel}</span>
					<span>{model.accountValue}</span>
				</div>

				{#if model.fromStocks}
					<div class="form-control inline">
						<span>{$t('soldStocks')}</span>
						<span class="expense-color">{Formatter.moneyPrecise(model.fromStocks, currency)}</span>
					</div>
				{/if}

				{#if model.toStocks}
					<div class="form-control inline">
						<span>{$t('purchasedStocks')}</span>
						<span class="deposit-color">{Formatter.moneyPrecise(model.toStocks, currency)}</span>
					</div>
				{/if}

				<div class="form-control inline">
					<span>{$t('category')}</span>
					<span>{model.category}</span>
				</div>

				<div class="form-control inline">
					<span>{$t('date')}</span>
					<span>{model.date}</span>
				</div>

				{#if model.description}
					<div class="form-control">
						<div class="description-view">
							<span>{$t('description')}</span>
							<textarea bind:value={model.description} readonly />
						</div>
					</div>
				{/if}

				{#if model.isEncrypted}
					<div class="form-control">
						<div class="description-view encrypted">
							<span>{$t('description')}</span>

							<form on:submit={decrypt} class="decrypt-form">
								<div class="viewable-password">
									<input
										type="password"
										bind:this={passwordInput}
										bind:value={model.decryptionPassword}
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
		{/if}
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
