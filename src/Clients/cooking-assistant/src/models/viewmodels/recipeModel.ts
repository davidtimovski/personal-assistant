import { SharingState } from "./sharingState";

export class RecipeModel {
  id: number;
  name: string;
  imageUri: string;
  ingredientsMissing: number;
  ingredientsMissingLabel: string;
  lastOpenedDate: Date;
  sharingState: SharingState;
}
