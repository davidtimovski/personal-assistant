<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../shared2/utils/dateHelper';
	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { alertState } from '$lib/stores';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { AccountsService } from '$lib/services/accountsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { DebtsService } from '$lib/services/debtsService';
	import { CategoryType } from '$lib/models/entities/category';

	import Checkbox from '$lib/components/Checkbox.svelte';
	import AmountInput from '$lib/components/AmountInput.svelte';
	import AlertBlock from '$lib/components/AlertBlock.svelte';
	import Tooltip from '$lib/components/Tooltip.svelte';

	export let data: PageData;

	let accountId: number | null = null;
	let categoryId: number | null = null;
	let amount: number | null = null;
	let currency: string | null = null;
	let description: string | null = null;
	let date = DateHelper.format(new Date());
	let encrypt = false;
	let encryptionPassword: string | null = null;
	let debtId: number | null = null;
	let userIsDebtor: boolean;
	let debtPerson: string;
	let accountOptions: SelectOption[] | null = null;
	let categoryOptions: SelectOption[] | null = null;
	const maxDate = date;
	let passwordShown = false;
	let amountIsInvalid = false;
	let dateIsInvalid = false;
	let encryptionPasswordIsInvalid = false;
	let submitButtonIsLoading = false;
	let passwordInput: HTMLInputElement | null = null;
	let passwordShowIconLabel: string;

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let accountsService: AccountsService;
	let categoriesService: CategoriesService;
	let debtsService: DebtsService;

	let amountFrom = 0.01;
	let amountTo = 8000000;

	$: pastMidnight = (): string | null => {
		if (!date) {
			return null;
		}

		const now = new Date();
		const selectedDate = new Date(date);

		const isToday =
			selectedDate.getDate() == now.getDate() &&
			selectedDate.getMonth() == now.getMonth() &&
			selectedDate.getFullYear() == now.getFullYear();

		if (isToday) {
			const hour = now.getHours();
			if (hour < 4) {
				return DateHelper.formatHoursMinutes(now);
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
		const result = new ValidationResult(true);

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
				goto('/');
			} catch {
				submitButtonIsLoading = false;
			}
		} else {
			const messages = new Array<string>();

			const invalidAmount = result.erroredFields.includes('amount');
			if (invalidAmount) {
				messages.push($t('amountBetween', { from: amountFrom, to: amountTo }));
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
				amountIsInvalid = !!invalidAmount;
				dateIsInvalid = !!invalidDate;
				encryptionPasswordIsInvalid = !!invalidEncryptionPassword;

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

		alertState.subscribe((value) => {
			if (value.hidden) {
				amountIsInvalid = false;
				dateIsInvalid = false;
				encryptionPasswordIsInvalid = false;
			}
		});

		const debtIdParam = $page.url.searchParams.get('debtId');
		if (debtIdParam) {
			debtId = parseInt(debtIdParam, 10);
		}

		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		accountsService = new AccountsService();
		categoriesService = new CategoriesService();
		debtsService = new DebtsService();

		currency = localStorage.get('currency');

		if (currency === 'MKD') {
			amountFrom = 1;
			amountTo = 450000000;
		}

		const categoryType = data.isExpense ? CategoryType.ExpenseOnly : CategoryType.DepositOnly;

		accountsService.getNonInvestmentFundsAsOptions().then(async (options) => {
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

		<a href={debtId ? '/debt' : '/'} class="back-button" role="button">
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

		<form on:submit={submit} autocomplete="off">
			<div class="form-control inline">
				<label for="amount">{$t('amount')}</label>
				<AmountInput bind:amount bind:currency invalid={amountIsInvalid} focusOnInit={true} />
			</div>

			<div class="form-control inline">
				<label for="account">{$t(data.isExpense ? 'fromAccount' : 'toAccount')}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select id="account" bind:value={accountId} disabled={!accountOptions} class="category-select">
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
						<Tooltip key="encryptedDescription" />

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
					type="button"
					on:click={submit}
					class="button primary-button"
					disabled={!amount || !accountId || submitButtonIsLoading}
				>
					<span class="button-loader" class:loading={submitButtonIsLoading}>
						<i class="fas fa-circle-notch fa-spin" />
					</span>
					<span>{$t('newTransaction.submit')}</span>
				</button>
				<a href={debtId ? '/debt' : '/'} class="button secondary-button">{$t('cancel')}</a>
			</div>
		</form>
	</div>
</section>
