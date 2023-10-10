import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { ValidationResult, ValidationUtil } from '../../../../../Core/shared2/utils/validationUtils';

import { CategoriesIDBHelper } from '$lib/utils/categoriesIDBHelper';
import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { Category, CategoryType } from '$lib/models/entities/category';
import { SelectOption } from '$lib/models/viewmodels/selectOption';
import { CreateCategory, UpdateCategory } from '$lib/models/server/requests/category';
import Variables from '$lib/variables';

export class CategoriesService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new CategoriesIDBHelper();
	private readonly localStorage = new LocalStorageUtil();
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

	static validate(name: string): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(name)) {
			result.fail('name');
		}

		return result;
	}

	async create(category: Category): Promise<number> {
		try {
			const now = new Date();
			category.createdDate = category.modifiedDate = now;
			category.name = category.name.trim();

			if (category.type === CategoryType.DepositOnly) {
				category.generateUpcomingExpense = false;
			}

			if (navigator.onLine) {
				const payload = new CreateCategory(
					category.parentId,
					category.name,
					category.type,
					category.generateUpcomingExpense,
					category.isTax,
					category.createdDate,
					category.modifiedDate
				);

				category.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/categories`, {
					method: 'post',
					body: window.JSON.stringify(payload)
				});
				category.synced = true;
			}

			await this.idbHelper.create(category);

			this.localStorage.setLastSyncedNow();

			return category.id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(category: Category): Promise<void> {
		try {
			category.modifiedDate = new Date();
			category.name = category.name.trim();

			if (category.type === CategoryType.DepositOnly) {
				category.generateUpcomingExpense = false;
			}

			if (navigator.onLine) {
				const payload = new UpdateCategory(
					category.id,
					category.parentId,
					category.name,
					category.type,
					category.generateUpcomingExpense,
					category.isTax,
					category.createdDate,
					category.modifiedDate
				);

				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/categories`, {
					method: 'put',
					body: window.JSON.stringify(payload)
				});
				category.synced = true;
			} else if (await this.idbHelper.isSynced(category.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(category);

			this.localStorage.setLastSyncedNow();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/categories/${id}`, {
					method: 'delete'
				});
			} else if (await this.idbHelper.isSynced(id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.delete(id);

			this.localStorage.setLastSyncedNow();
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
