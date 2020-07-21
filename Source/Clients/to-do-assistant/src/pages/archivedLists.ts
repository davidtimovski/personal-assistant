import { inject, computedFrom } from "aurelia-framework";
import { ListsService } from "services/listsService";
import { ArchivedList } from "models/viewmodels/archivedList";
import { I18N } from "aurelia-i18n";

@inject(ListsService, I18N)
export class ArchivedLists {
  private archivedLists: Array<ArchivedList>;
  private iconOptions = ListsService.getIconOptions();
  private lastEditedId: number;
  private emptyListMessage: string;

  constructor(
    private readonly listsService: ListsService,
    private readonly i18n: I18N
  ) {}

  async activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
  }

  attached() {
    this.listsService
      .getAllArchived()
      .then((archivedLists: Array<ArchivedList>) => {
        this.archivedLists = archivedLists;
        this.emptyListMessage = this.i18n.tr("archivedLists.emptyListMessage");
      });
  }

  getClassFromIcon(icon: string): string {
    return this.iconOptions.find((x) => x.icon === icon).cssClass;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
