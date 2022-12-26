import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import type { EditAmountProgress, EditAmountX2Progress, EditProgress } from '$lib/models/editProgress';
import Variables from '$lib/variables';

export class ProgressService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Trainer');

	async get(exerciseId: number, date: string | null = null) {
		try {
			if (!date) {
				date = DateHelper.format(new Date());
			}

			return await this.httpProxy.ajax<EditProgress>(`${Variables.urls.api}/progress/${exerciseId}/${date}`);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async createAmount(model: EditAmountProgress): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/progress/amount`, {
				method: 'post',
				body: window.JSON.stringify(model)
			});

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async createAmountX2(model: EditAmountX2Progress): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/progress/amountx2`, {
				method: 'post',
				body: window.JSON.stringify(model)
			});

			return id;
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
