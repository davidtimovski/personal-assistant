import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { ValidationResult, ValidationUtil } from '../../../../../Core/shared2/utils/validationUtils';

import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { state } from '$lib/stores';
import type { RecipeModel } from '$lib/models/viewmodels/recipeModel';
import type { ViewRecipe } from '$lib/models/viewmodels/viewRecipe';
import type { ShareRequest } from '$lib/models/viewmodels/shareRequest';
import type { EditRecipeModel } from '$lib/models/viewmodels/editRecipeModel';
import type { RecipeWithShares } from '$lib/models/viewmodels/recipeWithShares';
import type { SendRecipeModel } from '$lib/models/viewmodels/sendRecipeModel';
import type { ReceivedRecipe } from '$lib/models/viewmodels/receivedRecipe';
import type { ReviewIngredientsModel } from '$lib/models/viewmodels/reviewIngredientsModel';
import type { CanShareRecipe } from '$lib/models/server/responses/canShareRecipe';
import type { CanSendRecipe } from '$lib/models/server/responses/canSendRecipe';
import type { CreateRecipe } from '$lib/models/server/requests/createRecipe';
import type { UpdateRecipe } from '$lib/models/server/requests/updateRecipe';
import type { ShareRecipe } from '$lib/models/server/requests/shareRecipe';
import type { SetShareIsAccepted } from '$lib/models/server/requests/setShareIsAccepted';
import type { CreateSendRequest } from '$lib/models/server/requests/createSendRequest';
import { DeclineSendRequest } from '$lib/models/server/requests/declineSendRequest';
import type { ImportRecipe } from '$lib/models/server/requests/importRecipe';
import { State } from '$lib/models/state';
import Variables from '$lib/variables';

