<script lang="ts">
	import { t } from '$lib/localization/i18n';
	import type { ViewRecipe } from '$lib/models/viewmodels/viewRecipe';

	export let recipe: ViewRecipe;

	function decrementServings() {
		if (recipe.servings > 1) {
			recipe.servings--;

			adjustIngredientAmountToServings();
			adjustCostToServings();
		}
	}

	function incrementServings() {
		if (recipe.servings < 50) {
			recipe.servings++;

			adjustIngredientAmountToServings();
			adjustCostToServings();
		}
	}

	function adjustIngredientAmountToServings() {
		for (let ingredient of recipe.ingredients.filter((x) => x.amount)) {
			ingredient.amount = parseFloat((ingredient.amountPerServing * recipe.servings).toFixed(2));
		}
	}

	function adjustCostToServings() {
		recipe.costSummary.cost = parseFloat((recipe.costSummary.costPerServing * recipe.servings).toFixed(2));
	}

	$: servingsLabel = recipe.servings > 1 ? $t('recipe.servings', { servings: recipe.servings }) : $t('recipe.oneServing');
</script>

<div class="servings-selector">
	<a on:click={decrementServings} class="servings-button" role="button" title={$t('decreaseServings')} aria-label={$t('decreaseServings')}>
		<i class="fas fa-minus" />
	</a>
	<span class="servings">{servingsLabel}</span>
	<a on:click={incrementServings} class="servings-button" role="button" title={$t('increaseServings')} aria-label={$t('increaseServings')}>
		<i class="fas fa-plus" />
	</a>
</div>
