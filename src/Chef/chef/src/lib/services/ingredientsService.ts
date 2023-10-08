import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';

import type { SimpleIngredient } from '$lib/models/viewmodels/simpleIngredient';
import type { EditIngredientModel } from '$lib/models/viewmodels/editIngredientModel';
import type { ViewIngredientModel } from '$lib/models/viewmodels/viewIngredientModel';
import type { TaskSuggestion } from '$lib/models/viewmodels/taskSuggestion';
import type { IngredientSuggestion, PublicIngredientSuggestions } from '$lib/models/viewmodels/ingredientSuggestions';
import type { UpdateIngredient } from '$lib/models/server/requests/updateIngredient';
import type { UpdatePublicIngredient } from '$lib/models/server/requests/updatePublicIngredient';
import Variables from '$lib/variables';

export class IngredientsService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Chef');

	getAll(): Promise<SimpleIngredient[]> {
		return this.httpProxy.ajax<SimpleIngredient[]>(`${Variables.urls.api}/ingredients`);
	}

	getForUpdate(id: number): Promise<EditIngredientModel> {
		return this.httpProxy.ajax<EditIngredientModel>(`${Variables.urls.api}/ingredients/${id}/update`);
	}

	getPublic(id: number): Promise<ViewIngredientModel> {
		return this.httpProxy.ajax<ViewIngredientModel>(`${Variables.urls.api}/ingredients/${id}/public`);
	}

	async update(dto: UpdateIngredient): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/ingredients`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async updatePublic(dto: UpdatePublicIngredient): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/ingredients/public`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/ingredients/${id}`, {
				method: 'delete'
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	getTaskSuggestions(): Promise<TaskSuggestion[]> {
		return this.httpProxy.ajax<TaskSuggestion[]>(`${Variables.urls.api}/ingredients/task-suggestions`);
	}

	getUserIngredientSuggestions(): Promise<IngredientSuggestion[]> {
		return this.httpProxy.ajax<IngredientSuggestion[]>(`${Variables.urls.api}/ingredients/user-suggestions`);
	}

	getPublicIngredientSuggestions(): Promise<PublicIngredientSuggestions> {
		return this.httpProxy.ajax<PublicIngredientSuggestions>(`${Variables.urls.api}/ingredients/public-suggestions`);
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
