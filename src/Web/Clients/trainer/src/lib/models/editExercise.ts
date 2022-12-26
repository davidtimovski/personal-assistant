export class CreateExercise {
	constructor(public name: string, public sets: number) {}
}

export class UpdateExercise {
	constructor(public id: number, public name: string, public sets: number) {}
}

export class CreateAmountExercise extends CreateExercise {
	constructor(public name: string, public sets: number, public amountUnit: string) {
		super(name, sets);
	}
}

export class UpdateAmountExercise extends UpdateExercise {
	constructor(public id: number, public name: string, public sets: number, public amountUnit: string) {
		super(id, name, sets);
	}
}

export class CreateAmountX2Exercise extends CreateExercise {
	constructor(public name: string, public sets: number, public amount1Unit: string, public amount2Unit: string) {
		super(name, sets);
	}
}

export class UpdateAmountX2Exercise extends UpdateExercise {
	constructor(
		public id: number,
		public name: string,
		public sets: number,
		public amount1Unit: string,
		public amount2Unit: string
	) {
		super(id, name, sets);
	}
}
