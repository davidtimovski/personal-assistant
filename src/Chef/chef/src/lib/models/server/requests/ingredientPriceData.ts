export class IngredientPriceData {
	constructor(
		public readonly isSet: boolean,
		public readonly productSize: number,
		public readonly productSizeIsOneUnit: boolean,
		public readonly price: number | null,
		public readonly currency: string | null
	) {}
}
