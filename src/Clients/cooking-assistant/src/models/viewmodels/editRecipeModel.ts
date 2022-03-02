import { EditRecipeIngredient } from "./editRecipeIngredient";
import * as environment from "../../../config/environment.json";
import { SharingState } from "./sharingState";

export class EditRecipeModel {
  id: number;
  name: string;
  description: string;
  ingredients: Array<EditRecipeIngredient>;
  instructions: string;
  prepDuration: string;
  cookDuration: string;
  servings: number;
  imageUri: string;
  videoUrl: string;
  sharingState: SharingState;
  userIsOwner: boolean;

  constructor() {
    this.id = 0;
    this.ingredients = new Array<EditRecipeIngredient>();
    this.servings = 1;
    this.imageUri = JSON.parse(<any>environment).defaultRecipeImageUri;
    this.userIsOwner = true;
  }
}
