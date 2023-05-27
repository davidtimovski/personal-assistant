<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../../Core/shared2/utils/dateHelper';
	import { ValidationResult, ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
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

	export let data: PageData;

	let fromAccountId: number | null = null;
	let fromAccountName = '---';
	let toAccountId: number | null = null;
	let toAccountName = '---';
	let categoryId: number | null = null;
	let amount: number | null = null;
	let fromStocks: number | null = null;
	let toStocks: number | null = null;
	let currency: string | null = null;
	let description: string | null = null;
	let date = DateHelper.format(new Date());
	let isEncrypted: boolean;
	let encryptedDescription: string | null = null;
	let salt: string | null = null;
	let nonce: string | null = null;
	let generated: boolean;
	let decryptionPassword: string | null = null;
	let encrypt = false;
	let encryptionPassword: string | null = null;
	let createdDate: Date | null = null;
	let synced: boolean;
	let type: TransactionType;
	let accountOptions: SelectOption[] | null = null;
	let categoryOptions: SelectOption[] | null = null;
	const maxDate = date;
	let decPasswordShown = false;
	let encPasswordShown = false;
	let amountIsInvalid = false;
	let dateIsInvalid = false;
	let decryptionPasswordIsInvalid = false;
	let encryptionPasswordIsInvalid = false;
	let decryptButtonIsLoading = false;
	let deleteInProgress = false;
	let deleteButtonText: string;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;
	let encryptPasswordInput: HTMLInputElement | null = null;
	let decryptPasswordInput: HTMLInputElement | null = null;
	let passwordShowIconLabel: string;
	let stockPurchase = false;
	let stockSelling = false;

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

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
			result.fail('amount');
		}

		if (!fromAccountId && !toAccountId) {
			result.fail('accountsMissing');
		} else if (fromAccountId === toAccountId) {
			result.fail('accountsEqual');
		}

		if (!date) {
			result.fail('date');
		}

		if (encrypt && ValidationUtil.isEmptyOrWhitespace(encryptionPassword)) {
			result.fail('encryptionPassword');
		}

		return result;
	}

	$: canSave = $syncStatus.status !== SyncEvents.SyncStarted && !!amount && !(!$isOnline && synced);

	async function save() {
		if (!amount || !currency) {
			throw new Error('Unexpected error: required fields missing');
		}

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();
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
			<i class="fas fa-pencil-alt" />
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
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div>
			{#if !$isOnline && synced}
				<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
			{/if}

			<form on:submit|preventDefault={save} autocomplete="off">
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
								<i class="fas fa-circle-notch fa-spin" />
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
								<i class="fas fa-circle-notch fa-spin" />
							</div>
						{/if}
					</div>
				{/if}

				{#if stockSelling}
					<div class="form-control inline">
						<label for="sold-stocks">{$t('soldStocks')}</label>
						<input
							type="number"
							id="sold-stocks"
							bind:value={fromStocks}
							min="0.0001"
							max="100000"
							step="0.0001"
							class="stocks-input"
							required
						/>
					</div>
				{/if}

				{#if stockPurchase}
					<div class="form-control inline">
						<label for="purchased-stocks">{$t('purchasedStocks')}</label>
						<input
							type="number"
							id="purchased-stocks"
							bind:value={toStocks}
							min="0.0001"
							max="100000"
							step="0.0001"
							class="stocks-input"
							required
						/>
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
						<i class="fas fa-circle-notch fa-spin" />
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
							<button
								type="button"
								on:click={toggleDecPasswordShow}
								class="password-show-button"
								class:shown={decPasswordShown}
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
						<button
							type="button"
							on:click={erase}
							class="erase-button"
							title={$t('editTransaction.resetDescription')}
							aria-label={$t('editTransaction.resetDescription')}
						>
							<i class="fas fa-eraser" />
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
							/>

							<Checkbox labelKey="editTransaction.encryptDescription" bind:value={encrypt} disabled={!canEncrypt()} />

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
											on:click={toggleEncPasswordShow}
											class="password-show-button"
											class:shown={encPasswordShown}
											title={passwordShowIconLabel}
											aria-label={passwordShowIconLabel}
										>
											<i class="fas fa-eye" />
											<i class="fas fa-eye-slash" />
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
								<i class="fas fa-circle-notch fa-spin" />
							</span>
							<span>{$t('save')}</span>
						</button>
					{/if}

					<button
						type="button"
						on:click={deleteTransaction}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={deleteInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{deleteButtonText}</span>
					</button>

					{#if deleteInProgress}
						<button type="button" on:click={cancel} class="button secondary-button">
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
