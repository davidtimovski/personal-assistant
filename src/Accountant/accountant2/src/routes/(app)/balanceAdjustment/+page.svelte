<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import { ValidationResult, ValidationUtil } from '../../../../../../Core/shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user, alertState } from '$lib/stores';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { AccountsService } from '$lib/services/accountsService';
	import { TransactionsService } from '$lib/services/transactionsService';

	import AmountInput from '$lib/components/AmountInput.svelte';

	let accountId: number | null = $state(null);
	let balance: number | null = $state(null);
	let description = $state('');
	let originalBalance: number | null = $state(null);
	let currency: string | null = $state(null);
	let accountOptions: SelectOption[] | null = $state(null);
	let balanceIsInvalid = $state(false);
	let adjustButtonIsLoading = $state(false);
	let balanceLoaded = $state(false);

	const localStorage = new LocalStorageUtil();
	let accountsService: AccountsService;
	let transactionsService: TransactionsService;

	let min = 0.01;
	let adjustedByLabel = $derived(() => {
		const adjustedBy = !balance || originalBalance === null ? 0 : balance - originalBalance;
		return (adjustedBy > 0 ? '+' : '') + Formatter.moneyWithoutCurrency(adjustedBy, currency, $user.culture);
	});

	$effect(() => {
		if (currency && accountId) {
			loadBalance();
		}
	});

	async function loadBalance() {
		balance = null;
		const accountBalance = await accountsService.getBalance(<number>accountId, <string>currency);
		originalBalance = accountBalance;
		balance = originalBalance;
		balanceLoaded = true;
	}

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (!ValidationUtil.sameOrHigher(balance, min)) {
			result.fail('balance');
		}

		return result;
	}

	async function adjust(event: Event) {
		if (originalBalance === null) {
			throw new Error("Original balance hasn't been initialized yet");
		}

		event.preventDefault();

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

				await transactionsService.adjust(<number>accountId, amount, description, <string>currency);

				alertState.update((x) => {
					x.showSuccess('balanceAdjustment.adjustmentSuccessful');
					return x;
				});
				goto('/dashboard');
			} catch {
				adjustButtonIsLoading = false;
			}
		} else {
			balanceIsInvalid = true;
			adjustButtonIsLoading = false;
		}
	}

	onMount(async () => {
		accountsService = new AccountsService();
		transactionsService = new TransactionsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		if (currency === 'MKD') {
			min = 1;
		}

		accountOptions = await accountsService.getNonInvestmentFundsAsOptions();
		const mainAccountId = <number>accountOptions[0].id;
		accountId = mainAccountId;

		description = $t('balanceAdjustment.balanceAdjustment');
	});

	onDestroy(() => {
		accountsService?.release();
		transactionsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-coins"></i>
		</div>
		<div class="page-title">{$t('balanceAdjustment.balanceAdjustment')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<form onsubmit={adjust}>
			<div class="form-control inline">
				<label for="balance">{$t('balance')}</label>
				<AmountInput bind:amount={balance} bind:currency invalid={balanceIsInvalid} inputId="balance" disabled={!balanceLoaded} />
			</div>

			<div class="form-control inline">
				<label for="account">{$t('account')}</label>
				<div class="loadable-select" class:loaded={accountOptions}>
					<select id="account" bind:value={accountId} disabled={!accountOptions} class="category-select">
						{#if accountOptions}
							{#each accountOptions as account}
								<option value={account.id}>{account.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin"></i>
				</div>
			</div>

			<div class="form-control inline">
				<span>{$t('balanceAdjustment.adjustedBy')}</span>
				<span> {adjustedByLabel()}</span>
			</div>

			<div class="form-control">
				<textarea bind:value={description} maxlength="500" class="description-textarea" placeholder={$t('description')} aria-label={$t('description')}
				></textarea>
			</div>

			<hr />

			<div class="save-delete-wrap">
				<button class="button primary-button" disabled={!balance || adjustButtonIsLoading}>
					<span class="button-loader" class:loading={adjustButtonIsLoading}>
						<i class="fas fa-circle-notch fa-spin"></i>
					</span>
					<span>{$t('balanceAdjustment.adjust')}</span>
				</button>

				<a href="/menu" class="button secondary-button">{$t('cancel')}</a>
			</div>
		</form>
	</div>
</section>
