<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { ValidationResult, ValidationUtil } from '../../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { alertState } from '$lib/stores';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { AccountsService } from '$lib/services/accountsService';
	import { TransactionsService } from '$lib/services/transactionsService';

	import AmountInput from '$lib/components/AmountInput.svelte';

	let accountId: number;
	let balance: number | null = null;
	let description: string;
	let originalBalance: number;
	let currency: string | null = null;
	let accountOptions: SelectOption[] | null = null;
	let balanceIsInvalid = false;
	let adjustButtonIsLoading = false;

	let localStorage: LocalStorageUtil;
	let accountsService: AccountsService;
	let transactionsService: TransactionsService;

	let min = 0.01;

	async function accountChanged() {
		const accountBalance = await accountsService.getBalance(accountId, <string>currency);
		originalBalance = balance = accountBalance;
	}

	$: adjustedBy = !balance ? 0 : balance - originalBalance;

	function validate(): ValidationResult {
		const result = new ValidationResult(true);

		if (!ValidationUtil.sameOrHigher(balance, min)) {
			result.fail('balance');
		}

		return result;
	}

	async function adjust() {
		adjustButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();
		if (result.valid) {
			balanceIsInvalid = false;

			try {
				const amount = parseFloat(<any>balance) - originalBalance;

				await transactionsService.adjust(accountId, amount, description, <string>currency);

				alertState.update((x) => {
					x.showSuccess('balanceAdjustment.adjustmentSuccessful');
					return x;
				});
				goto('/');
			} catch {
				adjustButtonIsLoading = false;
			}
		} else {
			balanceIsInvalid = true;
			adjustButtonIsLoading = false;
		}
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		accountsService = new AccountsService();
		transactionsService = new TransactionsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		if (currency === 'MKD') {
			min = 1;
		}

		accountOptions = await accountsService.getNonInvestmentFundsAsOptions();
		const mainAccountId = <number>accountOptions[0].id;

		const accountBalance = await accountsService.getBalance(mainAccountId, currency);
		originalBalance = accountBalance;

		accountId = mainAccountId;
		balance = accountBalance;
		description = $t('balanceAdjustment.balanceAdjustment');
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-coins" />
		</div>
		<div class="page-title">{$t('balanceAdjustment.balanceAdjustment')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form on:submit|preventDefault={adjust}>
			<div class="form-control inline">
				<label for="balance">{$t('balance')}</label>
				<AmountInput bind:amount={balance} bind:currency invalid={balanceIsInvalid} inputId="balance" />
			</div>

			<div class="form-control inline">
				<label for="account">{$t('account')}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select
						id="account"
						bind:value={accountId}
						on:change={accountChanged}
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

			<div class="form-control inline">
				<span>{$t('balanceAdjustment.adjustedBy')}</span>
				<span> {(adjustedBy > 0 ? '+' : '') + Formatter.number(adjustedBy, currency)}</span>
			</div>

			<div class="form-control">
				<textarea
					bind:value={description}
					maxlength="500"
					class="description-textarea"
					placeholder={$t('description')}
					aria-label={$t('description')}
				/>
			</div>

			<hr />

			<div class="save-delete-wrap">
				<button class="button primary-button" disabled={!balance || adjustButtonIsLoading}>
					<span class="button-loader" class:loading={adjustButtonIsLoading}>
						<i class="fas fa-circle-notch fa-spin" />
					</span>
					<span>{$t('balanceAdjustment.adjust')}</span>
				</button>

				<a href="/menu" class="button secondary-button">{$t('cancel')}</a>
			</div>
		</form>
	</div>
</section>
