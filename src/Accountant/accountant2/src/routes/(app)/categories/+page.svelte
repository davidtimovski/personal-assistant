<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import EmptyListMessage from '../../../../../../Core/shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { syncStatus } from '$lib/stores';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { CategoryItem } from '$lib/models/viewmodels/categoryItem';
	import { SyncEvents } from '$lib/models/syncStatus';
	import type { Category } from '$lib/models/entities/category';

	let categories: CategoryItem[] | null = $state(null);
	let editedId: number | undefined = $state(undefined);

	let categoriesService: CategoriesService;

	function mapToCategoryItem(category: Category, subCategories: Category[]): CategoryItem {
		return new CategoryItem(
			category.id,
			category.name,
			category.type,
			category.generateUpcomingExpense,
			category.synced,
			subCategories.map((c: Category) => {
				return mapToCategoryItem(c, []);
			})
		);
	}

	onMount(async () => {
		const url = new URL(window.location.href);
		const queryParams = new URLSearchParams(url.search);
		const edited = queryParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		categoriesService = new CategoriesService();

		const categoryDtos = await categoriesService.getAll();

		const categoryItems = new Array<CategoryItem>();
		for (const categoryDto of categoryDtos) {
			if (categoryDto.parentId !== null) {
				continue;
			}

			const subCategories = categoryDtos.filter((c) => c.parentId === categoryDto.id).sort((a, b) => (a.name > b.name ? 1 : -1));
			const item = mapToCategoryItem(categoryDto, subCategories);
			categoryItems.push(item);
		}

		categories = categoryItems;
	});

	onDestroy(() => {
		categoriesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-inbox"></i>
		</div>
		<div class="page-title">{$t('categories.categories')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !categories}
				<div class="double-circle-loading">
					<div class="double-bounce1"></div>
					<div class="double-bounce2"></div>
				</div>
			{:else if categories.length > 0}
				{#each categories as category}
					<div>
						<div class="category-wrap">
							<a class="category" class:highlighted-row={category.id === editedId} href="/editCategory/{category.id}">
								<span class="name">{category.name}</span>
								<span>
									{#if category.generateUpcomingExpense}
										<span
											class="category-indicator"
											title={$t('categories.generatingUpcomingExpenses')}
											aria-label={$t('categories.generatingUpcomingExpenses')}
										>
											<i class="far fa-calendar-alt"></i>
										</span>
									{/if}

									{#if category.type === 1}
										<span class="category-indicator" title={$t('categories.depositOnly')} aria-label={$t('categories.depositOnly')}>
											<i class="fas fa-donate"></i>
										</span>
									{/if}

									{#if category.type === 2}
										<span class="category-indicator" title={$t('categories.expenseOnly')} aria-label={$t('categories.expenseOnly')}>
											<i class="fas fa-wallet"></i>
										</span>
									{/if}
								</span>
							</a>

							<div class="sync">
								{#if !category.synced}
									<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')}></i>
								{/if}
							</div>
						</div>

						<!-- SUB -->
						{#each category.subCategories as sub}
							<div class="subcategory-wrap">
								<a class="category" class:highlighted-row={sub.id === editedId} href="editCategory/{sub.id}">
									<span class="name">{sub.name}</span>
									<span>
										{#if sub.generateUpcomingExpense}
											<span
												class="category-indicator"
												title={$t('categories.generatingUpcomingExpenses')}
												aria-label={$t('categories.generatingUpcomingExpenses')}
											>
												<i class="far fa-calendar-alt"></i>
											</span>
										{/if}

										{#if sub.type === 1}
											<span class="category-indicator" title={$t('categories.depositOnly')} aria-label={$t('categories.depositOnly')}>
												<i class="fas fa-donate"></i>
											</span>
										{/if}

										{#if sub.type === 2}
											<span class="category-indicator" title={$t('categories.expenseOnly')} aria-label={$t('categories.expenseOnly')}>
												<i class="fas fa-wallet"></i>
											</span>
										{/if}
									</span>
								</a>

								<div class="sync">
									{#if !sub.synced}
										<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')}></i>
									{/if}
								</div>
							</div>
						{/each}
					</div>
				{/each}
			{:else}
				<EmptyListMessage messageKey="categories.emptyListMessage" />
			{/if}
		</div>

		<div class="centering-wrap">
			<button
				type="button"
				onclick={() => goto('/editCategory/0')}
				class="new-button"
				disabled={$syncStatus.status === SyncEvents.SyncStarted}
				title={$t('categories.newCategory')}
				aria-label={$t('categories.newCategory')}
			>
				<i class="fas fa-plus"></i>
			</button>
		</div>
	</div>
</section>

<style lang="scss">
	.category-wrap {
		display: flex;
	}

	.subcategory-wrap {
		display: flex;
		padding-left: 30px;
	}

	.category {
		display: flex;
		justify-content: space-between;
		width: 100%;
		background: #f4f1f8;
		border-radius: var(--border-radius);
		margin-bottom: 10px;
		padding: 9px 15px;
		color: var(--regular-color);
		line-height: 27px;
		text-decoration: none;
		user-select: none;

		&:hover {
			color: var(--primary-color-dark);
		}

		.name {
			padding-right: 15px;
		}

		&:last-child {
			margin-bottom: 0;
		}
	}

	.category-indicator {
		margin-left: 15px;
		font-size: 1.2rem;
		color: var(--faded-color);
	}

	.sync {
		line-height: 45px;
		font-size: 1.2rem;
		color: var(--faded-color);

		i {
			margin-left: 15px;
		}
	}
</style>
