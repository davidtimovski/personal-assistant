<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { alertState, isOnline } from '$lib/stores';
	import { AutomaticTransactionsService } from '$lib/services/automaticTransactionsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { CategoryType } from '$lib/models/entities/category';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { AutomaticTransaction } from '$lib/models/entities/automaticTransaction';

	import AlertBlock from '$lib/components/AlertBlock.svelte';
	import AmountInput from '$lib/components/AmountInput.svelte';
	import DoubleRadio from '$lib/components/DoubleRadio.svelte';

	export let data: PageData;

	const isNew = data.id === 0;

	let isDeposit: boolean;
	let categoryId: number | null = null;
	let amount: number | null = null;
	let currency: string | null = null;
	let description: string | null = null;
	let dayInMonth: number;
	let createdDate: Date | null;
	let synced: boolean;
	let categoryOptions: SelectOption[] | null = null;
	let dayInMonthOptions = new Array<SelectOption>();
	let amountIsInvalid = false;
	let saveButtonText: string;
	let deleteInProgress = false;
	let deleteButtonText: string;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;

	let localStorage: LocalStorageUtil;
	let automaticTransactionsService: AutomaticTransactionsService;
	let categoriesService: CategoriesService;

	let amountFrom = 0.01;
	let amountTo = 8000001;

	function validate(): ValidationResult {
		const result = new ValidationResult(true);

		if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
			result.fail('amount');
		}

		return result;
	}

	function isDepositChanged() {
		const categoryType = isDeposit ? CategoryType.DepositOnly : CategoryType.ExpenseOnly;

		categoriesService.getAllAsOptions($t('uncategorized'), categoryType).then((options) => {
			categoryOptions = options;
		});
	}

	$: canSave = () => {
		return !!amount && !(!$isOnline && synced);
	};

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

			if (isNew) {
				try {
					const automaticTransaction = new AutomaticTransaction(
						0,
						isDeposit,
						categoryId,
						amount,
						currency,
						description,
						dayInMonth,
						null,
						null
					);

					const newId = await automaticTransactionsService.create(automaticTransaction);

					goto('/automaticTransactions?edited=' + newId);
				} catch {
					saveButtonIsLoading = false;
				}
			} else {
				try {
					const automaticTransaction = new AutomaticTransaction(
						data.id,
						isDeposit,
						categoryId,
						amount,
						currency,
						description,
						dayInMonth,
						createdDate,
						null
					);

					await automaticTransactionsService.update(automaticTransaction);

					goto('/automaticTransactions?edited=' + data.id);
				} catch {
					saveButtonIsLoading = false;
				}
			}
		} else {
			saveButtonIsLoading = false;
		}
	}

	async function deleteAutomaticTransaction() {
		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await automaticTransactionsService.delete(data.id);

				alertState.update((x) => {
					x.showSuccess('editAutomaticTransaction.deleteSuccessful');
					return x;
				});
				goto('/automaticTransactions');
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
			goto('/automaticTransactions');
		}
		deleteButtonText = $t('delete');
		deleteInProgress = false;
	}

	onMount(async () => {
		deleteButtonText = $t('delete');

		for (let i = 1; i < 29; i++) {
			dayInMonthOptions.push(new SelectOption(i, $t(`dayOrdinal${i}`)));
		}

		alertState.subscribe((value) => {
			if (value.hidden) {
				amountIsInvalid = false;
			}
		});

		localStorage = new LocalStorageUtil();
		automaticTransactionsService = new AutomaticTransactionsService();
		categoriesService = new CategoriesService();

		if (isNew) {
			currency = localStorage.get(LocalStorageKeys.Currency);
			dayInMonth = 1;
			synced = false;

			saveButtonText = $t('create');
		} else {
			saveButtonText = $t('save');

			const automaticTransaction = await automaticTransactionsService.get(data.id);
			if (automaticTransaction === null) {
				throw new Error('Automatic transaction not found');
			}

			isDeposit = automaticTransaction.isDeposit;
			categoryId = automaticTransaction.categoryId;
			amount = automaticTransaction.amount;
			currency = automaticTransaction.currency;
			description = automaticTransaction.description;
			dayInMonth = automaticTransaction.dayInMonth;
			createdDate = automaticTransaction.createdDate;
			synced = automaticTransaction.synced;
		}

		if (currency === 'MKD') {
			amountFrom = 1;
			amountTo = 450000000;
		}

		isDepositChanged();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			<span
				>{$t(
					isNew
						? 'editAutomaticTransaction.newAutomaticTransaction'
						: 'editAutomaticTransaction.editAutomaticTransaction'
				)}</span
			>
		</div>
		<a href="/automaticTransactions" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !$isOnline && synced}
			<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
		{/if}

		<form on:submit|preventDefault={save}>
			<div class="form-control">
				<DoubleRadio
					name="depositExpenseToggle"
					leftLabelKey="editAutomaticTransaction.expense"
					rightLabelKey="editAutomaticTransaction.deposit"
					bind:value={isDeposit}
					on:change={isDepositChanged}
				/>
			</div>

			<div class="form-control inline">
				<label for="amount">{$t('amount')}</label>
				<AmountInput bind:amount bind:currency invalid={amountIsInvalid} />
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
				<label for="day-in-month">{$t('editAutomaticTransaction.onEvery')}</label>
				<select id="day-in-month" bind:value={dayInMonth} class="category-select">
					{#each dayInMonthOptions as day}
						<option value={day.id}>{day.name}</option>
					{/each}
				</select>
			</div>

			<div class="form-control">
				<textarea
					bind:value={description}
					maxlength="250"
					class="description-textarea"
					placeholder={$t('description')}
					aria-label={$t('description')}
				/>
			</div>

			<hr />

			<div class="save-delete-wrap">
				{#if !deleteInProgress}
					<button class="button primary-button" disabled={!canSave() || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if !isNew}
					<button
						type="button"
						on:click={deleteAutomaticTransaction}
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

				{#if isNew || deleteInProgress}
					<button type="button" on:click={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		</form>
	</div>
</section>
