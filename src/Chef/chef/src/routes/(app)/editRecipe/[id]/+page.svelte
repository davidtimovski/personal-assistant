<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationUtil } from '../../../../../../../Core/shared2/utils/validationUtils';
	import { ValidationErrors } from '../../../../../../../Core/shared2/models/validationErrors';

	import { t } from '$lib/localization/i18n';
	import { user, alertState, ingredientPickerState } from '$lib/stores';
	import { RecipesService } from '$lib/services/recipesService';
	import { UsersService } from '$lib/services/usersService';
	import type { SharingState } from '$lib/models/viewmodels/sharingState';
	import { EditRecipeIngredient } from '$lib/models/viewmodels/editRecipeIngredient';
	import { CreateRecipe } from '$lib/models/server/requests/createRecipe';
	import { UpdateRecipe } from '$lib/models/server/requests/updateRecipe';
	import type { ChefUser } from '$lib/models/chefUser';
	import { UpdateRecipeIngredient } from '$lib/models/server/requests/updateRecipeIngredient';
	import type { IngredientSuggestion } from '$lib/models/viewmodels/ingredientSuggestions';
	import Variables from '$lib/variables';

	import IngredientPicker from '$lib/components/IngredientPicker.svelte';
	import ServingsSelectorSimple from '$lib/components/ServingsSelectorSimple.svelte';
	import { IngredientPickEvent } from '$lib/models/ingredientPickerState';

	export let data: PageData;

	const isNew = data.id === 0;

	let name = '';
	let description: string | null;
	let ingredients = new Array<EditRecipeIngredient>();
	let instructions: string | null;
	let prepDuration: string;
	let cookDuration: string;
	let servings = 1;
	let imageUri = Variables.urls.defaultRecipeImageUrl;
	let videoUrl: string | null;
	let sharingState: SharingState;
	let userIsOwner: boolean;
	let imageInput: HTMLInputElement;
	let imageIsUploading = false;
	let nameInput: HTMLInputElement;
	let nameIsInvalid: boolean;
	let videoUrlIsInvalid: boolean;
	let recipeIngredientIds = new Array<number>();
	let prepDurationHours = '00';
	let prepDurationMinutes = '00';
	let cookDurationHours = '00';
	let cookDurationMinutes = '00';
	let confirmationInProgress = false;
	let saveButtonText: string;
	let deleteButtonText: string;
	let leaveButtonText: string;
	let saveButtonIsLoading = false;
	let deleteButtonIsLoading = false;
	let leaveButtonIsLoading = false;
	let videoIFrame: HTMLIFrameElement;
	let videoIFrameSrc = '';
	let addIngredientsInputPlaceholder: string;
	let measuringUnits: (string | null)[];
	let imperialSystem: boolean;
	let loading = !isNew;
	const unsubscriptions: Unsubscriber[] = [];

	unsubscriptions.push(
		alertState.subscribe((value) => {
			if (value.hidden) {
				nameIsInvalid = false;
				videoUrlIsInvalid = false;
			}
		})
	);

	unsubscriptions.push(
		ingredientPickerState.subscribe((value) => {
			if (value.event !== IngredientPickEvent.Added) {
				return;
			}

			const ingredientName = <string>value.data;
			if (existsInIngredients(ingredientName)) {
				return;
			}

			ingredients = ingredients.concat(new EditRecipeIngredient(0, ingredientName, '', null, false, false, true));
		})
	);

	unsubscriptions.push(
		ingredientPickerState.subscribe((value) => {
			if (value.event !== IngredientPickEvent.Selected) {
				return;
			}

			const ingredient = <IngredientSuggestion>value.data;

			const unit = imperialSystem ? ingredient.unitImperial : ingredient.unit;
			ingredients = ingredients.concat(
				new EditRecipeIngredient(ingredient.id, ingredient.name, '', unit, ingredient.hasNutritionData, ingredient.hasPriceData, false)
			);
		})
	);

	let recipesService: RecipesService;
	let usersService: UsersService;

	$: canSave = !ValidationUtil.isEmptyOrWhitespace(name) && !videoUrlIsInvalid;

	function videoUrlChanged() {
		if (!videoUrl) {
			videoUrlIsInvalid = false;
			return;
		}

		try {
			videoIFrameSrc = recipesService.videoUrlToEmbedSrc(videoUrl, $user.language);

			// Hack for back button iframe issue
			videoIFrame?.contentWindow?.location.replace(videoIFrameSrc);

			return;
		} catch {
			videoUrlIsInvalid = true;
			videoIFrameSrc = '';
		}
	}

	function prepDurationChanged() {
		const prepHours = !prepDurationHours
			? '00'
			: parseInt(prepDurationHours, 10).toLocaleString('en-US', {
					minimumIntegerDigits: 2
				});
		const prepMinutes = !prepDurationMinutes
			? '00'
			: parseInt(prepDurationMinutes, 10).toLocaleString('en-US', {
					minimumIntegerDigits: 2
				});

		if (prepHours === '00' && prepMinutes === '00') {
			prepDuration = '';
		} else {
			prepDuration = `${prepHours}:${prepMinutes}`;
		}
	}

	function cookDurationChanged() {
		const cookHours = !cookDurationHours
			? '00'
			: parseInt(cookDurationHours, 10).toLocaleString('en-US', {
					minimumIntegerDigits: 2
				});
		const cookMinutes = !cookDurationMinutes
			? '00'
			: parseInt(cookDurationMinutes, 10).toLocaleString('en-US', {
					minimumIntegerDigits: 2
				});

		if (cookHours === '00' && cookMinutes === '00') {
			cookDuration = '';
		} else {
			cookDuration = `${cookHours}:${cookMinutes}`;
		}
	}

	function existsInIngredients(ingredientName: string): boolean {
		const duplicates = ingredients.filter((i) => {
			return i.name.trim().toUpperCase() === ingredientName.trim().toUpperCase();
		});
		return duplicates.length > 0;
	}

	function toggleUnit(ingredient: EditRecipeIngredient) {
		const index = measuringUnits.indexOf(ingredient.unit);
		ingredient.unit = index === measuringUnits.length - 1 ? measuringUnits[0] : measuringUnits[index + 1];

		// Hack to activate the this.canSave() getter
		if (instructions) {
			instructions = instructions + ' ';
			instructions = instructions.trim();
		} else {
			instructions = ' ';
			instructions = null;
		}

		ingredients = [...ingredients];
	}

	function removeIngredient(ingredient: EditRecipeIngredient) {
		if (!ingredient.isNew) {
			ingredientPickerState.update((x) => {
				x.event = IngredientPickEvent.Unselected;
				x.data = ingredient.id;
				return x;
			});
		}

		ingredients = ingredients.filter((x) => x !== ingredient);
	}

	async function uploadImage() {
		if (imageInput.files?.length !== 1) {
			throw new Error('No files were selected');
		}

		imageIsUploading = true;

		try {
			imageUri = await recipesService.uploadTempImage(imageInput.files[0]);
		} catch {
			saveButtonIsLoading = false;
		}

		imageIsUploading = false;
	}

	function removeImage() {
		imageUri = Variables.urls.defaultRecipeImageUrl;
	}

	$: imageIsNotDefault = imageUri !== Variables.urls.defaultRecipeImageUrl;

	function parseIngredientsAmount(ingredients: EditRecipeIngredient[]) {
		return ingredients.map((ingredient: EditRecipeIngredient) => {
			let amount: number;
			if (ingredient.amount && typeof ingredient.amount === 'string' && ingredient.amount.includes('/')) {
				const fractions = ingredient.amount.split('/');
				amount = Number((parseInt(fractions[0], 10) / parseInt(fractions[1], 10)).toFixed(2));
			} else {
				amount = parseFloat(ingredient.amount);
			}

			return new UpdateRecipeIngredient(ingredient.id, ingredient.name, amount, ingredient.unit);
		});
	}

	async function save() {
		saveButtonIsLoading = true;
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const result = RecipesService.validateEdit(name);

		if (result.valid) {
			nameIsInvalid = false;

			if (!isNew) {
				try {
					await recipesService.update(
						new UpdateRecipe(
							data.id,
							name,
							description,
							parseIngredientsAmount(ingredients),
							instructions,
							prepDuration.length > 0 ? prepDuration : null,
							cookDuration.length > 0 ? cookDuration : null,
							servings,
							imageUri,
							videoUrl
						)
					);

					nameIsInvalid = false;

					await goto('/recipes?edited=' + data.id);
				} catch (e) {
					if (e instanceof ValidationErrors) {
						nameIsInvalid = e.fields.includes('Name');
					}

					saveButtonIsLoading = false;
				}
			} else {
				try {
					const newId = await recipesService.create(
						new CreateRecipe(
							name,
							description,
							parseIngredientsAmount(ingredients),
							instructions,
							prepDuration.length > 0 ? prepDuration : null,
							cookDuration.length > 0 ? cookDuration : null,
							servings,
							imageUri,
							videoUrl
						)
					);

					nameIsInvalid = false;

					await goto('/recipes?edited=' + newId);
				} catch (e) {
					if (e instanceof ValidationErrors) {
						nameIsInvalid = e.fields.includes('Name');
					}

					saveButtonIsLoading = false;
				}
			}
		} else {
			nameIsInvalid = true;
			saveButtonIsLoading = false;
		}
	}

	async function deleteRecipe() {
		if (confirmationInProgress) {
			deleteButtonIsLoading = true;

			await recipesService.delete(data.id);

			alertState.update((x) => {
				x.showSuccess('editRecipe.deleteSuccessful');
				return x;
			});
			goto('/recipes');
		} else {
			deleteButtonText = $t('sure');
			confirmationInProgress = true;
		}
	}

	async function leaveRecipe() {
		if (confirmationInProgress) {
			leaveButtonIsLoading = true;

			await recipesService.leave(data.id);

			alertState.update((x) => {
				x.showSuccess('editRecipe.youHaveLeftTheRecipe');
				return x;
			});
			goto('/recipes');
		} else {
			leaveButtonText = $t('sure');
			confirmationInProgress = true;
		}
	}

	async function cancel() {
		if (!confirmationInProgress) {
			back();
		}
		deleteButtonText = $t('delete');
		confirmationInProgress = false;
	}

	function back() {
		if (isNew) {
			goto('/recipes');
		} else {
			goto(`/recipe/${data.id}`);
		}
	}

	onMount(async () => {
		deleteButtonText = $t('delete');
		leaveButtonText = $t('editRecipe.leave');

		recipesService = new RecipesService();
		usersService = new UsersService();

		if (isNew) {
			userIsOwner = true;

			saveButtonText = $t('editRecipe.create');
		} else {
			saveButtonText = $t('save');

			const recipe = await recipesService.getForUpdate(data.id);
			if (recipe === null) {
				throw new Error('Recipe not found');
			}

			name = recipe.name;
			description = recipe.description;
			ingredients = recipe.ingredients;
			instructions = recipe.instructions;
			prepDuration = recipe.prepDuration;
			cookDuration = recipe.cookDuration;
			servings = recipe.servings;
			imageUri = recipe.imageUri;
			videoUrl = recipe.videoUrl;
			sharingState = recipe.sharingState;
			userIsOwner = recipe.userIsOwner;

			recipeIngredientIds = recipe.ingredients.map((x) => x.id);

			if (recipe.videoUrl) {
				videoIFrameSrc = recipesService.videoUrlToEmbedSrc(recipe.videoUrl, $user.language);
			}

			if (recipe.prepDuration) {
				prepDurationHours = recipe.prepDuration.substring(0, 2);
				prepDurationMinutes = recipe.prepDuration.substring(3, 5);
			}
			if (recipe.cookDuration) {
				cookDurationHours = recipe.cookDuration.substring(0, 2);
				cookDurationMinutes = recipe.cookDuration.substring(3, 5);
			}

			loading = false;
		}

		usersService.get<ChefUser>().then((user: ChefUser) => {
			imperialSystem = user.imperialSystem;

			const units = user.imperialSystem ? [null, 'oz', 'cup', 'tbsp', 'tsp', 'pinch'] : [null, 'g', 'ml', 'tbsp', 'tsp', 'pinch'];

			if (!isNew) {
				const metricUnits = ['g', 'ml'];
				const imperialUnits = ['oz', 'cup'];

				if (user.imperialSystem) {
					const hasMetricUnitIngredients = ingredients.filter((x) => x.unit && metricUnits.includes(x.unit)).length > 0;
					if (hasMetricUnitIngredients) {
						units.push(...metricUnits);
					}
				} else {
					const hasImperialUnitIngredients = ingredients.filter((x) => x.unit && imperialUnits.includes(x.unit)).length > 0;
					if (hasImperialUnitIngredients) {
						units.push(...imperialUnits);
					}
				}
			}

			measuringUnits = units;
		});
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		recipesService?.release();
		usersService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			{#if isNew}
				<span>{$t('editRecipe.newRecipe')}</span>
			{:else}
				<span>{$t('editRecipe.edit')}</span>&nbsp;<span class="colored-text">{name}</span>
			{/if}
		</div>
		<button type="button" on:click={back} class="back-button">
			<i class="fas fa-times" />
		</button>
	</div>

	<div class="content-wrap">
		{#if loading}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else}
			<form on:submit|preventDefault={save} class="edit-recipe-form">
				<div class="form-control">
					<input
						type="text"
						bind:value={name}
						bind:this={nameInput}
						maxlength="50"
						class:invalid={nameIsInvalid}
						placeholder={$t('editRecipe.recipeName')}
						aria-label={$t('editRecipe.recipeName')}
					/>
				</div>

				<div class="edit-image-wrap">
					<img src={imageUri} class="image" alt={$t('editRecipe.imageOfMeal')} />
					<div class="image-loader" class:uploading={imageIsUploading}>
						<i class="fas fa-circle-notch fa-spin" />
					</div>
					<div hidden={imageIsUploading} class="edit-image-buttons">
						<input type="file" on:change={uploadImage} bind:this={imageInput} id="file-input" accept="image/*" />
						<label for="file-input">{$t('editRecipe.change')}</label>
						<button type="button" hidden={!imageIsNotDefault} on:click={removeImage}>
							{$t('editRecipe.remove')}
						</button>
					</div>
				</div>

				<div class="form-control">
					<textarea
						bind:value={description}
						class="small"
						maxlength="250"
						placeholder={$t('editRecipe.description')}
						aria-label={$t('editRecipe.description')}
					/>
				</div>

				<div class="form-control">
					<div class="video-url-input-wrap">
						<input
							type="text"
							bind:value={videoUrl}
							on:change={videoUrlChanged}
							class:invalid={videoUrlIsInvalid}
							placeholder={$t('editRecipe.youTubeUrl')}
							aria-label={$t('editRecipe.youTubeUrl')}
						/>
						<i class="fab fa-youtube" />
					</div>

					<pre hidden={videoIFrameSrc.length === 0} class="video-wrap"><iframe
							bind:this={videoIFrame}
							src={videoIFrameSrc}
							class="video-iframe"
							title={$t('editRecipe.youTubeUrl')}
							allowfullscreen
						/></pre>
				</div>

				<div class="form-control">
					<label class="label" class:dark={ingredients.length > 0}>
						<span>{$t('editRecipe.ingredients')}</span>: <span>{ingredients.length}</span>
					</label>

					<IngredientPicker
						inputPlaceholder={addIngredientsInputPlaceholder}
						addingEnabled={true}
						userIngredientsAllowed={userIsOwner}
						{recipeIngredientIds}
					/>

					<div hidden={ingredients.length === 0} class="new-ingredients-wrap">
						{#each ingredients as ingredient}
							<div class="new-ingredient au-animate animate-fade-in">
								<div class="ingredient-wrap">
									<div class="ingredient-name-wrap">
										{#if ingredient.isNew}
											<input type="text" bind:value={ingredient.name} maxlength="50" class="ingredient-name-input" />
										{:else}
											<div class="ingredient-name">
												<span>{ingredient.name}</span>
												<span class="icons-container">
													{#if ingredient.hasNutritionData}
														<i class="fas fa-clipboard" title={$t('hasNutrition')} aria-label={$t('hasNutrition')} />
													{/if}

													{#if ingredient.hasPriceData}
														<i class="fas fa-tag" title={$t('hasPrice')} aria-label={$t('hasPrice')} />
													{/if}
												</span>
											</div>
										{/if}
									</div>

									<button
										type="button"
										on:click={() => removeIngredient(ingredient)}
										class="remove-button"
										title={$t('editRecipe.removeIngredient')}
										aria-label={$t('editRecipe.removeIngredient')}
									>
										<i class="fas fa-times-circle" />
									</button>
								</div>

								<div class="ingredient-amount-input-wrap">
									<input
										type="text"
										bind:value={ingredient.amount}
										hidden={ingredient.unit === 'pinch'}
										class="amount-input"
										maxlength="4"
										placeholder={$t('editRecipe.amount')}
										aria-label={$t('editRecipe.amount')}
									/>
									<button
										type="button"
										on:click={() => toggleUnit(ingredient)}
										class="unit-toggle {ingredient.unit === null ? 'fas fa-asterisk' : ''}"
										class:pinch={ingredient.unit === 'pinch'}
										title={$t('editRecipe.toggleUnitOfMeasure')}
										aria-label={$t('editRecipe.toggleUnitOfMeasure')}>{ingredient.unit ? $t(ingredient.unit) : ''}</button
									>
								</div>
							</div>
						{/each}
					</div>
				</div>

				<div class="form-control">
					<textarea
						bind:value={instructions}
						maxlength="5000"
						placeholder={$t('editRecipe.instructions')}
						aria-label={$t('editRecipe.instructions')}
					/>
				</div>

				<div class="form-control">
					<div class="prep-cook-duration-wrap">
						<div class="duration-side">
							<div class="label">{$t('editRecipe.prepDuration')}</div>
							<input
								type="number"
								bind:value={prepDurationHours}
								on:keyup={prepDurationChanged}
								min="0"
								max="23"
								class="duration-input hours"
								aria-label={$t('editRecipe.prepDurationHours')}
							/>
							<span>{$t('hoursLetter')}</span>&nbsp;
							<input
								type="number"
								bind:value={prepDurationMinutes}
								on:keyup={prepDurationChanged}
								min="0"
								max="59"
								class="duration-input minutes"
								aria-label={$t('editRecipe.prepDurationMinutes')}
							/>
							<span>{$t('minutesLetter')}</span>
						</div>
						<div class="duration-side">
							<div class="label">{$t('editRecipe.cookDuration')}</div>
							<input
								type="number"
								bind:value={cookDurationHours}
								on:keyup={cookDurationChanged}
								min="0"
								max="23"
								class="duration-input hours"
								aria-label={$t('editRecipe.cookDurationHours')}
							/>
							<span>{$t('hoursLetter')}</span>&nbsp;
							<input
								type="number"
								bind:value={cookDurationMinutes}
								on:keyup={cookDurationChanged}
								min="0"
								max="59"
								class="duration-input minutes"
								aria-label={$t('editRecipe.cookDurationMinutes')}
							/>
							<span>{$t('minutesLetter')}</span>
						</div>
					</div>
				</div>

				<ServingsSelectorSimple bind:servings />
			</form>

			<hr />

			<div class="save-delete-wrap">
				{#if !confirmationInProgress}
					<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
						<span class="button-loader" class:loading={saveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{saveButtonText}</span>
					</button>
				{/if}

				{#if !isNew && sharingState !== 3}
					<button
						type="button"
						on:click={deleteRecipe}
						class="button danger-button"
						disabled={deleteButtonIsLoading}
						class:confirm={confirmationInProgress}
					>
						<span class="button-loader" class:loading={deleteButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{deleteButtonText}</span>
					</button>
				{/if}

				{#if sharingState === 3}
					<button
						type="button"
						on:click={leaveRecipe}
						class="button danger-button"
						disabled={leaveButtonIsLoading}
						class:confirm={confirmationInProgress}
					>
						<span class="button-loader" class:loading={leaveButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{leaveButtonText}</span>
					</button>
				{/if}

				{#if isNew || confirmationInProgress}
					<button type="button" on:click={cancel} class="button secondary-button">
						{$t('cancel')}
					</button>
				{/if}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.label {
		display: block;
		padding-left: 5px;
		margin-bottom: 10px;
		color: #888;

		&.dark {
			color: var(--regular-color);
		}
	}

	.edit-image-wrap {
		position: relative;
		margin-bottom: 10px;
		font-size: 0;

		.image {
			width: calc(100% - 2px);
			border: 1px solid #ddd;
			border-radius: var(--border-radius);
		}

		.image-loader {
			position: absolute;
			top: 0;
			left: 0;
			display: flex;
			align-items: center;
			justify-content: center;
			width: 100%;
			height: 100%;
			background: rgba(0, 0, 0, 0.4);
			border-radius: var(--border-radius);
			opacity: 0;

			transition: opacity var(--transition);

			i {
				font-size: 50px;
				color: var(--primary-color);
			}

			&.uploading {
				opacity: 1;
			}
		}

		.edit-image-buttons {
			position: absolute;
			bottom: 12px;
			right: 12px;
			font-size: 0;
		}

		input[type='file'] {
			display: none;
		}

		label {
			display: inline-block;
			background: var(--primary-color);
			border-radius: var(--border-radius);
			box-shadow: var(--box-shadow);
			padding: 5px 12px;
			font-size: 1.2rem;
			line-height: 1.5rem;
			color: #fafafa;
			cursor: pointer;
			white-space: nowrap;
			transition: background var(--transition);
		}

		button {
			background: var(--primary-color);
			border: none;
			outline: none;
			border-radius: var(--border-radius);
			padding: 5px 12px;
			margin-left: 10px;
			font-size: 1.2rem;
			line-height: 1.5rem;
			color: #fafafa;
			cursor: pointer;
			transition: background var(--transition);
		}

		label:hover,
		button:hover {
			background: var(--primary-color-dark);
		}
	}

	.video-url-input-wrap {
		position: relative;
		margin-bottom: 10px;

		input {
			width: calc(100% - 68px);
			padding-right: 56px;
			line-height: 45px;
		}

		.fa-youtube {
			position: absolute;
			top: 10px;
			right: 10px;
			font-size: 29px;
			color: var(--faded-color);
		}
	}

	.edit-recipe-form {
		.prep-cook-duration-wrap {
			display: flex;
			justify-content: space-around;
			margin-bottom: 20px;

			.duration-side label {
				padding: 0;
				margin-bottom: 5px;
				text-align: center;
			}

			.duration-input {
				width: 26px;
				text-align: center;
			}
		}
	}

	.new-ingredients-wrap {
		margin-top: 10px;

		.new-ingredient {
			&:nth-child(n + 2) {
				margin-top: 15px;
			}

			.fa-list {
				display: none;
				position: absolute;
				top: 10px;
				right: 10px;
				font-size: 27px;
				color: var(--primary-color);
			}

			.ingredient-wrap {
				display: flex;
			}

			.ingredient-name-wrap {
				flex-grow: 1;
				position: relative;
				width: 100%;
			}

			.ingredient-name-input {
				border: 1px solid #ddd;
				border-radius: var(--border-radius);
				border-bottom-left-radius: 0;
				padding: 7px 12px;
				line-height: 29px;
			}
			.ingredient-name {
				display: flex;
				justify-content: space-between;
				background: #eeffee;
				border: 1px solid #ddd;
				border-radius: var(--border-radius);
				border-bottom-left-radius: 0;
				padding: 7px 12px;
				line-height: 29px;
				color: var(--primary-color-dark);

				.icons-container {
					display: flex;
					align-items: center;
					justify-content: center;
					margin-left: 15px;

					i {
						vertical-align: middle;
						margin-left: 4px;
						font-size: 22px;
						color: var(--faded-color);

						&.fa-tag {
							margin-left: 10px;
							font-size: 23px;
						}
					}
				}
			}

			.ingredient-amount-input-wrap {
				display: inline-flex;
				border: 1px solid #ddd;
				border-top: none;
				border-bottom-left-radius: var(--border-radius);
				border-bottom-right-radius: var(--border-radius);
			}

			.amount-input {
				width: auto;
				max-width: 55px;
				border: none;
				line-height: 37px;
			}

			.unit-toggle {
				background: var(--primary-color);
				border: none;
				border-bottom-right-radius: var(--border-radius);
				outline: none;
				padding: 0 12px;
				font-size: 20px;
				line-height: 37px;
				color: #fafafa;

				&.pinch {
					border-bottom-left-radius: var(--border-radius);
				}

				&.fas {
					font-size: 15px;
				}

				&:hover {
					background: var(--primary-color-dark);
				}
			}

			.remove-button {
				background: transparent;
				border: none;
				outline: none;
				min-width: 45px;
				height: 45px;
				font-size: 27px;
				text-align: right;
				color: var(--primary-color);
				cursor: pointer;

				&:hover {
					color: var(--primary-color-dark);
				}
			}
		}
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .new-ingredient .unit-toggle > button:hover,
	.touch-device .new-ingredient .unit-toggle > .fa-asterisk:hover,
	.touch-device .edit-image-wrap label:hover {
		background: var(--primary-color);
	}

	@media screen and (min-width: 1200px) {
		.new-ingredients-wrap .new-ingredient {
			.ingredient-name {
				padding: 7px 12px;
				line-height: 29px;

				.icons-container i {
					font-size: 24px;

					&.fa-tag {
						font-size: 25px;
					}
				}
			}

			.amount-input {
				max-width: 95px;
			}
		}
	}
</style>
