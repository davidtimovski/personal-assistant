<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import AlertBlock from '../../../../../../../Core/shared2/components/AlertBlock.svelte';
	import Checkbox from '../../../../../../../Core/shared2/components/Checkbox.svelte';
	import Tooltip from '../../../../../../../Core/shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { alertState, isOnline, syncStatus } from '$lib/stores';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { Category, CategoryType } from '$lib/models/entities/category';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { SyncEvents } from '$lib/models/syncStatus';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	const isNew = data.id === 0;

	let parentId: number | null = $state(null);
	let name = $state('');
	let type: CategoryType | null = $state(null);
	let generateUpcomingExpense = $state(false);
	let isTax = $state(false);
	let createdDate: Date | null;
	let modifiedDate: Date | null;
	let synced = $state(false);
	let isParent = $state(false);
	let parentCategoryOptions: SelectOption[] | null = $state(null);
	let typeOptions: SelectOption[] = $state([]);
	let nameInput: HTMLInputElement;
	let nameIsInvalid = $state(false);
	let saveButtonText = $state('');
	let transactionsWarningVisible = $state(false);
	let deleteInProgress = $state(false);
	let deleteButtonText = $state('');
	let saveButtonIsLoading = $state(false);
	let deleteButtonIsLoading = $state(false);
	let categoryHasTransactionsHtml = $state('');

	let categoriesService: CategoriesService;

	const alertUnsubscriber = alertState.subscribe((value) => {
		if (value.hidden) {
			nameIsInvalid = false;
		}
	});

	let canSave = $derived($syncStatus.status !== SyncEvents.SyncStarted && !ValidationUtil.isEmptyOrWhitespace(name) && !(!$isOnline && synced));

	function typeChanged() {
		if (type !== CategoryType.ExpenseOnly) {
			generateUpcomingExpense = false;
			isTax = false;
		}
	}

	async function save(event: Event) {
		event.preventDefault();

		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = CategoriesService.validate(name);

		if (result.valid) {
			nameIsInvalid = false;

			if (isNew) {
				try {
					const category = new Category(0, parentId, name, <CategoryType>type, generateUpcomingExpense, isTax, createdDate, modifiedDate);
					const newId = await categoriesService.create(category);

					goto('/categories?edited=' + newId);
				} catch {
					saveButtonIsLoading = false;
				}
			} else {
				try {
					const category = new Category(data.id, parentId, name, <CategoryType>type, generateUpcomingExpense, isTax, createdDate, modifiedDate);

					await categoriesService.update(category);

					goto('/categories?edited=' + data.id);
				} catch {
					saveButtonIsLoading = false;
				}
			}
		} else {
			nameIsInvalid = true;
			saveButtonIsLoading = false;
		}
	}

	async function deleteCategory() {
		if (deleteInProgress) {
			deleteButtonIsLoading = true;

			try {
				await categoriesService.delete(data.id);
				alertState.update((x) => {
					x.showSuccess('editCategory.deleteSuccessful');
					return x;
				});
				goto('/categories');
			} catch {
				deleteButtonText = $t('delete');
				deleteInProgress = false;
				deleteButtonIsLoading = false;
				return;
			}
		} else {
			if (await categoriesService.hasTransactions(data.id)) {
				transactionsWarningVisible = true;
				deleteButtonText = $t('editCategory.okayDelete');
			} else {
				deleteButtonText = $t('sure');
			}

			deleteInProgress = true;
		}
	}

	function cancel() {
		if (!deleteInProgress) {
			goto('/categories');
		}
		deleteButtonText = $t('delete');
		deleteInProgress = false;
		transactionsWarningVisible = false;
	}

	onMount(async () => {
		categoryHasTransactionsHtml = $t('editCategory.categoryHasTransactions');
		deleteButtonText = $t('delete');
		typeOptions.push(
			...[
				new SelectOption(CategoryType.AllTransactions, $t('editCategory.allTransactions')),
				new SelectOption(CategoryType.DepositOnly, $t('editCategory.depositOnly')),
				new SelectOption(CategoryType.ExpenseOnly, $t('editCategory.expenseOnly'))
			]
		);

		categoriesService = new CategoriesService();

		if (isNew) {
			type = CategoryType.AllTransactions;
			synced = false;
			saveButtonText = $t('create');

			nameInput.focus();
		} else {
			saveButtonText = $t('save');

			categoriesService.isParent(data.id).then((value: boolean) => {
				isParent = value;
			});

			categoriesService.get(data.id).then((category: Category) => {
				if (category === null) {
					throw new Error('Category not found');
				}

				parentId = category.parentId;
				name = category.name;
				type = category.type;
				generateUpcomingExpense = category.generateUpcomingExpense;
				isTax = category.isTax;
				createdDate = category.createdDate;
				modifiedDate = category.modifiedDate;
				synced = category.synced;
			});
		}

		parentCategoryOptions = await categoriesService.getParentAsOptions($t('editCategory.none'), data.id);
	});

	onDestroy(() => {
		alertUnsubscriber();
		categoriesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt"></i>
		</div>
		<div class="page-title">
			{#if isNew}
				<span>{$t('editCategory.newCategory')}</span>
			{:else}
				<span>{$t('editCategory.edit')}</span>&nbsp;<span class="colored-text">{name}</span>
			{/if}
		</div>
		<a href="/categories" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		{#if !$isOnline && synced}
			<AlertBlock type="warning" message={$t('whileOfflineCannotModify')} />
		{/if}

		{#if isParent}
			<AlertBlock type="info" message={$t('editCategory.thisIsAParentCategory')} />
		{/if}

		<form onsubmit={save}>
			<div class="form-control">
				<input
					type="text"
					bind:this={nameInput}
					bind:value={name}
					maxlength="30"
					class:invalid={nameIsInvalid}
					placeholder={$t('editCategory.categoryName')}
					aria-label={$t('editCategory.categoryName')}
					required
				/>
			</div>

			{#if !isParent}
				<div class="form-control inline">
					<label for="parent-category">{$t('editCategory.parentCategory')}</label>
					<div class="loadable-select" class:loaded={parentCategoryOptions}>
						<select id="parent-category" bind:value={parentId} disabled={!parentCategoryOptions} class="category-select">
							{#if parentCategoryOptions}
								{#each parentCategoryOptions as category}
									<option value={category.id}>{category.name}</option>
								{/each}
							{/if}
						</select>
						<i class="fas fa-circle-notch fa-spin"></i>
					</div>
				</div>
			{/if}

			<div class="form-control inline">
				<label for="type">{$t('editCategory.type')}</label>
				<select id="type" bind:value={type} onchange={typeChanged} class="category-select">
					{#if typeOptions}
						{#each typeOptions as type}
							<option value={type.id}>{type.name}</option>
						{/each}
					{/if}
				</select>
			</div>

			<div class="form-control">
				<Checkbox labelKey="editCategory.generateUpcomingExpense" bind:value={generateUpcomingExpense} disabled={type !== 2} />
			</div>

			<div class="form-control">
				<Checkbox labelKey="editCategory.tax" bind:value={isTax} disabled={type !== 2} />
				<Tooltip key="taxCategories" application="Accountant" />
			</div>

			<hr />

			{#if deleteInProgress && transactionsWarningVisible}
				<AlertBlock type="danger" message={categoryHasTransactionsHtml} />
			{/if}

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
						onclick={deleteCategory}
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
