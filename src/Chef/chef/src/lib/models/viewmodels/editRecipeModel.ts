import type { EditRecipeIngredient } from './editRecipeIngredient';
import type { SharingState } from './sharingState';
import Variables from '$lib/variables';

export class EditRecipeModel {
	constructor(
		public id: number,
		public name: string,
		public description: string,
		public ingredients: EditRecipeIngredient[],
		public instructions: string,
		public prepDuration: string,
		public cookDuration: string,
		public servings: number,
		public imageUri: string,
		public videoUrl: string,
		public sharingState: SharingState,
		public userIsOwner: boolean
	) {
		this.id = 0;
		this.ingredients = new Array<EditRecipeIngredient>();
		this.servings = 1;
		this.imageUri = Variables.urls.defaultProfileImageUrl;
		this.userIsOwner = true;
	}
}
