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
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { alertState, isOnline } from '$lib/stores';
	import type { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { UpcomingExpensesService } from '$lib/services/upcomingExpensesService';
	import { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
	import { CategoryType } from '$lib/models/entities/category';
	import { CategoriesService } from '$lib/services/categoriesService';

	import AlertBlock from '$lib/components/AlertBlock.svelte';
	import AmountInput from '$lib/components/AmountInput.svelte';
	import MonthSelector from '$lib/components/MonthSelector.svelte';

	export let id: number;

	let categoryId: number | null = null;
	let amount: number | null = null;
	let currency: string;
	let description: string;
	let generated: boolean;
	let createdDate: Date | null;
	let synced: boolean;
	let month: number | null = null;
	let year: number | null = null;
	let categoryOptions: SelectOption[] | null = null;
	let isNew: boolean;
	let amountIsInvalid: boolean;
	let saveButtonText: string;
	let deleteInProgress = false;
	let deleteButtonText: string;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;
	let language: string;

	let localStorage: LocalStorageUtil;
	let upcomingExpensesService: UpcomingExpensesService;
	let categoriesService: CategoriesService;

	let amountTo = 8000000;

	$: canSave = () => {
		return !!amount && !(!$isOnline && synced);
	};

	function validate(): ValidationResult {
		if (!currency) {
			return new ValidationResult(false);
		}

		const result = new ValidationResult(true);

		if (!ValidationUtil.between(<number>amount, 0, amountTo)) {
			result.fail('amount');
		}

		return result;
	}

	async function save() {
		if (!canSave() || saveButtonIsLoading) {
			return;
		}

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = validate();
		if (result.valid) {
			if (isNew) {
				try {
					const upcomingExpense = new UpcomingExpense(
						0,
						<number>categoryId,
						<number>amount,
						currency,
						description,
						DateHelper.format(new Date(<number>year, <number>month, 1)),
						false,
						null,
						null
					);
					const id = await upcomingExpensesService.create(upcomingExpense);
					amountIsInvalid = false;

					goto('/upcomingExpenses?edited=' + id);
				} catch {
					saveButtonIsLoading = false;
				}
			} else {
				try {
					const upcomingExpense = new UpcomingExpense(
						id,
						<number>categoryId,
						<number>amount,
						currency,
						description,
						DateHelper.format(new Date(<number>year, <number>month, 1)),
						generated,
						createdDate,
						null
					);

					await upcomingExpensesService.update(upcomingExpense);
					amountIsInvalid = false;

					goto('/upcomingExpenses?edited=' + id);
				} catch {
					saveButtonIsLoading = false;
				}
			}
		} else {
			saveButtonIsLoading = false;
		}
	}

	async function deleteUpcomingExpense() {
		if (deleteButtonIsLoading) {
			return;
		}

		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await upcomingExpensesService.delete(id);

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

		alertState.subscribe((value) => {
			if (value.hidden) {
				amountIsInvalid = false;
			}
		});

		localStorage = new LocalStorageUtil();
		upcomingExpensesService = new UpcomingExpensesService();
		categoriesService = new CategoriesService();

		language = localStorage.get('language');

		isNew = id === 0;

		if (isNew) {
			currency = localStorage.get('currency');
			if (currency === 'MKD') {
				amountTo = 450000000;
			}
			synced = false;

			saveButtonText = $t('create');
		} else {
			saveButtonText = $t('save');

			upcomingExpensesService.get(id).then((upcomingExpense: UpcomingExpense) => {
				if (upcomingExpense === null) {
					// TODO
					goto('notFound');
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

		categoriesService.getAllAsOptions($t('uncategorized'), CategoryType.ExpenseOnly).then((options) => {
			categoryOptions = options;
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
					<span
						>{isNew
							? $t('editUpcomingExpense.newUpcomingExpense')
							: $t('editUpcomingExpense.editUpcomingExpense')}</span
					>
				</div>
				<a href="/upcomingExpenses" class="back-button">
					<i class="fas fa-times" />
				</a>
			</div>

			<div class="content-wrap">
				{#if !$isOnline && synced}
					<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
				{/if}

				{#if generated}
					<div class="message info">
						<i class="fas fa-info-circle" />
						<span>{$t('editUpcomingExpense.generatedAlert')}</span>
					</div>
				{/if}

				<form on:submit={save}>
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
						<label for="month-selector">{$t('editUpcomingExpense.month')}</label>
						<MonthSelector bind:month bind:year disabled={generated} {language} />
					</div>

					<div class="form-control">
						<textarea
							bind:value={description}
							maxlength="250"
							class="description-textarea"
							placeholder={$t('editUpcomingExpense.description')}
							aria-label={$t('editUpcomingExpense.description')}
						/>
					</div>

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
								<span>{saveButtonText}</span>
							</a>
						{/if}

						{#if !isNew}
							<a
								on:click={deleteUpcomingExpense}
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
						{/if}

						{#if isNew || deleteInProgress}
							<button type="button" on:click={cancel} class="button secondary-button">
								{$t('cancel')}
							</button>
						{/if}
					</div>
				</form>
			</div>
		</div>
	</div>
</section>
