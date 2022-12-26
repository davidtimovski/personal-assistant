export class Exercise {
	constructor(public id: number, public name: string, public sets: number, public ofType: ExerciseType) {}
}

export class ExerciseAmount extends Exercise {
	constructor(public id: number, public name: string, public sets: number, public amountUnit: string) {
		super(id, name, sets, ExerciseType.Amount);
	}
}

export class ExerciseAmountX2 extends Exercise {
	constructor(
		public id: number,
		public name: string,
		public sets: number,
		public amount1Unit: string,
		public amount2Unit: string
	) {
		super(id, name, sets, ExerciseType.AmountX2);
	}
}

export enum ExerciseType {
	Amount,
	AmountX2
}
