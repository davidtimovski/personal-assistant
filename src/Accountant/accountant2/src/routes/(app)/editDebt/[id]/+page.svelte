<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';
	import DoubleRadioBool from '../../../../../../../Core/shared2/components/DoubleRadioBool.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { user, alertState, isOnline, syncStatus } from '$lib/stores';
	import { DebtsService } from '$lib/services/debtsService';
	import { DebtModel } from '$lib/models/entities/debt';
	import { SyncEvents } from '$lib/models/syncStatus';

	import AmountInput from '$lib/components/AmountInput.svelte';

	export let data: PageData;

	const isNew = data.id === 0;

	let person = '';
	let amount: number | null = null;
	let currency: string | null = null;
	let description: string | null;
	let userIsDebtor: boolean | null = null;
	let createdDate: Date | null;
	let synced: boolean;
	let personInput: HTMLInputElement;
	let mergeDebtPerPerson: boolean | null = null;
	let personIsInvalid = false;
	let amountIsInvalid = false;
	let saveButtonText: string;
	let deleteInProgress = false;
	let deleteButtonText: string;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;

	let localStorage: LocalStorageUtil;
	let debtsService: DebtsService;

	let amountTo = 8000000;

	const alertUnsubscriber = alertState.subscribe((value) => {
		if (value.hidden) {
			personIsInvalid = false;
			amountIsInvalid = false;
		}
	});

	$: canSave = $syncStatus.status !== SyncEvents.SyncStarted && !!amount && !(!$isOnline && synced);

	async function save() {
		if (!amount || !currency || userIsDebtor === null) {
			throw new Error('Unexpected error: required fields missing');
		}

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = DebtsService.validate(person, amount, amountTo);
		if (result.valid) {
			personIsInvalid = false;
			amountIsInvalid = false;

			if (isNew) {
				try {
					const debt = new DebtModel(0, person, amount, currency, description, userIsDebtor, null, null);
					const newId = await debtsService.createOrMerge(
						debt,
						<boolean>mergeDebtPerPerson,
						$user.culture,
						$t('editDebt.lended'),
						$t('editDebt.borrowed')
					);

					goto('/debt?edited=' + newId);
				} catch {
					saveButtonIsLoading = false;
				}
			} else {
				try {
					const debt = new DebtModel(data.id, person, amount, currency, description, userIsDebtor, createdDate, null);
					await debtsService.update(debt);

					goto('/debt?edited=' + data.id);
				} catch {
					saveButtonIsLoading = false;
				}
			}
		} else {
			personIsInvalid = result.erroredFields.includes('person');
			amountIsInvalid = result.erroredFields.includes('amount');

			saveButtonIsLoading = false;
		}
	}

	async function deleteDebt() {
		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await debtsService.delete(data.id);

				alertState.update((x) => {
					x.showSuccess('editDebt.deleteSuccessful');
					return x;
				});
				goto('/debt');
			} catch {
				deleteButtonText = $t('delete');
				deleteInProgress = false;
				deleteButtonIsLoading = false;
			}
		} else {
			deleteButtonText = $t('sure');
			deleteInProgress = true;
		}
	}

	function cancel() {
		if (!deleteInProgress) {
			goto('/debt');
		}
		deleteButtonText = $t('delete');
		deleteInProgress = false;
	}

	onMount(() => {
		deleteButtonText = $t('delete');

		localStorage = new LocalStorageUtil();
		debtsService = new DebtsService();

		mergeDebtPerPerson = localStorage.getBool('mergeDebtPerPerson');

		if (isNew) {
			userIsDebtor = false;

			currency = localStorage.get(LocalStorageKeys.Currency);
			synced = false;

			saveButtonText = $t('create');

			personInput.focus();
		} else {
			saveButtonText = $t('save');

			debtsService.get(data.id).then((debt: DebtModel) => {
				if (debt === null) {
					throw new Error('Debt not found');
				}

				person = debt.person;
				amount = debt.amount;
				currency = debt.currency;
				description = debt.description;
				userIsDebtor = debt.userIsDebtor;
				createdDate = debt.createdDate;
				synced = debt.synced;
			});
		}

		if (currency === 'MKD') {
			amountTo = 450000000;
		}
	});

	onDestroy(() => {
		alertUnsubscriber();
		debtsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			<span>{$t(isNew ? 'editDebt.newDebt' : 'editDebt.editDebt')}</span>
		</div>
		<a href="/debt" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !$isOnline && synced}
			<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
		{/if}

		<form on:submit|preventDefault={save}>
			<div class="form-control">
				<input
					type="text"
					bind:this={personInput}
					bind:value={person}
					maxlength="20"
					class:invalid={personIsInvalid}
					placeholder={$t('editDebt.person')}
					aria-label={$t('editDebt.person')}
					required
				/>
			</div>

			<div class="form-control inline">
				<label for="amount">{$t('amount')}</label>
				<AmountInput bind:amount bind:currency invalid={amountIsInvalid} />
			</div>

			<div class="form-control with-descriptor">
				<div class="setting-descriptor">{$t('editDebt.iAmThe')}</div>

				<DoubleRadioBool name="debtorLenderToggle" leftLabelKey="editDebt.lender" rightLabelKey="editDebt.debtor" bind:value={userIsDebtor} />
			</div>

			<div class="form-control">
				<textarea
					bind:value={description}
					maxlength="2000"
					class="description-textarea"
					placeholder={$t('description')}
					aria-label={$t('description')}
				/>
			</div>

			<hr />

			<div class="save-delete-wrap">
				{#if !deleteInProgress}
					<button class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if !isNew}
					<button type="button" on:click={deleteDebt} class="button danger-button" disabled={deleteButtonIsLoading} class:confirm={deleteInProgress}>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{deleteButtonText}</span>
					</button>
				{/if}

				{#if isNew || deleteInProgress}
					<button type="button" on:click={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		</form>
	</div>
</section>

<style lang="scss">
	.description-textarea {
		height: 110px;
	}
</style>
