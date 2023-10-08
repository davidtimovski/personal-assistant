<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { tweened } from 'svelte/motion';
	import { cubicOut } from 'svelte/easing';
	import { page } from '$app/stores';

	import { t } from '$lib/localization/i18n';
	import { isOffline, user, state } from '$lib/stores';
	import { UsersService } from '$lib/services/usersService';
	import { RecipesService } from '$lib/services/recipesService';
	import type { RecipeModel } from '$lib/models/viewmodels/recipeModel';

	let recipes: RecipeModel[] | null = null;
	let editedId: number | undefined;
	const unsubscriptions: Unsubscriber[] = [];
	let recipesContainer: HTMLDivElement;
	let resizeObserver: ResizeObserver;
	let imageWidth: number;
	let imageHeight: number;

	// Progress bar
	let progressBarActive = false;
	const progress = tweened(0, {
		duration: 500,
		easing: cubicOut
	});
	let progressIntervalId: number | undefined;
	let progressBarVisible = false;

	let usersService: UsersService;
	let recipesService: RecipesService;

	function sync() {
		startProgressBar();

		recipesService.getAll();
	}

	function setRecipesFromState() {
		if ($state.recipes === null) {
			throw new Error('Recipes not loaded yet');
		}

		recipes = $state.recipes
			.sort((a: RecipeModel, b: RecipeModel) => {
				const aDate = new Date(a.lastOpenedDate);
				const bDate = new Date(b.lastOpenedDate);

				if (aDate > bDate) {
					return -1;
				} else if (aDate < bDate) {
					return 1;
				}
				return 0;
			})
			.map((recipe: RecipeModel) => {
				if (recipe.ingredientsMissing !== 0) {
					const missingIngredientsKey =
						recipe.ingredientsMissing > 1 ? 'recipes.missingIngredients' : 'recipes.missingIngredient';
					recipe.ingredientsMissingLabel = recipe.ingredientsMissing + ' ' + $t(missingIngredientsKey);
				}

				return recipe;
			});
	}

	unsubscriptions.push(
		state.subscribe((s) => {
			if (s.recipes === null) {
				return;
			}

			setRecipesFromState();

			if (!s.fromCache) {
				finishProgressBar();
			}
		})
	);

	function startProgressBar() {
		progressBarActive = true;
		progress.set(10);

		progressIntervalId = window.setInterval(() => {
			if ($progress < 85) {
				progress.update((x) => {
					x += 15;
					return x;
				});
			} else if (progressIntervalId) {
				window.clearInterval(progressIntervalId);
			}
		}, 500);

		progressBarVisible = true;
	}

	function finishProgressBar() {
		window.clearInterval(progressIntervalId);
		progress.set(100);
		window.setTimeout(() => {
			progressBarActive = false;
			progressBarVisible = false;
		}, 500);
	}

	onMount(() => {
		// Set image width and height to avoid reflows
		resizeObserver = new ResizeObserver(() => {
			imageWidth = recipesContainer.offsetWidth;
			imageHeight = recipesContainer.offsetWidth / 2;
		});
		resizeObserver.observe(document.body);

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		usersService = new UsersService();
		recipesService = new RecipesService();

		if ($state.recipes === null) {
			startProgressBar();
		}
	});

	onDestroy(() => {
		resizeObserver?.disconnect();
		usersService?.release();
		recipesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap-loader">
		<div class="title-wrap">
			<a href="/menu" class="profile-image-container" title={$t('recipes.menu')} aria-label={$t('recipes.menu')}>
				<img src={$user.imageUri} class="profile-image" width="40" height="40" alt="" />
			</a>

			<div class="page-title" />
			<button
				type="button"
				on:click={sync}
				class="sync-button"
				disabled={$isOffline || progressBarActive}
				title={$t('recipes.refresh')}
				aria-label={$t('recipes.refresh')}
			>
				<i class="fas fa-sync-alt" />
			</button>
		</div>
		<div class="progress-bar">
			<div class="progress" class:visible={progressBarVisible} style="width: {$progress}%;" />
		</div>
	</div>

	<div class="content-wrap recipes">
		<div class="recipes-wrap" bind:this={recipesContainer}>
			{#if recipes}
				{#each recipes as recipe}
					<a
						href="/recipe/{recipe.id}"
						class="recipe"
						class:highlighted={recipe.id === editedId}
						class:is-shared={recipe.sharingState !== 0 && recipe.sharingState !== 1}
						class:pending-share={recipe.sharingState === 1}
						class:ingredients-missing={recipe.ingredientsMissing > 0}
					>
						<img src={recipe.imageUri} alt={recipe.name} width={imageWidth} height={imageHeight} />
						<div class="recipe-name">
							<i class="fas fa-users" />
							<i class="fas fa-user-clock" />
							<span>{recipe.name}</span>
						</div>
						<div class="num-ingredients-missing">{recipe.ingredientsMissingLabel}</div>
					</a>
				{/each}
			{/if}
		</div>

		<div class="centering-wrap">
			<a href="/editRecipe/0" class="new-button" title={$t('recipes.newRecipe')} aria-label={$t('recipes.newRecipe')}>
				<i class="fas fa-plus" />
			</a>
		</div>
	</div>
</section>

<style lang="scss">
	.content-wrap.recipes {
		padding-top: 15px;
	}

	.recipes-wrap {
		margin-bottom: 30px;
	}

	.recipe {
		position: relative;
		display: block;
		background: #ddd;
		border-radius: var(--border-radius);
		margin-bottom: 15px;
		font-size: 0;
		text-decoration: none;
		color: inherit;
		user-select: none;

		&:hover {
			color: var(--primary-color-dark);
		}

		&.is-shared .recipe-name .fa-users {
			display: inline;
		}

		&.pending-share .recipe-name .fa-user-clock {
			display: inline;
		}

		img {
			border-radius: var(--border-radius);
			box-shadow: var(--box-shadow);
		}
	}

	.recipe-name {
		position: absolute;
		top: 15px;
		left: -3px;
		background: #fafafa;
		border: 1px solid #eee;
		border-left: 3px solid var(--primary-color);
		border-top-right-radius: var(--border-radius);
		border-bottom-right-radius: 15px;
		box-shadow: var(--box-shadow);
		padding: 5px 16px 6px 15px;
		font-size: 1rem;
		line-height: 1.2rem;
		color: inherit;

		i {
			display: none;
			margin-right: 5px;
			color: var(--primary-color);
		}
	}

	.num-ingredients-missing {
		display: none;
		position: absolute;
		right: -3px;
		bottom: 15px;
		background: #fafafa;
		border: 1px solid #eee;
		border-right: 3px solid var(--danger-color);
		border-top-left-radius: var(--border-radius);
		border-bottom-left-radius: 12px;
		box-shadow: var(--box-shadow);
		padding: 5px 15px 6px 16px;
		text-align: right;
		font-size: 0.9rem;
		line-height: 1rem;
		color: var(--danger-color);
	}
	.ingredients-missing .num-ingredients-missing {
		display: block;
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .recipe:hover {
		color: var(--primary-color);
	}
</style>
