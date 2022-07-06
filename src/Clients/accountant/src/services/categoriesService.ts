import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { CategoriesIDBHelper } from "../utils/categoriesIDBHelper";
import { Category, CategoryType } from "models/entities/category";
import { SelectOption } from "models/viewmodels/selectOption";
import * as environment from "../../config/environment.json";

@inject(AuthService, HttpClient, EventAggregator, CategoriesIDBHelper)
export class CategoriesService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly idbHelper: CategoriesIDBHelper
  ) {
    super(authService, httpClient, eventAggregator);
  }

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
        category.id = await this.ajax<number>("categories", {
          method: "post",
          body: json(category),
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
        await this.ajaxExecute("categories", {
          method: "put",
          body: json(category),
        });
        category.synced = true;
      } else if (await this.idbHelper.isSynced(category.id)) {
        throw "failedToFetchError";
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
        await this.ajaxExecute(`categories/${id}`, {
          method: "delete",
        });
      } else if (await this.idbHelper.isSynced(id)) {
        throw "failedToFetchError";
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
}
