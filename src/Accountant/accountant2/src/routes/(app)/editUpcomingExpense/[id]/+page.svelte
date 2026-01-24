<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { DateHelper } from '../../../../../../../Core/shared2/utils/dateHelper';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { alertState, isOnline, syncStatus } from '$lib/stores';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { UpcomingExpensesService } from '$lib/services/upcomingExpensesService';
	import { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
	import { CategoryType } from '$lib/models/entities/category';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { SyncEvents } from '$lib/models/syncStatus';

	import AmountInput from '$lib/components/AmountInput.svelte';
	import MonthSelector from '$lib/components/MonthSelector.svelte';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	const isNew = data.id === 0;

	let categoryId: number | null = $state(null);
	let amount: number | null = $state(null);
	let currency: string | null = $state(null);
	let description: string | null = $state(null);
	let generated = $state(false);
	let createdDate: Date | null = $state(null);
	let synced = $state(false);
	let month: number | null = $state(null);
	let year: number | null = $state(null);
	let categoryOptions: SelectOption[] | null = $state(null);
	let amountIsInvalid = $state(false);
	let saveButtonText = $state('');
	let deleteInProgress = $state(false);
	let deleteButtonText = $state('');
	let saveButtonIsLoading = $state(false);
	let deleteButtonIsLoading = $state(false);

	const localStorage = new LocalStorageUtil();
	let upcomingExpensesService: UpcomingExpensesService;
	let categoriesService: CategoriesService;

	let amountTo = 8000000;

	const alertUnsubscriber = alertState.subscribe((value) => {
		if (value.hidden) {
			amountIsInvalid = false;
		}
	});

	let canSave = $derived($syncStatus.status !== SyncEvents.SyncStarted && !!amount && !(!$isOnline && synced));

	async function save(event: Event) {
		event.preventDefault();

		if (!amount || !currency || year === null || month === null) {
			throw new Error('Unexpected error: required fields missing');
		}

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = UpcomingExpensesService.validate(amount, amountTo);
		if (result.valid) {
			amountIsInvalid = false;

			if (isNew) {
				try {
					const upcomingExpense = new UpcomingExpense(
						0,
						categoryId,
						amount,
						currency,
						description,
						DateHelper.format(new Date(year, month, 1)),
						false,
						null,
						null
					);
					const newId = await upcomingExpensesService.create(upcomingExpense);

					goto('/upcomingExpenses?edited=' + newId);
				} catch {
					saveButtonIsLoading = false;
				}
			} else {
				try {
					const upcomingExpense = new UpcomingExpense(
						data.id,
						categoryId,
						amount,
						currency,
						description,
						DateHelper.format(new Date(year, month, 1)),
						generated,
						createdDate,
						null
					);
					await upcomingExpensesService.update(upcomingExpense);

					goto('/upcomingExpenses?edited=' + data.id);
				} catch {
					saveButtonIsLoading = false;
				}
			}
		} else {
			amountIsInvalid = true;
			saveButtonIsLoading = false;
		}
	}

	async function deleteUpcomingExpense() {
		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await upcomingExpensesService.delete(data.id);

				alertState.update((x) => {
					x.showSuccess('editUpcomingExpense.deleteSuccessful');
					return x;
				});
				goto('/upcomingExpenses');
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
			goto('/upcomingExpenses');
		}
		deleteButtonText = $t('delete');
		deleteInProgress = false;
	}

	onMount(() => {
		deleteButtonText = $t('delete');

		upcomingExpensesService = new UpcomingExpensesService();
		categoriesService = new CategoriesService();

		if (isNew) {
			currency = localStorage.get(LocalStorageKeys.Currency);
			synced = false;

			saveButtonText = $t('create');
		} else {
			saveButtonText = $t('save');

			upcomingExpensesService.get(data.id).then((upcomingExpense: UpcomingExpense) => {
				if (upcomingExpense === null) {
					throw new Error('Upcoming expense not found');
				}

				categoryId = upcomingExpense.categoryId;
				amount = upcomingExpense.amount;
				currency = upcomingExpense.currency;
				if (currency === 'MKD') {
					amountTo = 450000000;
				}
				description = upcomingExpense.description;
				month = parseInt(upcomingExpense.date.slice(5, 7), 10) - 1;
				year = parseInt(upcomingExpense.date.slice(0, 4), 10);
				generated = upcomingExpense.generated;
				createdDate = upcomingExpense.createdDate;
				synced = upcomingExpense.synced;
			});
		}

		if (currency === 'MKD') {
			amountTo = 450000000;
		}

		categoriesService.getAllAsOptions($t('uncategorized'), CategoryType.ExpenseOnly).then((options) => {
			categoryOptions = options;
		});
	});

	onDestroy(() => {
		alertUnsubscriber();
		upcomingExpensesService?.release();
		categoriesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt"></i>
		</div>
		<div class="page-title">
			<span>{$t(isNew ? 'editUpcomingExpense.newUpcomingExpense' : 'editUpcomingExpense.editUpcomingExpense')}</span>
		</div>
		<a href="/upcomingExpenses" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		{#if !$isOnline && synced}
			<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
		{/if}

		{#if generated}
			<AlertBlock type="info" message={$t('editUpcomingExpense.generatedAlert')} />
		{/if}

		<form onsubmit={save}>
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
					<i class="fas fa-circle-notch fa-spin"></i>
				</div>
			</div>

			<div class="form-control inline">
				<label for="month-selector">{$t('editUpcomingExpense.month')}</label>
				<MonthSelector bind:month bind:year disabled={generated} />
			</div>

			<div class="form-control">
				<textarea bind:value={description} maxlength="250" class="description-textarea" placeholder={$t('description')} aria-label={$t('description')}
				></textarea>
			</div>

			<hr />

			<div class="save-delete-wrap">
				{#if !deleteInProgress}
					<button class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if !isNew}
					<button
						type="button"
						onclick={deleteUpcomingExpense}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={deleteInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin"></i>
						</span>
						<span>{deleteButtonText}</span>
					</button>
				{/if}

				{#if isNew || deleteInProgress}
					<button type="button" onclick={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		</form>
	</div>
</section>
