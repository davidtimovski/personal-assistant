import { EditRecipeIngredient } from "./editRecipeIngredient";
import * as environment from "../../../config/environment.json";
import { SharingState } from "./sharingState";

export class EditRecipeModel {
  public id: number;
  public name: string;
  public description: string;
  public ingredients: Array<EditRecipeIngredient>;
  public instructions: string;
  public prepDuration: string;
  public cookDuration: string;
  public servings: number;
  public imageUri: string;
  public videoUrl: string;
  public sharingState: SharingState;
  public userIsOwner: boolean;

  constructor() {
    this.id = 0;
    this.ingredients = new Array<EditRecipeIngredient>();
    this.servings = 1;
    this.imageUri = JSON.parse(<any>environment).defaultRecipeImageUri;
  }
}
