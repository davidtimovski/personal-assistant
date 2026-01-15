<script lang="ts">
	import { t } from '$lib/localization/i18n';
	import { ingredientPickerState } from '$lib/stores';
	import type { IngredientSuggestion } from '$lib/models/viewmodels/ingredientSuggestions';
	import { IngredientPickEvent, IngredientPickerState } from '$lib/models/ingredientPickerState';

	import PublicIngredientSuggestion from '$lib/components/PublicIngredientSuggestion.svelte';

	let { ingredient }: { ingredient: IngredientSuggestion } = $props();

	function ingredientClicked() {
		if (ingredient.selected) {
			return;
		}

		$ingredientPickerState = new IngredientPickerState(IngredientPickEvent.Selected, ingredient);

		ingredient.selected = true;
	}
</script>

{#if ingredient.matched}
	<button type="button" onclick={ingredientClicked} disabled={ingredient.selected} class="ingredient-suggestion" class:selected={ingredient.selected}>
		{#if ingredient.brandName}
			<span>{ingredient.name} ({ingredient.brandName})</span>
		{:else}
			<span>{ingredient.name}</span>
		{/if}

		<span class="icons-container">
			{#if ingredient.hasNutritionData}
				<i class="fas fa-clipboard" title={$t('hasNutrition')} aria-label={$t('hasNutrition')}></i>
			{/if}
			{#if ingredient.hasPriceData}
				<i class="fas fa-tag" title={$t('hasPrice')} aria-label={$t('hasPrice')}></i>
			{/if}
			<i class="fas fa-check"></i>
		</span>
	</button>

	{#each ingredient.children as childIngredient}
		<div class="ingredient-suggestion-children">
			<PublicIngredientSuggestion ingredient={childIngredient} />
		</div>
	{/each}
{/if}

<style lang="scss">
	.ingredient-suggestion {
		display: block;
		width: 100%;
		background: transparent;
		border: none;
		outline: none;
		padding: 5px 15px;
		line-height: 27px;
		text-align: left;
		cursor: pointer;

		&.selected {
			color: var(--primary-color-dark);
			cursor: default;

			.icons-container .fa-check {
				display: inline;
			}
		}

		&:hover {
			color: var(--primary-color-dark);
		}

		.icons-container {
			margin-left: 5px;

			i {
				margin-left: 4px;
				font-size: 16px;
				color: var(--primary-color);

				&.fa-tag {
					font-size: 17px;
				}
			}

			.fa-check {
				display: none;
				font-size: 18px;
			}
		}
	}

	.ingredient-suggestion-children {
		padding-left: 15px;
	}
</style>
