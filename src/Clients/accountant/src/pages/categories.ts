import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { CategoriesService } from "services/categoriesService";
import { Category } from "models/entities/category";
import { CategoryItem } from "models/viewmodels/categoryItem";

@inject(Router, CategoriesService, I18N, EventAggregator)
export class Categories {
  private categories: Array<CategoryItem>;
  private lastEditedId: number;
  private emptyListMessage: string;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe("sync-started", () => {
      this.syncing = true;
    });
    this.eventAggregator.subscribe("sync-finished", () => {
      this.syncing = false;
    });
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  attached() {
    this.categoriesService.getAll().then((categories: Array<Category>) => {
      const categoryItems = new Array<CategoryItem>();
      for (const category of categories) {
        if (category.parentId !== null) {
          continue;
        }

        const subCategories = categories
          .filter(c => c.parentId === category.id)
          .sort((a, b) => a.name > b.name ? 1 : -1);
        const item = this.mapToCategoryItem(category, subCategories);

        categoryItems.push(item);
      }

      this.categories = categoryItems;
      this.emptyListMessage = this.i18n.tr("categories.emptyListMessage");
    });
  }

  mapToCategoryItem(category: Category, subCategories: Array<Category>) {
    return new CategoryItem(
      category.id, 
      category.name, 
      category.generateUpcomingExpense, 
      category.synced, 
      subCategories.map((c: Category) => { return this.mapToCategoryItem(c, []); })
    );
  }

  newCategory() {
    if (!this.syncing) {
      this.router.navigateToRoute("editCategory", { id: 0 });
    }
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
