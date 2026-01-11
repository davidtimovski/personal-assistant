<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { page } from '$app/stores';
	import type { PageData } from './$types';

	import { t } from '$lib/localization/i18n';
	import { Formatter } from '$lib/utils/formatter';
	import { TimeFormatter } from '$lib/utils/timeFormatter';
	import { AmountFormatter } from '$lib/utils/amountFormatter';
	import { user } from '$lib/stores';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { SharingState } from '$lib/models/viewmodels/sharingState';
	import { RecipesService } from '$lib/services/recipesService';
	import type { ViewRecipe } from '$lib/models/viewmodels/viewRecipe';
	import type { Ingredient } from '$lib/models/viewmodels/ingredient';

	import IngredientAmount from '$lib/components/IngredientAmount.svelte';
	import ServingsSelector from '$lib/components/ServingsSelector.svelte';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	let editedId: number | undefined = $state(undefined);
	let topDrawerIsOpen = $state(false);
	let shareButtonText = $state('');
	let model: ViewRecipe | undefined = $state(undefined);
	let instructionsInHtml: string | undefined = $state(undefined);
	let videoIFrame: HTMLIFrameElement | null = $state(null);
	let videoIFrameSrc = $state('');
	let servingsSelectorIsVisible = $state(false);
	let currency: string | null = $state(null);
	let wakeLockSupported: boolean | undefined = $state(undefined);
	let wakeLock: any;
	let copyAsTextCompleted = $state(false);
	let recipeContainer: HTMLDivElement;
	let resizeObserver: ResizeObserver;

	let localStorage: LocalStorageUtil;
	let recipesService: RecipesService;

	function toggleTopDrawer(event: Event) {
		event.stopPropagation();

		topDrawerIsOpen = !topDrawerIsOpen;
	}

	function closeDrawer() {
		topDrawerIsOpen = false;
	}

	function copyAsText() {
		if (!model) {
			throw new Error('Recipe not loaded yet');
		}

		recipesService.copyAsText(
			model,
			$t('editRecipe.ingredients'),
			$t('editRecipe.instructions'),
			$t('editRecipe.youTubeUrl'),
			$t('editRecipe.prepDuration'),
			$t('minutesLetter'),
			$t('hoursLetter'),
			$t('editRecipe.cookDuration'),
			$t('recipe.servings', {
				servings: model.servings
			})
		);

		copyAsTextCompleted = true;
		window.setTimeout(() => {
			copyAsTextCompleted = false;
		}, 2000);
	}

	onMount(async () => {
		wakeLockSupported = 'wakeLock' in navigator;

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		recipesService = new RecipesService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const viewRecipe = await recipesService.get(data.id, currency);
		if (viewRecipe === null) {
			throw new Error('Recipe not found');
		} else {
			instructionsInHtml = viewRecipe.instructions ? viewRecipe.instructions.replace(/(?:\r\n|\r|\n)/g, '<br>') : '';

			servingsSelectorIsVisible = viewRecipe.ingredients.some((ingredient: Ingredient) => {
				return !!ingredient.amount;
			});

			model = viewRecipe;

			// Set image width and height to avoid reflows
			resizeObserver = new ResizeObserver(() => {
				if (!model) {
					throw new Error('Recipe not loaded yet');
				}

				model.imageWidth = recipeContainer.offsetWidth - 2;
				model.imageHeight = (recipeContainer.offsetWidth - 2) / 2;
			});
			resizeObserver.observe(document.body);

			if (model.videoUrl) {
				videoIFrameSrc = recipesService.videoUrlToEmbedSrc(model.videoUrl, $user.language);
			}

			shareButtonText = model.sharingState === SharingState.NotShared ? $t('recipe.shareRecipe') : $t('recipe.members');

			if (wakeLockSupported) {
				(<any>navigator).wakeLock.request('screen').then((wl: any) => {
					wakeLock = wl;
				});
			}
		}
	});

	onDestroy(() => {
		if (wakeLockSupported && wakeLock) {
			wakeLock.release();
			wakeLock = null;
		}
		resizeObserver?.disconnect();
		recipesService?.release();
	});
</script>

