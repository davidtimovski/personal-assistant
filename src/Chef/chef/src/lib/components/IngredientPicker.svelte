<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';

	import { ValidationUtil } from '../../../../../Core/shared2/utils/validationUtils';

	import { t } from '$lib/localization/i18n';
	import { ingredientPickerState } from '$lib/stores';
	import { IngredientsService } from '$lib/services/ingredientsService';
	import {
		IngredientCategory,
		IngredientSuggestion,
		IngredientSuggestions
	} from '$lib/models/viewmodels/ingredientSuggestions';
	import { IngredientPickEvent } from '$lib/models/ingredientPickerState';

	import PublicCategorySuggestion from '$lib/components/PublicCategorySuggestion.svelte';
	import PublicIngredientSuggestion from '$lib/components/PublicIngredientSuggestion.svelte';

	export let inputPlaceholder: string;
	export let addingEnabled: boolean;
	export let userIngredientsAllowed = true;
	export let recipeIngredientIds: number[];

	let ingredientName = '';
	let ingredientNameIsInvalid: boolean;
	let suggestions: IngredientSuggestions;
	let suggestionsMatched = false;
	const unsubscriptions: Unsubscriber[] = [];

	let ingredientsService: IngredientsService;

	function addNewIngredient() {
		ingredientNameIsInvalid = ValidationUtil.isEmptyOrWhitespace(ingredientName);

		if (!ingredientNameIsInvalid) {
			ingredientPickerState.update((x) => {
				x.event = IngredientPickEvent.Added;
				x.data = ingredientName;
				return x;
			});

			ingredientName = '';
		}

		resetIngredientMatches();
	}

	function ingredientInputChanged() {
		resetIngredientMatches();

		const search = ingredientName.trim().toUpperCase();
		if (!search || search.length < 3) {
			return;
		}

		suggestions.userIngredients.forEach((x) => searchIngredient(x, search));
		suggestions.userIngredients = [...suggestions.userIngredients];

		suggestions.publicIngredients.uncategorized.forEach((x) => searchIngredient(x, search));
		suggestions.publicIngredients.uncategorized = [...suggestions.publicIngredients.uncategorized];

		suggestions.publicIngredients.categories.forEach((x) => searchCategory(x, search));
		suggestions.publicIngredients.categories = [...suggestions.publicIngredients.categories];
	}

	function resetIngredientMatches() {
		suggestions.userIngredients.forEach((x) => matchIngredient(x, false));
		suggestions.userIngredients = [...suggestions.userIngredients];

		suggestions.publicIngredients.uncategorized.forEach((x) => matchIngredient(x, false));
		suggestions.publicIngredients.uncategorized = [...suggestions.publicIngredients.uncategorized];

		suggestions.publicIngredients.categories.forEach((x) => matchCategory(x, false));
		suggestions.publicIngredients.categories = [...suggestions.publicIngredients.categories];

		suggestionsMatched = false;
	}

	function matchIngredient(ingredient: IngredientSuggestion, matched: boolean) {
		ingredient.matched = matched;
		ingredient.children.forEach((x) => matchIngredient(x, matched));
	}

	function searchIngredient(ingredient: IngredientSuggestion, search: string) {
		ingredient.matched = ingredient.name.toUpperCase().includes(search);

		if (ingredient.matched) {
			matchIngredient(ingredient, true);
			suggestionsMatched = true;
		} else {
			for (const child of ingredient.children) {
				if (searchIngredient(child, search)) {
					ingredient.matched = true;
				}
			}
		}

		return ingredient.matched;
	}

	function matchCategory(category: IngredientCategory, matched: boolean) {
		category.matched = matched;
		category.ingredients.forEach((x) => matchIngredient(x, matched));
		category.subcategories.forEach((x) => matchCategory(x, matched));
	}

	function searchCategory(category: IngredientCategory, search: string) {
		category.matched = category.name.toUpperCase().includes(search);

		if (category.matched) {
			matchCategory(category, true);
			suggestionsMatched = true;
		} else {
			for (const ingredient of category.ingredients) {
				if (searchIngredient(ingredient, search)) {
					category.matched = true;
				}
			}

			for (const subcategory of category.subcategories) {
				if (searchCategory(subcategory, search)) {
					category.matched = true;
				}
			}
		}

		return category.matched;
	}

	function findInIngredientRecursive(
		ingredient: IngredientSuggestion,
		ingredientId: number
	): IngredientSuggestion | null {
		if (ingredient.id === ingredientId) {
			return ingredient;
		}

		let result = null;
		for (let i = 0; result == null && i < ingredient.children.length; i++) {
			result = findInIngredientRecursive(ingredient.children[i], ingredientId);
		}
		return result;
	}

	function findSuggestion(ingredientId: number) {
		let foundSuggestion: IngredientSuggestion | null | undefined = suggestions.userIngredients.find(
			(x) => x.id === ingredientId
		);
		if (foundSuggestion) {
			return foundSuggestion;
		}

		for (const suggestion of suggestions.publicIngredients.uncategorized) {
			foundSuggestion = findInIngredientRecursive(suggestion, ingredientId);
			if (foundSuggestion) {
				return foundSuggestion;
			}
		}

		for (const category of suggestions.publicIngredients.categories) {
			for (const suggestion of category.ingredients) {
				foundSuggestion = findInIngredientRecursive(suggestion, ingredientId);
				if (foundSuggestion) {
					return foundSuggestion;
				}
			}

			for (const subcategory of category.subcategories) {
				for (const suggestion of subcategory.ingredients) {
					foundSuggestion = findInIngredientRecursive(suggestion, ingredientId);
					if (foundSuggestion) {
						return foundSuggestion;
					}
				}
			}
		}

		throw new Error('Could not find suggestion');
	}

	onMount(() => {
		unsubscriptions.push(
			ingredientPickerState.subscribe((value) => {
				if (value.event !== IngredientPickEvent.Selected) {
					return;
				}

				ingredientName = '';
				ingredientNameIsInvalid = false;
				resetIngredientMatches();
			})
		);

		unsubscriptions.push(
			ingredientPickerState.subscribe((value) => {
				if (value.event !== IngredientPickEvent.Unselected) {
					return;
				}

				const ingredientId = <number>value.data;
				const suggestion = findSuggestion(ingredientId);
				suggestion.selected = false;

				suggestions.userIngredients = [...suggestions.userIngredients];

				suggestions.publicIngredients.uncategorized = [...suggestions.publicIngredients.uncategorized];
				suggestions.publicIngredients.categories = [...suggestions.publicIngredients.categories];
			})
		);

		unsubscriptions.push(
			ingredientPickerState.subscribe((value) => {
				if (value.event !== IngredientPickEvent.Reset) {
					return;
				}

				resetIngredientMatches();
			})
		);

		ingredientsService = new IngredientsService();

		let userSuggestionsPromise: Promise<IngredientSuggestion[]>;
		if (userIngredientsAllowed) {
			userSuggestionsPromise = ingredientsService.getUserIngredientSuggestions();
		} else {
			userSuggestionsPromise = new Promise((resolve) => {
				resolve([]);
			});
		}

		const publicSuggestionsPromise = ingredientsService.getPublicIngredientSuggestions();

		Promise.all([userSuggestionsPromise, publicSuggestionsPromise]).then((result) => {
			suggestions = new IngredientSuggestions(result[0], result[1]);

			if (recipeIngredientIds) {
				for (const id of recipeIngredientIds) {
					const suggestion = findSuggestion(id);
					suggestion.selected = true;
				}
			}
		});
	});

	onDestroy(() => {
		ingredientsService?.release();
	});
