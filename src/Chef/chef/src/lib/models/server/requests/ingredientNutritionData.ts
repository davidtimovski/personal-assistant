export class IngredientNutritionData {
	constructor(
		public readonly isSet: boolean,
		public readonly servingSize: number,
		public readonly servingSizeIsOneUnit: boolean,
		public readonly calories: number | null,
		public readonly fat: number | null,
		public readonly saturatedFat: number | null,
		public readonly carbohydrate: number | null,
		public readonly sugars: number | null,
		public readonly addedSugars: number | null,
		public readonly fiber: number | null,
		public readonly protein: number | null,
		public readonly sodium: number | null,
		public readonly cholesterol: number | null,
		public readonly vitaminA: number | null,
		public readonly vitaminC: number | null,
		public readonly vitaminD: number | null,
		public readonly calcium: number | null,
		public readonly iron: number | null,
		public readonly potassium: number | null,
		public readonly magnesium: number | null
	) {}
}
