<script lang="ts">
	import type { IngredientCategory } from '$lib/models/viewmodels/ingredientSuggestions';

	import PublicCategorySuggestion from '$lib/components/PublicCategorySuggestion.svelte';
	import PublicIngredientSuggestion from '$lib/components/PublicIngredientSuggestion.svelte';

	let { category }: { category: IngredientCategory } = $props();
</script>

{#if category.matched}
	<div>
		<div class="category-name">{category.name}</div>

		<div class="category-body">
			{#each category.ingredients as ingredient}
				<div>
					<PublicIngredientSuggestion {ingredient} />
				</div>
			{/each}

			{#each category.subcategories as subcategory}
				<div class="subcategory-suggestion">
					<PublicCategorySuggestion category={subcategory} />
				</div>
			{/each}
		</div>
	</div>
{/if}

<style lang="scss">
	.category-name {
		background: #eeffee;
		padding: 0 15px;
		color: var(--primary-color-dark);
	}

	.category-body {
		padding: 0 15px;
	}
</style>
