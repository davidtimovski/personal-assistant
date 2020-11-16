import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { CategoriesService } from "services/categoriesService";
import { Category } from "models/entities/category";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

@inject(Router, CategoriesService, I18N, EventAggregator)
export class Categories {
  private categories: Array<Category>;
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
      this.categories = categories;
      this.emptyListMessage = this.i18n.tr("categories.emptyListMessage");
    });
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