export class RecipesService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Chef');
	private readonly localStorage = new LocalStorageUtil();

	async getAll(includeCache = false) {
		if (includeCache) {
			let cachedRecipes = this.localStorage.getObject<RecipeModel[]>('homePageData');
			if (cachedRecipes) {
				state.set(new State(cachedRecipes, true));
			}
		}

		const recipes = await this.httpProxy.ajax<RecipeModel[]>(`${Variables.urls.api}/recipes`);

		this.localStorage.set('homePageData', JSON.stringify(recipes));
		state.set(new State(recipes, false));
	}

	async get(id: number, currency: string): Promise<ViewRecipe> {
		const result = await this.httpProxy.ajax<ViewRecipe>(`${Variables.urls.api}/recipes/${id}/${currency}`);

		// Update last opened date
		let cachedRecipes = this.localStorage.getObject<RecipeModel[]>('homePageData');
		let recipe = cachedRecipes?.find((x) => x.id === result.id);
		recipe!.lastOpenedDate = new Date();

		this.localStorage.set('homePageData', JSON.stringify(cachedRecipes));
		state.set(new State(cachedRecipes, false));

		return result;
	}

	getForUpdate(id: number): Promise<EditRecipeModel> {
		return this.httpProxy.ajax<EditRecipeModel>(`${Variables.urls.api}/recipes/${id}/update`);
	}

	getWithShares(id: number): Promise<RecipeWithShares> {
		return this.httpProxy.ajax<RecipeWithShares>(`${Variables.urls.api}/recipes/${id}/with-shares`);
	}

	getShareRequests(): Promise<ShareRequest[]> {
		return this.httpProxy.ajax<ShareRequest[]>(`${Variables.urls.api}/recipes/share-requests`);
	}

	getPendingShareRequestsCount(): Promise<number> {
		return this.httpProxy.ajax<number>(`${Variables.urls.api}/recipes/pending-share-requests-count`);
	}

	getForSending(id: number): Promise<SendRecipeModel> {
		return this.httpProxy.ajax<SendRecipeModel>(`${Variables.urls.api}/recipes/${id}/sending`);
	}

	getSendRequests(): Promise<ReceivedRecipe[]> {
		return this.httpProxy.ajax<ReceivedRecipe[]>(`${Variables.urls.api}/recipes/send-requests`);
	}

	getPendingSendRequestsCount(): Promise<number> {
		return this.httpProxy.ajax<number>(`${Variables.urls.api}/recipes/pending-send-requests-count`);
	}

	async tryImport(dto: ImportRecipe): Promise<number> {
		try {
			const result = await this.httpProxy.ajax<number>(`${Variables.urls.api}/recipes/try-import`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});

			this.getAll();

			return result;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	getForReview(id: number): Promise<ReviewIngredientsModel> {
		return this.httpProxy.ajax<ReviewIngredientsModel>(`${Variables.urls.api}/recipes/${id}/review`);
	}

	static validateEdit(name: string): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(name)) {
			result.fail('name');
		}

		return result;
	}

	async create(dto: CreateRecipe): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/recipes`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});

			this.getAll();

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async uploadTempImage(image: File): Promise<string> {
		try {
			const formData = new FormData();
			formData.append('image', image);

			const data: any = await this.httpProxy.ajaxUploadFile(`${Variables.urls.api}/recipes/upload-temp-image`, {
				method: 'post',
				body: formData
			});

			return data.tempImageUri;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(dto: UpdateRecipe): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/${id}`, {
				method: 'delete'
			});

			this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	canShareRecipeWithUser(email: string): Promise<CanShareRecipe> {
		return this.httpProxy.ajax<CanShareRecipe>(`${Variables.urls.api}/recipes/can-share-with-user/${email}`);
	}

	async share(dto: ShareRecipe): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/share`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async setShareIsAccepted(dto: SetShareIsAccepted): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/share-is-accepted`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async leave(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/${id}/leave`, {
				method: 'delete'
			});

			this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	canSendRecipeToUser(email: string, recipeId: number): Promise<CanSendRecipe> {
		return this.httpProxy.ajax<CanSendRecipe>(`${Variables.urls.api}/recipes/can-send-recipe-to-user/${email}/${recipeId}`);
	}

	async send(dto: CreateSendRequest): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/send`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async declineSendRequest(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/decline-send-request`, {
				method: 'put',
				body: window.JSON.stringify(new DeclineSendRequest(id))
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async deleteSendRequest(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/recipes/${id}/send-request`, {
				method: 'delete'
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	copyAsText(
		recipe: ViewRecipe,
		ingredientsLabel: string,
		instructionsLabel: string,
		youTubeUrlLabel: string,
		prepDurationLabel: string,
		minutesLetter: string,
		hoursLetter: string,
		cookDurationLabel: string,
		servingsLabel: string
	) {
		try {
			let text = recipe.name + '\n----------';

			if (recipe.description) {
				text += '\n\n' + recipe.description;
			}

			if (recipe.ingredients.length > 0) {
				text += `\n\n${ingredientsLabel}:`;

				for (const ingredient of recipe.ingredients) {
					text += `\n◾ ${ingredient.name}`;
					if (ingredient.amount) {
						text += ` - ${ingredient.amount + (ingredient.unit ? ' ' + ingredient.unit : '')}`;
					}
				}
			}

			if (recipe.instructions) {
				text += `\n\n${instructionsLabel}:`;
				text += '\n----------\n' + recipe.instructions + '\n----------';
			}

			if (recipe.videoUrl) {
				text += `\n\n${youTubeUrlLabel}: ${recipe.videoUrl}`;
			}

			if (recipe.prepDuration || recipe.cookDuration) {
				text += '\n';

				if (recipe.prepDuration) {
					const prepDurationHours = recipe.prepDuration.substring(0, 2);
					const prepDurationMinutes = recipe.prepDuration.substring(3, 5);

					text += `\n${prepDurationLabel}: `;

					if (parseInt(prepDurationHours, 10) === 0) {
						text += parseInt(prepDurationMinutes, 10) + minutesLetter;
					} else {
						text += parseInt(prepDurationHours, 10) + hoursLetter + ' ' + parseInt(prepDurationMinutes, 10) + minutesLetter;
					}
				}
				if (recipe.cookDuration) {
					const cookDurationHours = recipe.cookDuration.substring(0, 2);
					const cookDurationMinutes = recipe.cookDuration.substring(3, 5);

					text += `\n${cookDurationLabel}: `;

					if (parseInt(cookDurationHours, 10) === 0) {
						text += parseInt(cookDurationMinutes, 10) + minutesLetter;
					} else {
						text += parseInt(cookDurationHours, 10) + hoursLetter + ' ' + parseInt(cookDurationMinutes, 10) + minutesLetter;
					}
				}
			}

			if (recipe.servings > 1) {
				text += '\n\n' + servingsLabel;
			}

			const textArea = document.createElement('textarea');
			textArea.value = text;
			textArea.style.position = 'fixed'; // avoid scrolling to bottom
			document.body.appendChild(textArea);
			textArea.focus();
			textArea.select();

			document.execCommand('copy');

			document.body.removeChild(textArea);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	videoUrlToEmbedSrc(videoUrl: string, language: string): string {
		try {
			if (videoUrl.includes('youtube.com') || videoUrl.includes('youtu.be')) {
				const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
				const match = videoUrl.match(regExp);

				if (match && match[2].length == 11) {
					const id = match[2];

					const hl = language == 'en-US' ? 'en' : 'mk';

					return `//www.youtube.com/embed/${id}?disablekb=1&hl=${hl}&iv_load_policy=3&loop=1&modestbranding=1&rel=0`;
				}
			}
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}

		throw 'Invalid url';
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
