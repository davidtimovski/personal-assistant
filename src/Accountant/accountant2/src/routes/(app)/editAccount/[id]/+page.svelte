<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { alertState, isOnline, syncStatus } from '$lib/stores';
	import { AccountsService } from '$lib/services/accountsService';
	import { Account } from '$lib/models/entities/account';
	import { SyncEvents } from '$lib/models/syncStatus';

	import StockPriceInput from '$lib/components/StockPriceInput.svelte';

	export let data: PageData;

	const isNew = data.id === 0;

	let name: string;
	let currency: string;
	let isMain: boolean;
	let stockPrice: number | null;
	let createdDate: Date | null;
	let modifiedDate: Date | null;
	let synced = false;
	let isMainAccount: boolean;
	let nameInput: HTMLInputElement;
	let nameIsInvalid = false;
	let stockPriceIsInvalid = false;
	let investmentFund: boolean;
	let investmentFundCheckboxDisabled = true;
	let saveButtonText: string;
	let transactionsWarningVisible = false;
	let deleteInProgress = false;
	let deleteButtonText: string;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;
	let accountHasTransactionsHtml: string;

	const localStorage = new LocalStorageUtil();
	let accountsService: AccountsService;

	const alertUnsubscriber = alertState.subscribe((value) => {
		if (value.hidden) {
			nameIsInvalid = false;
			stockPriceIsInvalid = false;
		}
	});

	$: canSave = $syncStatus.status !== SyncEvents.SyncStarted && !ValidationUtil.isEmptyOrWhitespace(name) && !(!$isOnline && synced);

	async function save() {
		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = AccountsService.validate(name, investmentFund, stockPrice);
		if (result.valid) {
			nameIsInvalid = false;
			stockPriceIsInvalid = false;

			if (isNew) {
				try {
					const account = new Account(0, name, currency, false, createdDate, modifiedDate);
					account.stockPrice = investmentFund ? stockPrice : null;

					const newId = await accountsService.create(account);

					goto('/accounts?edited=' + newId);
				} catch {
					saveButtonIsLoading = false;
				}
			} else {
				try {
					const account = new Account(data.id, name, currency, isMain, createdDate, modifiedDate);
					account.stockPrice = investmentFund ? stockPrice : null;

					await accountsService.update(account);

					goto('/accounts?edited=' + data.id);
				} catch {
					saveButtonIsLoading = false;
				}
			}
		} else {
			nameIsInvalid = result.erroredFields.includes('name');
			stockPriceIsInvalid = result.erroredFields.includes('stockPrice');

			saveButtonIsLoading = false;
		}
	}

	async function deleteAccount() {
		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await accountsService.delete(data.id);

				alertState.update((x) => {
					x.showSuccess('editAccount.deleteSuccessful');
					return x;
				});
				goto('/accounts');
			} catch {
				deleteButtonText = $t('delete');
				deleteInProgress = false;
				deleteButtonIsLoading = false;
			}
		} else {
			if (await accountsService.hasTransactions(data.id)) {
				transactionsWarningVisible = true;
				deleteButtonText = $t('editAccount.okayDelete');
			} else {
				deleteButtonText = $t('sure');
			}

			deleteInProgress = true;
		}
	}

	function cancel() {
		if (!deleteInProgress) {
			goto('/accounts');
		}
		deleteButtonText = $t('delete');
		deleteInProgress = false;
		transactionsWarningVisible = false;
	}

	onMount(async () => {
		accountHasTransactionsHtml = $t('editAccount.accountHasTransactions');
		deleteButtonText = $t('delete');

		accountsService = new AccountsService();

		if (isNew) {
			currency = localStorage.get(LocalStorageKeys.Currency);
			investmentFund = false;
			saveButtonText = $t('create');

			nameInput.focus();
			investmentFundCheckboxDisabled = false;
		} else {
			saveButtonText = $t('save');

			const mainId = await accountsService.getMainId();
			isMainAccount = data.id === mainId;

			const account = await accountsService.get(data.id);
			if (account === null) {
				throw new Error('Account not found');
			}

			name = account.name;
			currency = account.currency;
			isMain = account.isMain;
			stockPrice = account.stockPrice;
			createdDate = account.createdDate;
			modifiedDate = account.modifiedDate;
			synced = account.synced;
			investmentFund = !!account.stockPrice;

			const balance = await accountsService.getBalance(data.id, account.currency);
			investmentFundCheckboxDisabled = balance !== 0;
		}
	});

	onDestroy(() => {
		alertUnsubscriber();
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			{#if isNew}
				<span>{$t('editAccount.newAccount')}</span>
			{:else}
				<span>{$t('editAccount.edit')}</span>&nbsp;<span class="colored-text">{name}</span>
			{/if}
		</div>
		<a href="/accounts" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !$isOnline && synced}
			<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
		{/if}

		{#if isMainAccount}
			<AlertBlock type="info" message={$t('editAccount.mainAccount')} />
		{/if}

		<form on:submit|preventDefault={save}>
			<div class="form-control">
				<input
					type="text"
					bind:this={nameInput}
					bind:value={name}
					maxlength="30"
					class:invalid={nameIsInvalid}
					placeholder={$t('editAccount.accountName')}
					aria-label={$t('editAccount.accountName')}
					required
				/>
			</div>

			{#if !isMainAccount}
				<div>
					<div class="form-control">
						<Checkbox labelKey="editAccount.investmentFund" bind:value={investmentFund} disabled={investmentFundCheckboxDisabled} />
					</div>

					{#if investmentFund}
						<div class="form-control inline">
							<label for="stock-price">{$t('editAccount.stockPrice')}</label>
							<StockPriceInput bind:stockPrice bind:currency bind:invalid={stockPriceIsInvalid} />
						</div>
					{/if}
				</div>
			{/if}

			<hr />

			{#if deleteInProgress && transactionsWarningVisible}
				<AlertBlock type="danger" message={accountHasTransactionsHtml} />
			{/if}

			<div class="save-delete-wrap">
				{#if !deleteInProgress}
					<button class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if !isNew && !isMainAccount}
					<button
						type="button"
						on:click={deleteAccount}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={deleteInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{deleteButtonText}</span>
					</button>
				{/if}

				{#if isNew || deleteInProgress || isMainAccount}
					<button type="button" on:click={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		</form>
	</div>
</section>
