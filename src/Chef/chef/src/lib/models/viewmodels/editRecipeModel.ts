import type { EditRecipeIngredient } from './editRecipeIngredient.svelte';
import type { SharingState } from './sharingState';

export class EditRecipeModel {
	constructor(
		public id: number,
		public name: string,
		public description: string | null,
		public ingredients: EditRecipeIngredient[],
		public instructions: string | null,
		public prepDuration: string,
		public cookDuration: string,
		public servings: number,
		public imageUri: string,
		public videoUrl: string | null,
		public sharingState: SharingState,
		public userIsOwner: boolean
	) {}
}
