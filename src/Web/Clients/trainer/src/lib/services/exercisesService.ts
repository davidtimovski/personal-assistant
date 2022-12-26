import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import type { Exercise } from '$lib/models/exercise';
import type {
	CreateAmountExercise,
	UpdateAmountExercise,
	CreateAmountX2Exercise,
	UpdateAmountX2Exercise
} from '$lib/models/editExercise';
import Variables from '$lib/variables';

export class ExercisesService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Trainer');

	async getAll() {
		try {
			return await this.httpProxy.ajax<Exercise[]>(`${Variables.urls.api}/exercises`);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async get(id: number) {
		try {
			return await this.httpProxy.ajax<Exercise>(`${Variables.urls.api}/exercises/${id}`);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async createAmount(model: CreateAmountExercise): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/exercises/amount`, {
				method: 'post',
				body: window.JSON.stringify(model)
			});

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async updateAmount(model: UpdateAmountExercise) {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/exercises/amount`, {
				method: 'put',
				body: window.JSON.stringify(model)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async createAmountX2(model: CreateAmountX2Exercise): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/exercises/amountx2`, {
				method: 'post',
				body: window.JSON.stringify(model)
			});

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async updateAmountX2(model: UpdateAmountX2Exercise) {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/exercises/amountx2`, {
				method: 'put',
				body: window.JSON.stringify(model)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