<section class="container" onclick={closeDrawer}>
	<div class="page-title-wrap">
		<a href="/editRecipe/{data.id}" class="edit-button" title={$t('recipe.edit')} aria-label={$t('recipe.edit')}>
			<i class="fas fa-pencil-alt"></i>
		</a>

		<div class="page-title">{model ? model.name : ''}</div>

		<a href="/recipes" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="body">
		<div class="top-buttons-drawer-container">
			<div class="top-buttons-drawer" class:open={topDrawerIsOpen}>
				<div class="top-buttons-drawer-wrap">
					<div class="top-buttons-drawer-content horizontal-buttons-wrap">
						<!-- <a href="/shareRecipe/{data.id}" class="wide-button">{shareButtonText}</a>
						<a href="/sendRecipe/{data.id}" class="wide-button">{$t('recipe.sendRecipe')}</a> -->
						<a onclick={copyAsText} class="wide-button with-badge">
							<span>{$t('recipe.copyAsText')}</span>
							<i class="fas fa-check badge toggled" class:visible={copyAsTextCompleted}></i>
						</a>
					</div>
				</div>

				<button type="button" onclick={toggleTopDrawer} class="top-drawer-handle">
					<i class="fas fa-angle-down"></i>
					<i class="fas fa-angle-up"></i>
				</button>
			</div>
		</div>

		<div class="content-wrap">
			<div class="recipe-wrap" bind:this={recipeContainer}>
				{#if !model}
					<div class="double-circle-loading">
						<div class="double-bounce1"></div>
						<div class="double-bounce2"></div>
					</div>
				{:else}
					<div>
						<img src={model.imageUri} class="image" width={model.imageWidth} height={model.imageHeight} alt={model.name} />

						{#if model.description}
							<div class="description">{model.description}</div>
						{/if}

						<pre hidden={videoIFrameSrc.length === 0} class="video-wrap"><iframe
								bind:this={videoIFrame}
								src={videoIFrameSrc}
								class="video-iframe"
								allowfullscreen></iframe></pre>

						{#if model.prepDuration || model.cookDuration}
							<div class="prep-cook-duration-wrap">
								{#if model.prepDuration}
									<div class="duration-side">
										<div class="duration-label">
											<span>{$t('recipe.prep')}</span>:
											<span>{TimeFormatter.format(model.prepDuration, $t('hoursLetter'), $t('minutesLetter'))}</span>
										</div>
									</div>
								{/if}

								{#if model.cookDuration}
									<div class="duration-side">
										<div class="duration-label">
											<span>{$t('recipe.cook')}</span>:
											<span>{TimeFormatter.format(model.cookDuration, $t('hoursLetter'), $t('minutesLetter'))}</span>
										</div>
									</div>
								{/if}
							</div>
						{/if}

						{#if model.ingredients.length > 0}
							<section>
								<header>{$t('recipe.ingredients')}</header>

								<table class="recipe-ingredients-table">
									<tbody>
										{#each model.ingredients as ingredient}
											<tr class:missing={ingredient.missing}>
												<td class="recipe-ingredient-name">
													{ingredient.parentName ? `${ingredient.parentName} (${ingredient.name})` : ingredient.name}
													<span class="icons-container">
														{#if ingredient.hasNutritionData}
															<i class="fas fa-clipboard" title={$t('hasNutrition')} aria-label={$t('hasNutrition')}></i>
														{/if}
														{#if ingredient.hasPriceData}
															<i class="fas fa-tag" title={$t('hasPrice')} aria-label={$t('hasPrice')}></i>
														{/if}
													</span>
												</td>
												<td>
													<IngredientAmount bind:amount={ingredient.amount} bind:unit={ingredient.unit} />
												</td>
											</tr>
										{/each}
									</tbody>
								</table>
							</section>
						{/if}

						{#if model.instructions}
							<section class="instructions-wrap">
								<header>{$t('recipe.instructions')}</header>
								<div class="instructions" contenteditable="false" bind:innerHTML={instructionsInHtml} />
							</section>
						{/if}

						{#if servingsSelectorIsVisible}
							<ServingsSelector bind:recipe={model} />
						{/if}

						{#if model.nutritionSummary.isSet}
							<section>
								<header>{$t('recipe.nutritionPerServing')}</header>

								{#if model.nutritionSummary.calories}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('calories')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.calories)} kcal</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.caloriesFromDaily}
												<div
													style="width: {model.nutritionSummary.caloriesFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.caloriesFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.fat}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('fat')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.fat)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap"></div>
									</div>
								{/if}

								{#if model.nutritionSummary.saturatedFat}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('saturatedFat')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.saturatedFat)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.saturatedFatFromDaily}
												<div
													style="width: {model.nutritionSummary.saturatedFatFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.saturatedFatFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.carbohydrate}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('carbohydrate')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.carbohydrate)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.carbohydrateFromDaily}
												<div
													style="width: {model.nutritionSummary.carbohydrateFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.carbohydrateFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.sugars}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('sugars')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.sugars)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.sugarsFromDaily}
												<div
													style="width: {model.nutritionSummary.sugarsFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.sugarsFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.addedSugars}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('addedSugars')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.addedSugars)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.addedSugarsFromDaily}
												<div
													style="width: {model.nutritionSummary.addedSugarsFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.addedSugarsFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.fiber}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('fiber')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.fiber)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.fiberFromDaily}
												<div
													style="width: {model.nutritionSummary.fiberFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.fiberFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.protein}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('protein')}</div>
											<div>{AmountFormatter.formatGrams(model.nutritionSummary.protein)} g</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.proteinFromDaily}
												<div
													style="width: {model.nutritionSummary.proteinFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.proteinFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.sodium}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('sodium')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.sodium)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.sodiumFromDaily}
												<div
													style="width: {model.nutritionSummary.sodiumFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.sodiumFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.cholesterol}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('cholesterol')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.cholesterol)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.cholesterolFromDaily}
												<div
													style="width: {model.nutritionSummary.cholesterolFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.cholesterolFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.vitaminA}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('vitaminA')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.vitaminA)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.vitaminAFromDaily}
												<div
													style="width: {model.nutritionSummary.vitaminAFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.vitaminAFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.vitaminC}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('vitaminC')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.vitaminC)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.vitaminCFromDaily}
												<div
													style="width: {model.nutritionSummary.vitaminCFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.vitaminCFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.vitaminD}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('vitaminD')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.vitaminD)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.vitaminDFromDaily}
												<div
													style="width: {model.nutritionSummary.vitaminDFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.vitaminDFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.calcium}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('calcium')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.calcium)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.calciumFromDaily}
												<div
													style="width: {model.nutritionSummary.calciumFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.calciumFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.iron}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('iron')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.iron)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.ironFromDaily}
												<div
													style="width: {model.nutritionSummary.ironFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.ironFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.potassium}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('potassium')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.potassium)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.potassiumFromDaily}
												<div
													style="width: {model.nutritionSummary.potassiumFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.potassiumFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}

								{#if model.nutritionSummary.magnesium}
									<div class="nutrition-row">
										<div class="nutrition-row-quantity">
											<div>{$t('magnesium')}</div>
											<div>{AmountFormatter.formatMilligrams(model.nutritionSummary.magnesium)} mg</div>
										</div>
										<div class="nutrition-row-progress-wrap">
											{#if model.nutritionSummary.magnesiumFromDaily}
												<div
													style="width: {model.nutritionSummary.magnesiumFromDaily}%;"
													class="nutrition-row-progress {model.nutritionSummary.magnesiumFromDailyGrade}"
												></div>
											{/if}
										</div>
									</div>
								{/if}
							</section>
						{/if}

						{#if model.costSummary.isSet}
							<section class="cost-wrap">
								<div class="cost-content">
									<div>
										<span>{$t('recipe.cost')}</span>
										<span class="cost">{Formatter.money(model.costSummary.cost, currency, $user.language)}</span>
									</div>
									<div hidden={model.servings <= 1}>
										<span>{$t('recipe.perServing')}</span>
										<span class="cost">{Formatter.money(model.costSummary.costPerServing, currency, $user.language)}</span>
									</div>
								</div>
							</section>
						{/if}
					</div>
				{/if}
			</div>
		</div>

		<button type="button" onclick={closeDrawer} class="body-overlay" class:visible={topDrawerIsOpen}></button>
	</div>
</section>

<style lang="scss">
	.recipe-wrap {
		.prep-cook-duration-wrap {
			display: flex;
			justify-content: space-around;
			background: #fff;
			border: 1px solid #ddd;
			border-radius: 6px;
			padding: 15px;
			margin-bottom: 10px;
		}

		.image {
			border: 1px solid #ddd;
			border-radius: var(--border-radius);
			margin-bottom: 10px;
		}

		.duration-label {
			padding-left: 0;
			margin-bottom: 0;
			color: inherit;
		}

		.video-wrap {
			margin-bottom: 10px;
			font-size: 0;
		}
	}

	.nutrition-row {
		margin: 5px 0;
	}
	.nutrition-row-quantity {
		display: flex;
		justify-content: space-between;
		padding: 5px 15px;
		line-height: 28px;
	}
	.nutrition-row-progress-wrap {
		position: relative;
		height: 4px;
		background: #ddd;
		margin: 0 15px;
	}
	.nutrition-row-progress {
		position: absolute;
		top: 0;
		left: 0;
		height: 4px;

		&.good {
			background: var(--primary-color);
		}

		&.average {
			background: #f8a042;
		}

		&.bad {
			background: var(--danger-color);
		}

		&.terrible {
			background: var(--danger-color-dark);
		}
	}

	.recipe-ingredients-table {
		width: 100%;
		margin-bottom: 10px;

		.icons-container {
			margin-left: 5px;

			i {
				margin-left: 4px;
				color: var(--faded-color);

				&.fa-tag {
					font-size: 18px;
				}
			}
		}

		tr td {
			padding: 5px 15px;
			line-height: 28px;
		}

		tr:hover td {
			color: var(--primary-color-dark);
		}

		tr:nth-child(odd) {
			background: #eee;
		}

		tr td:nth-child(even) {
			text-align: right;
		}

		tr.missing td {
			color: var(--danger-color);
		}
	}

	.instructions-wrap {
		margin-bottom: 10px;

		.instructions {
			border: 1px solid #ddd;
			border-radius: var(--border-radius);
			border-top-left-radius: 0;
			border-top-right-radius: 0;
			border-top: none;
			padding: 15px;
			font-size: 1rem;
			line-height: 1.5rem;
		}
	}

	.cost-wrap {
		margin-top: 20px;
		line-height: 30px;

		.cost-content {
			text-align: right;

			.cost {
				color: var(--primary-color);
			}
		}
	}

	@media screen and (min-width: 1200px) {
		.recipe-ingredients-table .icons-container i.fa-tag {
			font-size: 22px;
		}

		.instructions-wrap .instructions {
			font-size: 1.2rem;
			line-height: 1.7rem;
		}
	}
</style>
