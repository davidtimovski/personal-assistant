export class EditProgress {
	constructor(public exerciseId: number, public date: string) {}
}

export class EditAmountProgress extends EditProgress {
	constructor(public exerciseId: number, public date: string, public sets: AmountSet[]) {
		super(exerciseId, date);
	}
}

export class EditAmountX2Progress extends EditProgress {
	constructor(public exerciseId: number, public date: string, public sets: AmountX2Set[]) {
		super(exerciseId, date);
	}
}

export class AmountSet {
	constructor(public set: number, public amount: number) {}
}

export class AmountX2Set {
	constructor(public set: number, public amount1: number, public amount2: number) {}
}
