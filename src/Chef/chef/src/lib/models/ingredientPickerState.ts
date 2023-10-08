export enum IngredientPickEvent {
	Unselected,
	Selected,
	Added,
	Reset
}

export class IngredientPickerState {
	constructor(public event: IngredientPickEvent | null, public data: any) {}
}