</script>

<div class="ingredient-picker">
	<div class="input-wrap">
		<input
			type="text"
			bind:value={ingredientName}
			on:keyup={ingredientInputChanged}
			disabled={!suggestions}
			class:invalid={ingredientNameIsInvalid}
			placeholder={inputPlaceholder}
			aria-label={inputPlaceholder}
			maxlength="50"
		/>

		{#if addingEnabled}
			<button type="button" on:click={addNewIngredient} title={$t('addIngredient')} aria-label={$t('addIngredient')}>
				<i class="fas fa-plus-circle" />
			</button>
		{/if}
	</div>

	{#if suggestionsMatched}
		<div>
			{#if userIngredientsAllowed}
				<div class="suggestions">
					<div class="suggestions-header">{$t('mine')}</div>

					<div class="suggestions-body">
						{#each suggestions.userIngredients as ingredient}
							<PublicIngredientSuggestion {ingredient} />
						{/each}
					</div>
				</div>
			{/if}

			<div class="suggestions">
				<div class="suggestions-header">{$t('public')}</div>

				<div class="ingredients">
					{#each suggestions.publicIngredients.uncategorized as ingredient}
						<PublicIngredientSuggestion {ingredient} />
					{/each}
				</div>

				{#each suggestions.publicIngredients.categories as category}
					<div class="category-suggestion">
						<PublicCategorySuggestion {category} />
					</div>
				{/each}
			</div>
		</div>
	{/if}
</div>

<style lang="scss">
	.ingredient-picker {
		.input-wrap {
			position: relative;
			width: 100%;

			input {
				width: calc(100% - 62px);
				padding-right: 50px;
				line-height: 45px;
			}

			button {
				position: absolute;
				top: 0;
				right: 0;
				width: 45px;
				height: 47px;
				display: flex;
				justify-content: center;
				align-items: center;
				background: none;
				border: none;
				outline: none;
				font-size: 23px;
				line-height: 45px;
				color: var(--primary-color);
				cursor: pointer;

				&:hover {
					color: var(--primary-color-dark);
				}
			}
		}

		.suggestions {
			border: 1px solid #ddd;
			border-radius: 6px;
			margin-top: 10px;

			&-header {
				background: var(--primary-color);
				text-align: center;
				line-height: 37px;
				color: #fff;
			}
		}
	}

	.category-suggestion {
		.category-name {
			background: #eeffee;
			padding: 0 15px;
			color: var(--primary-color-dark);
		}

		.category-body {
			padding: 0 15px;
		}
	}
</style>
