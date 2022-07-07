import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { EventAggregator } from "aurelia-event-aggregator";

import { CategoriesService } from "services/categoriesService";
import { Category } from "models/entities/category";
import { CategoryItem } from "models/viewmodels/categoryItem";
import { AppEvents } from "models/appEvents";

@autoinject
export class Categories {
  private categories: CategoryItem[];
  private lastEditedId: number;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe(AppEvents.SyncStarted, () => {
      this.syncing = true;
    });
    this.eventAggregator.subscribe(AppEvents.SyncFinished, () => {
      this.syncing = false;
    });
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  async attached() {
    const categories = await this.categoriesService.getAll();

    const categoryItems = new Array<CategoryItem>();
    for (const category of categories) {
      if (category.parentId !== null) {
        continue;
      }

      const subCategories = categories
        .filter((c) => c.parentId === category.id)
        .sort((a, b) => (a.name > b.name ? 1 : -1));
      const item = this.mapToCategoryItem(category, subCategories);

      categoryItems.push(item);
    }

    this.categories = categoryItems;
  }

  mapToCategoryItem(category: Category, subCategories: Category[]) {
    return new CategoryItem(
      category.id,
      category.name,
      category.type,
      category.generateUpcomingExpense,
      category.synced,
      subCategories.map((c: Category) => {
        return this.mapToCategoryItem(c, []);
      })
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
