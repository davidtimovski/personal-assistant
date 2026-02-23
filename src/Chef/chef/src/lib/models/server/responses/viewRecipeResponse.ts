import type { Ingredient } from '../../viewmodels/ingredient';
import type { SharingState } from '../../sharingState';

export class ViewRecipeResponse {
	constructor(
		public id: number,
		public name: string,
		public description: string,
		public ingredients: Array<Ingredient>,
		public instructions: string,
		public prepDuration: string,
		public cookDuration: string,
		public servings: number,
		public imageUri: string,
		public videoUrl: string,
		public lastOpenedDate: Date,
		public nutritionSummary: NutritionSummary,
		public costSummary: CostSummary,
		public sharingState: SharingState,
		public imageWidth: number,
		public imageHeight: number
	) {
		this.ingredients = ingredients || new Array<Ingredient>();
	}
}

export class NutritionSummary {
	constructor(
		public isSet: boolean,
		public servingSize: number,
		public servingSizeIsOneUnit: boolean,
		public calories: number,
		public caloriesFromDaily: number,
		public caloriesFromDailyGrade: string,
		public fat: number,
		public fatFromDaily: number,
		public saturatedFat: number,
		public saturatedFatFromDaily: number,
		public saturatedFatFromDailyGrade: string,
		public carbohydrate: number,
		public carbohydrateFromDaily: number,
		public carbohydrateFromDailyGrade: string,
		public sugars: number,
		public sugarsFromDaily: number,
		public sugarsFromDailyGrade: number,
		public addedSugars: number,
		public addedSugarsFromDaily: number,
		public addedSugarsFromDailyGrade: string,
		public fiber: number,
		public fiberFromDaily: number,
		public fiberFromDailyGrade: string,
		public protein: number,
		public proteinFromDaily: number,
		public proteinFromDailyGrade: string,
		public sodium: number,
		public sodiumFromDaily: number,
		public sodiumFromDailyGrade: string,
		public cholesterol: number,
		public cholesterolFromDaily: number,
		public cholesterolFromDailyGrade: string,
		public vitaminA: number,
		public vitaminAFromDaily: number,
		public vitaminAFromDailyGrade: string,
		public vitaminC: number,
		public vitaminCFromDaily: number,
		public vitaminCFromDailyGrade: string,
		public vitaminD: number,
		public vitaminDFromDaily: number,
		public vitaminDFromDailyGrade: string,
		public calcium: number,
		public calciumFromDaily: number,
		public calciumFromDailyGrade: string,
		public iron: number,
		public ironFromDaily: number,
		public ironFromDailyGrade: string,
		public potassium: number,
		public potassiumFromDaily: number,
		public potassiumFromDailyGrade: string,
		public magnesium: number,
		public magnesiumFromDaily: number,
		public magnesiumFromDailyGrade: string
	) {}
}

export class CostSummary {
	constructor(
		public isSet: boolean,
		public productSize: number,
		public productSizeIsOneUnit: boolean,
		public cost: number,
		public costPerServing: number,
		public currency: string
	) {}
}
