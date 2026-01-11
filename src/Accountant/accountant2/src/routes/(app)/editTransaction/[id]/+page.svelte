<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../../Core/shared2/utils/dateHelper';
	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';
	import Tooltip from '../../../../../../../Core/shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { alertState, isOnline, syncStatus } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { AccountsService } from '$lib/services/accountsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { EncryptionService } from '$lib/services/encryptionService';
	import { CategoryType } from '$lib/models/entities/category';
	import { TransactionModel } from '$lib/models/entities/transaction';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { SyncEvents } from '$lib/models/syncStatus';

	import AmountInput from '$lib/components/AmountInput.svelte';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let fromAccountId: number | null = $state(null);
	let fromAccountName = $state('---');
	let toAccountId: number | null = $state(null);
	let toAccountName = $state('---');
	let categoryId: number | null = $state(null);
	let amount: number | null = $state(null);
	let fromStocks: number | null = $state(null);
	let toStocks: number | null = $state(null);
	let currency: string | null = $state(null);
	let description: string | null = $state(null);
	let date = $state(DateHelper.format(new Date()));
	let isEncrypted = $state(false);
	let encryptedDescription: string | null = null;
	let salt: string | null = null;
	let nonce: string | null = null;
	let generated = $state(false);
	let decryptionPassword: string | null = $state(null);
	let encrypt = $state(false);
	let encryptionPassword: string | null = $state(null);
	let createdDate: Date | null = null;
	let synced = $state(false);
	let type: TransactionType | null = $state(null);
	let accountOptions: SelectOption[] | null = $state(null);
	let categoryOptions: SelectOption[] | null = $state(null);
	const maxDate = date;
	let decPasswordShown = $state(false);
	let encPasswordShown = $state(false);
	let amountIsInvalid = $state(false);
	let dateIsInvalid = $state(false);
	let decryptionPasswordIsInvalid = $state(false);
	let encryptionPasswordIsInvalid = $state(false);
	let decryptButtonIsLoading = $state(false);
	let deleteInProgress = $state(false);
	let deleteButtonText = $state('');
	let saveButtonIsLoading = $state(false);
	let deleteButtonIsLoading = $state(false);
	let encryptPasswordInput: HTMLInputElement | null = $state(null);
	let decryptPasswordInput: HTMLInputElement | null = $state(null);
	let passwordShowIconLabel = $state('');
	let stockPurchase = $state(false);
	let stockSelling = $state(false);

	let transactionsService: TransactionsService;
	let accountsService: AccountsService;
	let categoriesService: CategoriesService;
	let encryptionService: EncryptionService;

	let amountFrom = 0.01;
	let amountTo = 8000000;

	const alertUnsubscriber = alertState.subscribe((value) => {
		if (value.hidden) {
			amountIsInvalid = false;
			dateIsInvalid = false;
		}
	});

	let canEncrypt = $derived(!!description);

	$effect(() => {
		if (!description) {
			encrypt = false;
			encryptionPassword = null;
		}

		if (!canEncrypt) {
			encrypt = false;
			encryptionPassword = null;
		}
	});

	function toggleDecPasswordShow() {
		if (!decryptPasswordInput) {
			return;
		}

		if (decPasswordShown) {
			decryptPasswordInput.type = 'password';
			passwordShowIconLabel = $t('showPassword');
		} else {
			decryptPasswordInput.type = 'text';
			passwordShowIconLabel = $t('hidePassword');
		}

		decPasswordShown = !decPasswordShown;
	}

	function toggleEncPasswordShow() {
		if (!encryptPasswordInput) {
			return;
		}

		if (encPasswordShown) {
			encryptPasswordInput.type = 'password';
			passwordShowIconLabel = $t('showPassword');
		} else {
			encryptPasswordInput.type = 'text';
			passwordShowIconLabel = $t('hidePassword');
		}

		encPasswordShown = !encPasswordShown;
	}

	async function decrypt() {
		if (ValidationUtil.isEmptyOrWhitespace(decryptionPassword)) {
			decryptionPasswordIsInvalid = true;
			return;
		}

		decryptButtonIsLoading = true;

		try {
			const decryptedDescription = await encryptionService.decrypt(
				<string>encryptedDescription,
				<string>salt,
				<string>nonce,
				<string>decryptionPassword
			);

			description = decryptedDescription;
			isEncrypted = false;
			encryptionPassword = decryptionPassword;
			decryptionPassword = null;
			decryptButtonIsLoading = false;
		} catch {
			decryptionPasswordIsInvalid = true;
			decryptButtonIsLoading = false;
		}
	}

	function erase() {
		isEncrypted = false;
		decryptionPassword = null;
	}

	let canSave = $derived($syncStatus.status !== SyncEvents.SyncStarted && !!amount && !(!$isOnline && synced));

	async function save(event: Event) {
		event.preventDefault();

		if (!amount || !currency) {
			throw new Error('Unexpected error: required fields missing');
		}

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = TransactionsService.validate(amount, amountFrom, amountTo, fromAccountId, toAccountId, date, encrypt, encryptionPassword);
		if (result.valid) {
			amountIsInvalid = false;
			dateIsInvalid = false;

			try {
				const transaction = new TransactionModel(
					data.id,
					fromAccountId,
					toAccountId,
					categoryId,
					amount,
					fromStocks,
					toStocks,
					currency,
					description,
					date,
					encrypt,
					encryptedDescription,
					salt,
					nonce,
					generated,
					createdDate,
					null
				);

				await transactionsService.update(transaction, encryptionPassword);

				goto('/transactions?edited=' + data.id);
			} catch {
				saveButtonIsLoading = false;
			}
		} else {
			const messages = new Array<string>();

			const invalidAmount = result.erroredFields.includes('amount');
			if (invalidAmount) {
				messages.push($t('amountBetween', { from: amountFrom, to: amountTo }));
			}

			const accountsMissing = result.erroredFields.includes('accountsMissing');
			if (accountsMissing) {
				messages.push($t('editTransaction.anAccountIsRequired'));
			}

			const accountsEqual = result.erroredFields.includes('accountsEqual');
			if (accountsEqual) {
				messages.push($t('editTransaction.accountsCannotBeEqual'));
			}

			const invalidDate = result.erroredFields.includes('date');
			if (invalidDate) {
				messages.push($t('dateIsRequired'));
			}

			const invalidEncryptionPassword = result.erroredFields.includes('encryptionPassword');
			if (invalidEncryptionPassword) {
				messages.push($t('passwordIsRequired'));
			}

			if (messages.length > 0) {
				dateIsInvalid = !!invalidDate;
				encryptionPasswordIsInvalid = !!invalidEncryptionPassword;

				alertState.update((x) => {
					x.showErrors(messages);
					return x;
				});
			}

			saveButtonIsLoading = false;
		}
	}

	async function deleteTransaction() {
		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await transactionsService.delete(data.id);
				alertState.update((x) => {
					x.showSuccess('editTransaction.deleteSuccessful');
					return x;
				});
				goto('/transactions');
			} catch {
				deleteButtonText = $t('delete');
				deleteInProgress = false;
				deleteButtonIsLoading = false;
				return;
			}
		} else {
			deleteButtonText = $t('sure');
			deleteInProgress = true;
		}
	}

	function cancel() {
		deleteButtonText = $t('delete');
		deleteInProgress = false;
	}

	onMount(async () => {
		deleteButtonText = $t('delete');
		passwordShowIconLabel = $t('showPassword');

		transactionsService = new TransactionsService();
		accountsService = new AccountsService();
		categoriesService = new CategoriesService();
		encryptionService = new EncryptionService();

		categoriesService.getAllAsOptions($t('uncategorized'), CategoryType.AllTransactions).then((options) => {
			categoryOptions = options;
		});

		const transaction = await transactionsService.get(data.id);
		if (transaction === null) {
			throw new Error('Transaction not found');
		}

		type = TransactionsService.getType(transaction.fromAccountId, transaction.toAccountId);

		if (type === TransactionType.Transfer && (transaction.fromStocks || transaction.toStocks)) {
			accountsService.getAllAsOptions().then((options) => {
				options.unshift(new SelectOption(null, $t('editTransaction.none')));
				accountOptions = options;

				// Set names because they're uneditable when funds are involved
				fromAccountName = <string>options?.find((x) => x.id === fromAccountId)?.name;
				toAccountName = <string>options?.find((x) => x.id === toAccountId)?.name;
			});
		} else {
			accountsService.getNonInvestmentFundsAsOptions().then((options) => {
				if (type === TransactionType.Transfer) {
					options.unshift(new SelectOption(null, $t('editTransaction.none')));
				}

				accountOptions = options;
			});
		}

		fromAccountId = transaction.fromAccountId;
		toAccountId = transaction.toAccountId;
		categoryId = transaction.categoryId;
		amount = transaction.amount;
		fromStocks = transaction.fromStocks;
		toStocks = transaction.toStocks;
		currency = transaction.currency;
		description = transaction.description;
		date = transaction.date.slice(0, 10);
		isEncrypted = transaction.isEncrypted;
		encryptedDescription = transaction.encryptedDescription;
		salt = transaction.salt;
		nonce = transaction.nonce;
		generated = transaction.generated;
		isEncrypted = transaction.isEncrypted;
		createdDate = transaction.createdDate;
		synced = transaction.synced;
		stockPurchase = !!transaction.toStocks;
		stockSelling = !!transaction.fromStocks;

		if (currency === 'MKD') {
			amountFrom = 1;
			amountTo = 450000000;
		}
	});

	onDestroy(() => {
		alertUnsubscriber();
		transactionsService?.release();
		accountsService?.release();
		categoriesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt"></i>
		</div>
		<div class="page-title">
			{#if type === 1}
				<span>{$t('editTransaction.editExpense')}</span>
			{/if}
			{#if type === 2}
				<span>{$t('editTransaction.editDeposit')}</span>
			{/if}
			{#if type !== 1 && type !== 2}
				<span>{$t('editTransaction.editTransaction')}</span>
			{/if}
		</div>
		<a href="/transaction/{data.id}" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div>
			{#if !$isOnline && synced}
				<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
			{/if}

			<form onsubmit={save} autocomplete="off">
				<div class="form-control inline">
					<label for="amount">{$t('amount')}</label>
					<AmountInput bind:amount bind:currency invalid={amountIsInvalid} />
				</div>

				{#if type === TransactionType.Expense || type === TransactionType.Transfer}
					<div class="form-control inline">
						{#if stockPurchase || stockSelling}
							<span>{$t('fromAccount')}</span>
							<span>{fromAccountName}</span>
						{:else}
							<label for="from-account">{$t('fromAccount')}</label>
							<div class="loadable-select" class:loaded={accountOptions}>
								<select id="from-account" bind:value={fromAccountId} disabled={!accountOptions} class="category-select">
									{#if accountOptions}
										{#each accountOptions as account}
											<option value={account.id}>{account.name}</option>
										{/each}
									{/if}
								</select>
								<i class="fas fa-circle-notch fa-spin"></i>
							</div>
						{/if}
					</div>
				{/if}

				{#if type === TransactionType.Deposit || type === TransactionType.Transfer}
					<div class="form-control inline">
						{#if stockPurchase || stockSelling}
							<span>{$t('toAccount')}</span>
							<span>{toAccountName}</span>
						{:else}
							<label for="to-account">{$t('toAccount')}</label>
							<div class="loadable-select" class:loaded={accountOptions}>
								<select id="to-account" bind:value={toAccountId} disabled={!accountOptions} class="category-select">
									{#if accountOptions}
										{#each accountOptions as account}
											<option value={account.id}>{account.name}</option>
										{/each}
									{/if}
								</select>
								<i class="fas fa-circle-notch fa-spin"></i>
							</div>
						{/if}
					</div>
				{/if}

				{#if stockSelling}
					<div class="form-control inline">
						<label for="sold-stocks">{$t('soldStocks')}</label>
						<input type="number" id="sold-stocks" bind:value={fromStocks} min="0.0001" max="100000" step="0.0001" class="stocks-input" required />
					</div>
				{/if}

				{#if stockPurchase}
					<div class="form-control inline">
						<label for="purchased-stocks">{$t('purchasedStocks')}</label>
						<input type="number" id="purchased-stocks" bind:value={toStocks} min="0.0001" max="100000" step="0.0001" class="stocks-input" required />
					</div>
				{/if}

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
						<i class="fas fa-circle-notch fa-spin"></i>
					</div>
				</div>

				<div class="form-control inline">
					<label for="date">{$t('date')}</label>
					<input type="date" id="date" bind:value={date} max={maxDate} class:invalid={dateIsInvalid} required />
				</div>

				{#if isEncrypted}
					<div class="inline-decrypt-form">
						<div class="viewable-password">
							<input
								type="password"
								bind:this={decryptPasswordInput}
								bind:value={decryptionPassword}
								maxlength="100"
								class:invalid={decryptionPasswordIsInvalid}
								placeholder={$t('password')}
								aria-label={$t('password')}
							/>
							<button type="button" onclick={toggleDecPasswordShow} class="password-show-button" class:shown={decPasswordShown}>
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
						<button
							type="button"
							onclick={erase}
							class="erase-button"
							title={$t('editTransaction.resetDescription')}
							aria-label={$t('editTransaction.resetDescription')}
						>
							<i class="fas fa-eraser"></i>
						</button>
					</div>
				{:else}
					<div class="form-control">
						<div class="encryption-box" class:active={encrypt}>
							<textarea
								bind:value={description}
								maxlength="500"
								class="description-textarea"
								placeholder={$t('description')}
								aria-label={$t('description')}
							></textarea>

							<Checkbox labelKey="editTransaction.encryptDescription" bind:value={encrypt} disabled={!canEncrypt} />

							{#if encrypt}
								<div>
									<Tooltip key="encryptedDescription" application="Accountant" />

									<div class="viewable-password">
										<input
											type="password"
											bind:this={encryptPasswordInput}
											bind:value={encryptionPassword}
											disabled={!encrypt}
											maxlength="100"
											class:invalid={encryptionPasswordIsInvalid}
											placeholder={$t('password')}
											aria-label={$t('password')}
										/>
										<button
											type="button"
											onclick={toggleEncPasswordShow}
											class="password-show-button"
											class:shown={encPasswordShown}
											title={passwordShowIconLabel}
											aria-label={passwordShowIconLabel}
										>
											<i class="fas fa-eye"></i>
											<i class="fas fa-eye-slash"></i>
										</button>
									</div>
								</div>
							{/if}
						</div>
					</div>
				{/if}

				<hr />

				<div class="save-delete-wrap">
					{#if !deleteInProgress}
						<button class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
							<span class="button-loader" class:loading={saveButtonIsLoading}>
								<i class="fas fa-circle-notch fa-spin"></i>
							</span>
							<span>{$t('save')}</span>
						</button>
					{/if}

					<button
						type="button"
						onclick={deleteTransaction}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={deleteInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{deleteButtonText}</span>
					</button>

					{#if deleteInProgress}
						<button type="button" onclick={cancel} class="button secondary-button">
							{$t('cancel')}
						</button>
					{/if}
				</div>
			</form>
		</div>
	</div>
</section>

<style lang="scss">
	input[type='number'].stocks-input {
		width: 100px;
		text-align: center;
	}

	.erase-button {
		background: var(--warning-color-dark);
		border: none;
		border-radius: var(--border-radius);
		outline: none;
		padding: 0 10px;
		margin-left: 10px;
		font-size: 1.3rem;
		line-height: 37px;
		color: #fff;
	}

	@media screen and (min-width: 1200px) {
		.erase-button {
			line-height: 45px;
		}
	}
</style>
