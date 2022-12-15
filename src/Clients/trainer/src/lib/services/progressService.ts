import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import type { EditAmountProgressEntry } from '$lib/models/editProgressEntry';
import Variables from '$lib/variables';

export class ProgressService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('Trainer');

	get(culture: string) {
		try {
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async createAmount(model: EditAmountProgressEntry): Promise<number> {
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

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
