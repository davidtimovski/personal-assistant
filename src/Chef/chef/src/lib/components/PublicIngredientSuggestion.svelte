<script lang="ts">
	import { t } from '$lib/localization/i18n';
	import { ingredientPickerState } from '$lib/stores';
	import type { IngredientSuggestion } from '$lib/models/viewmodels/ingredientSuggestions';
	import { IngredientPickEvent } from '$lib/models/ingredientPickerState';

	import PublicIngredientSuggestion from '$lib/components/PublicIngredientSuggestion.svelte';

	export let ingredient: IngredientSuggestion;

	function ingredientClicked() {
		if (ingredient.selected) {
			return;
		}

		ingredientPickerState.update((x) => {
			x.event = IngredientPickEvent.Selected;
			x.data = ingredient;
			return x;
		});

		ingredient.selected = true;
	}
</script>

{#if ingredient.matched}
	<button
		type="button"
		on:click={ingredientClicked}
		disabled={ingredient.selected}
		class="ingredient-suggestion"
		class:selected={ingredient.selected}
	>
		{#if ingredient.brandName}
			<span>{ingredient.name} ({ingredient.brandName})</span>
		{:else}
			<span>{ingredient.name}</span>
		{/if}

		<span class="icons-container">
			{#if ingredient.hasNutritionData}
				<i class="fas fa-clipboard" title={$t('hasNutrition')} aria-label={$t('hasNutrition')} />
			{/if}
			{#if ingredient.hasPriceData}
				<i class="fas fa-tag" title={$t('hasPrice')} aria-label={$t('hasPrice')} />
			{/if}
			<i class="fas fa-check" />
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

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .ingredient-suggestion:hover {
		color: var(--primary-color);
	}
</style>
