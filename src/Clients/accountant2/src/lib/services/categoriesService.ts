import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { CategoriesIDBHelper } from '$lib/utils/categoriesIDBHelper';
import { Category, CategoryType } from '$lib/models/entities/category';
import { SelectOption } from '$lib/models/viewmodels/selectOption';
import Variables from '$lib/variables';

export class CategoriesService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new CategoriesIDBHelper();
	private readonly logger = new ErrorLogger('Accountant');

	getAll(): Promise<Array<Category>> {
		return this.idbHelper.getAll();
	}

	async getAllAsOptions(uncategorizedLabel: string, type: CategoryType): Promise<Array<SelectOption>> {
		try {
			const categories = await this.idbHelper.getAllAsOptions(type);

			const options = new Array<SelectOption>();
			options.push(new SelectOption(null, uncategorizedLabel));

			const selectOptions = categories.map((c) => new SelectOption(c.id, c.fullName));
			options.push(...selectOptions);

			return options;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getParentAsOptions(notSelectedLabel: string, excludeCategoryId: number): Promise<Array<SelectOption>> {
		try {
			const categories = await this.idbHelper.getParentAsOptions();

			const options = new Array<SelectOption>();
			options.push(new SelectOption(null, notSelectedLabel));

			if (excludeCategoryId === 0) {
				const selectOptions = categories.map((c) => new SelectOption(c.id, c.name));
				options.push(...selectOptions);
			} else {
				const selectOptions = categories
					.filter((c) => c.id !== excludeCategoryId)
					.map((c) => new SelectOption(c.id, c.name));

				options.push(...selectOptions);
			}

			return options;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<Category> {
		return this.idbHelper.get(id);
	}

	isParent(id: number): Promise<boolean> {
		return this.idbHelper.isParent(id);
	}

	async create(category: Category): Promise<number> {
		try {
			const now = DateHelper.adjustForTimeZone(new Date());
			category.createdDate = category.modifiedDate = now;

			if (category.type === CategoryType.DepositOnly) {
				category.generateUpcomingExpense = false;
			}

			if (navigator.onLine) {
				category.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/api/categories`, {
					method: 'post',
					body: window.JSON.stringify(category)
				});
				category.synced = true;
			}

			await this.idbHelper.create(category);

			return category.id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(category: Category): Promise<void> {
		try {
			category.modifiedDate = DateHelper.adjustForTimeZone(new Date());

			if (category.type === CategoryType.DepositOnly) {
				category.generateUpcomingExpense = false;
			}

			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/categories`, {
					method: 'put',
					body: window.JSON.stringify(category)
				});
				category.synced = true;
			} else if (await this.idbHelper.isSynced(category.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(category);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/categories/${id}`, {
					method: 'delete'
				});
			} else if (await this.idbHelper.isSynced(id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.delete(id);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	hasTransactions(id: number): Promise<boolean> {
		return this.idbHelper.hasTransactions(id);
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
