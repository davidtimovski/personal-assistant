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

	import { DateHelper } from '../../../../shared2/utils/dateHelper';
	import { ValidationResult, ValidationUtil } from '../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { alertState, isOnline } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import type { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { EncryptionService } from '$lib/services/encryptionService';
	import { CategoryType } from '$lib/models/entities/category';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';

	import Checkbox from '$lib/components/Checkbox.svelte';
	import AmountInput from '$lib/components/AmountInput.svelte';
	import AlertBlock from '$lib/components/AlertBlock.svelte';
	import { TransactionModel } from '$lib/models/entities/transaction';

	export let id: number;

	let fromAccountId: number | null = null;
	let toAccountId: number | null = null;
	let categoryId: number | null = null;
	let amount: number;
	let fromStocks: number | null = null;
	let toStocks: number | null = null;
	let currency: string;
	let description: string | null = null;
	let date: string;
	let isEncrypted: boolean;
	let encryptedDescription: string | null = null;
	let salt: string | null = null;
	let nonce: string | null = null;
	let generated: boolean;
	let decryptionPassword: string | null = null;
	let encrypt = false;
	let encryptionPassword: string | null = null;
	let createdDate: Date;
	let synced: boolean;
	let type: TransactionType;
	let categoryOptions: SelectOption[] | null = null;
	let maxDate: string;
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
	let passwordShowIconLabel: string;

	let transactionsService: TransactionsService;
	let categoriesService: CategoriesService;
	let encryptionService: EncryptionService;

	let amountFrom = 0.01;
	let amountTo = 8000001;

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
		const passwordInput = <HTMLInputElement>document.getElementById('decrypt-password-input');
		if (decPasswordShown) {
			passwordInput.type = 'password';
			passwordShowIconLabel = $t('showPassword');
		} else {
			passwordInput.type = 'text';
			passwordShowIconLabel = $t('hidePassword');
		}

		decPasswordShown = !decPasswordShown;
	}

	function toggleEncPasswordShow() {
		const passwordInput = <HTMLInputElement>document.getElementById('encrypt-password-input');
		if (encPasswordShown) {
			passwordInput.type = 'password';
			passwordShowIconLabel = $t('showPassword');
		} else {
			passwordInput.type = 'text';
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
		if (!currency) {
			return new ValidationResult(false);
		}

		const result = new ValidationResult(true);

		if (!amount) {
			result.fail('amount');
			return result;
		}

		if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
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

	$: canSave = () => {
		return !!amount && !(!$isOnline && synced);
	};

	async function save() {
		if (!amount || saveButtonIsLoading) {
			return;
		}

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();
		if (result.valid) {
			try {
				const transaction = new TransactionModel(
					id,
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

				await transactionsService.update(transaction, <string>encryptionPassword);
				amountIsInvalid = false;
				dateIsInvalid = false;

				goto('/transactions?edited=' + id);
			} catch {
				saveButtonIsLoading = false;
			}
		} else {
			const messages = new Array<string>();

			const invalidAmount = result.erroredFields.includes('amount');
			if (invalidAmount) {
				messages.push($t('amountBetween', { from: amountFrom, to: amountTo }));
			}

			const invalidDate = result.erroredFields.includes('date');
			if (invalidDate) {
				messages.push($t('newTransaction.dateIsRequired'));
			}

			const invalidEncryptionPassword = result.erroredFields.includes('encryptionPassword');
			if (invalidEncryptionPassword) {
				messages.push($t('newTransaction.passwordIsRequired'));
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
		if (deleteButtonIsLoading) {
			return;
		}

		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await transactionsService.delete(id);
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

	onMount(() => {
		maxDate = date = DateHelper.format(new Date());
		deleteButtonText = $t('delete');
		passwordShowIconLabel = $t('showPassword');

		alertState.subscribe((value) => {
			if (value.hidden) {
				amountIsInvalid = false;
				dateIsInvalid = false;
			}
		});

		transactionsService = new TransactionsService();
		categoriesService = new CategoriesService();
		encryptionService = new EncryptionService();

		categoriesService.getAllAsOptions($t('uncategorized'), CategoryType.AllTransactions).then((options) => {
			categoryOptions = options;
		});

		transactionsService.get(id).then(async (transaction: TransactionModel) => {
			if (transaction === null) {
				// TODO
				await goto('notFound');
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

			if (currency === 'MKD') {
				amountFrom = 0;
				amountTo = 450000001;
			}

			type = TransactionsService.getType(transaction.fromAccountId, transaction.toAccountId);
		});
	});
</script>

<section>
	<div class="container">
		<div class="au-animate animate-fade-in animate-fade-out">
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
				<a href="/transaction/{id}" class="back-button">
					<i class="fas fa-times" />
				</a>
			</div>

			<div class="content-wrap">
				<div>
					{#if !$isOnline && synced}
						<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
					{/if}

					<form on:submit={save} autocomplete="off">
						<div class="form-control inline">
							<label for="amount">{$t('amount')}</label>
							<AmountInput bind:amount bind:currency invalid={amountIsInvalid} />
						</div>

						{#if fromStocks !== null}
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

						{#if toStocks !== null}
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
										bind:value={decryptionPassword}
										id="decrypt-password-input"
										maxlength="100"
										class:invalid={decryptionPasswordIsInvalid}
										placeholder={$t('password')}
										aria-label={$t('password')}
									/>
									<a
										on:click={toggleDecPasswordShow}
										class="password-show-button"
										class:shown={decPasswordShown}
										role="button"
									>
										<i class="fas fa-eye" />
										<i class="fas fa-eye-slash" />
									</a>
								</div>
								<a
									on:click={decrypt}
									class="decrypt-button"
									class:loading={decryptButtonIsLoading}
									role="button"
									title={$t('decryptDescription')}
									aria-label={$t('decryptDescription')}
								>
									<i class="fas fa-unlock" />
									<i class="fas fa-circle-notch fa-spin" />
								</a>
								<a
									on:click={erase}
									class="erase-button"
									role="button"
									title={$t('editTransaction.resetDescription')}
									aria-label={$t('editTransaction.resetDescription')}
								>
									<i class="fas fa-eraser" />
								</a>
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

									<Checkbox
										labelKey="editTransaction.encryptDescription"
										bind:value={encrypt}
										disabled={!canEncrypt()}
									/>

									{#if encrypt}
										<div>
											<!-- TODO -->
											<!-- <tooltip key="encryptedDescription" /> -->
											<div class="viewable-password">
												<input
													type="password"
													bind:value={encryptionPassword}
													id="encrypt-password-input"
													disabled={!encrypt}
													maxlength="100"
													class:invalid={encryptionPasswordIsInvalid}
													placeholder={$t('password')}
													aria-label={$t('password')}
												/>
												<a
													on:click={toggleEncPasswordShow}
													class="password-show-button"
													class:shown={encPasswordShown}
													role="button"
													title={passwordShowIconLabel}
													aria-label={passwordShowIconLabel}
												>
													<i class="fas fa-eye" />
													<i class="fas fa-eye-slash" />
												</a>
											</div>
										</div>
									{/if}
								</div>
							</div>
						{/if}

						<hr />

						<div class="save-delete-wrap">
							{#if !deleteInProgress}
								<a
									on:click={save}
									class="button primary-button"
									class:disabled={!canSave() || saveButtonIsLoading}
									role="button"
								>
									<span class="button-loader" class:loading={saveButtonIsLoading}>
										<i class="fas fa-circle-notch fa-spin" />
									</span>
									<span>{$t('save')}</span>
								</a>
							{/if}

							<a
								on:click={deleteTransaction}
								class="button danger-button"
								class:disabled={deleteButtonIsLoading}
								class:confirm={deleteInProgress}
								role="button"
							>
								<span class="button-loader" class:loading={deleteButtonIsLoading}>
									<i class="fas fa-circle-notch fa-spin" />
								</span>
								<span>{deleteButtonText}</span>
							</a>
							{#if deleteInProgress}
								<button type="button" on:click={cancel} class="button secondary-button">
									{$t('cancel')}
								</button>
							{/if}
						</div>
					</form>
				</div>
			</div>
		</div>
	</div>
</section>

<style lang="scss">
	input[type='number'].stocks-input {
		width: 100px;
		text-align: center;
	}
</style>
