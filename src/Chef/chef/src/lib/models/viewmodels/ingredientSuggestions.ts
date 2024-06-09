export class IngredientSuggestion {
	constructor(
		public id: number,
		public taskId: number,
		public brandName: string,
		public name: string,
		public parentName: string | null,
		public unit: string,
		public unitImperial: string,
		public selected: boolean,
		public matched: boolean,
		public hasNutritionData: boolean,
		public hasPriceData: boolean,
		public isPublic: boolean,
		public children = new Array<IngredientSuggestion>()
	) {}
}

export class IngredientCategory {
	constructor(
		public name: string,
		public ingredients: IngredientSuggestion[],
		public subcategories: IngredientCategory[],
		public matched: boolean
	) {}
}

export class PublicIngredientSuggestions {
	constructor(
		public uncategorized: IngredientSuggestion[],
		public categories: IngredientCategory[]
	) {}
}

export class IngredientSuggestions {
	constructor(
		public userIngredients: IngredientSuggestion[],
		public publicIngredients: PublicIngredientSuggestions
	) {}
}
